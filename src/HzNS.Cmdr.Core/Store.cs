using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HzNS.Cmdr.Exception;
using HzNS.Cmdr.Internal;
using HzNS.Cmdr.Painter;
using HzNS.Cmdr.Tool.Ext;
using HzNS.Cmdr.Tool.ObjectCloner;
using Newtonsoft.Json.Linq;

namespace HzNS.Cmdr
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
    [SuppressMessage("ReSharper", "InvertIf")]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Store
    {
        private Slot Root { get; set; } = new Slot();

        public string[] Prefixes { get; set; } = {"app"};

        private readonly SortedSlots _fastMap = new SortedSlots();


        #region Dump()

        public void Dump(Action<string, string> print)
        {
            dumpTo(print, Root);
        }

        public void dumpTo(Action<string, string> print, Slot? node, int level = 0, params string[] parts)
        {
            if (node == null) return;

            foreach (var (key, slot) in node.Children)
            {
                var a = parts.ToList();
                a.Add(key);
                dumpTo(print, slot, level + 1, a.ToArray());
            }

            foreach (var (key, val) in node.Values)
            {
                var a = parts.ToList();
                a.Add(key);
                var path = string.Join(".", a.ToArray());
                print($"  {path,-45}", DefaultPainter.ColorDesc);
                print($"{val?.ToStringEx()}", DefaultPainter.ColorNormal);
                print($" ({val?.GetType()})\n", DefaultPainter.ColorDesc);
            }
        }

        #endregion


        public IEnumerable<string> WrapKeys(IEnumerable<string> keys)
        {
            return Prefixes.Concat(keys);
        }


        public IEnumerable<string> WrapKeys(string dottedKey)
        {
            return Prefixes.Concat(dottedKey.Split('.'));
        }


        #region Get(), GetAs<T>()

        public object? Get(string key, object? defaultValues = null)
        {
            var (slot, vk) = FindByDottedKey(key);
            return slot?.Values[vk] ?? defaultValues;
        }

        public T GetAs<T>(string key, params T[] defaultValues)
        {
            var (slot, vk) = FindByDottedKey(key);
            var v = slot?.Values[vk];
#pragma warning disable CS8653
            if (v == null) return defaultValues.Length > 0 ? defaultValues[^1] : default(T);
#pragma warning restore CS8653

            if (typeof(T) == v.GetType())
                return (T) v;

            if (typeof(T) == typeof(bool))
            {
                var bv = v.ToBool();
                return (T) Convert.ChangeType(bv, typeof(T));
            }

            if (Cmdr.Instance.EnableAutoBoxingWhenExtracting)
                return (T) Convert.ChangeType(v, typeof(T));

            throw new CmdrException(
                $"type info mismatch, cannot get value from option store. expect: {typeof(T)}, the underlying data type is: {v.GetType()}.");
        }

        public SlotEntries GetAsMap(string key)
        {
            var dict = new SlotEntries();
            var (slot, _) = FindByDottedKey(key);
            var stack = new List<SlotEntries>();
            // var mn = dict;
            var top = new SlotEntries();
            dict.Add("TOP", top);
            stack.Add(top);
            stack.Add(dict);

            if (slot != null)
                walkForSlot(slot, 0,
                    nodesWalker: (owner, childKey, childSlot, level, index) =>
                    {
                        switch (index)
                        {
                            case StatusOrIndex.ENTERING:
                                var newTop = new SlotEntries();
                                stack[0].Add(childKey, newTop);
                                stack.Insert(0, newTop);
                                break;
                            case StatusOrIndex.LEAVING:
                                stack.Remove(stack[0]);
                                break;
                            default:
                                return true;
                        }

                        return true;
                    },
                    valuesWalker: (owner, valueKey, value, level, index) =>
                    {
                        stack[0].Add(valueKey, value);
                        return true;
                    });
            return dict;
        }

        #endregion

        #region walkForSlot implementation

        public enum StatusOrIndex
        {
            ENTERING = -1,
            LEAVING = -2,
        }

        private static void walkForSlot(Slot parent, int level,
            Func<Slot /*owner*/, string /*childKey*/, Slot /*childSlot*/,
                int /*level*/, StatusOrIndex /*index*/, bool /*goAhead*/>? nodesWalker = null,
            Func<Slot /*owner*/, string /*valueKey*/, object? /*value*/,
                int /*level*/, int /*index*/, bool /*goAhead*/>? valuesWalker = null)
        {
            var i = 0;
            foreach (var (key, value) in parent.Values)
            {
                if (valuesWalker?.Invoke(parent, key, value, level, i) == false)
                    return;
                i++;
            }

            i = 0;
            foreach (var (key, childSlot) in parent.Children)
            {
                nodesWalker?.Invoke(parent, key, Slot.Empty, level, StatusOrIndex.ENTERING);
                if (nodesWalker?.Invoke(parent, key, childSlot, level, (StatusOrIndex) i) == false)
                    return;
                walkForSlot(childSlot, level + 1, nodesWalker, valuesWalker);
                nodesWalker?.Invoke(parent, key, Slot.Empty, level, StatusOrIndex.LEAVING);
                i++;
            }
        }

        #endregion

        public void WalkForSlot(Slot? parent)
        {
            walkForSlot(parent ?? Root, 0);
        }


        #region Set<T>(), SetByKeys<T>()

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns>the old value</returns>
        public object? Set<T>(string key, params T[] val)
        {
            // if (val == null)
            // {
            //     Delete(key, true);
            //     return;
            // }

            var parts = WrapKeys(key);
            return setValue(parts, Root, val);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="val"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns>the old value</returns>
        public object? SetByKeys<T>(IEnumerable<string> keys, params T[] val)
        {
            var parts = WrapKeys(keys);
            return setValue(parts, Root, val);
        }

        /// <summary>
        /// no 
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="val"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal object? SetByKeysInternal<T>(IEnumerable<string> keys, T val)
        {
            var parts = WrapKeys(keys);
            return setValue(parts, Root, val);
        }


        private static object? setValue<T>(IEnumerable<string> parts, Slot? node, params T[] val)
        {
            while (node != null)
            {
                var enumerable = parts as string[] ?? parts.ToArray();
                if (enumerable.Length < 1) return null;
                if (enumerable.Length == 1)
                {
                    var key = enumerable[0];
                    var yes = node.Values.ContainsKey(key);
                    var dv = yes
                        ? node.Values[key]
                        : (val.Length > 0 ? val[0] : default);
                    if (!yes)
                    {
                        if (val.Length > 0 && val[0] is JArray ja)
                        {
                            // var old = node.Values[key]?.DeepClone();
                            var remainsParts = enumerable.Skip(1).ToArray();
                            foreach (var token in ja)
                            {
                                setIt(node, key, remainsParts, token, true, true);
                            }
                        }
                        else
                        {
                            node.Values[key] = val.Length == 1 ? dv : val;
                        }

                        return null;
                    }
                    else
                    {
                        var old = node.Values[key]?.DeepClone();
                        if (val.Length > 0 && val[0] is JArray ja)
                        {
                            // var old = node.Values[key]?.DeepClone();
                            var remainsParts = enumerable.Skip(1).ToArray();
                            foreach (var token in ja)
                            {
                                setIt(node, key, remainsParts, token, true, true);
                            }
                        }
                        else
                        {
                            if (setValueInternal(dv, key, enumerable.Skip(1).ToArray(), node, true, val))
                                OnSetHandler?.Invoke(node, key, old, node.Values[key]);
                        }

                        return old;
                    }
                }

                var part = enumerable[0];
                parts = enumerable.Skip(1);
                if (!node.Children.ContainsKey(part))
                {
                    var slot = new Slot();
                    node.Children.Add(part, slot);
                }

                node = node.Children[part];
            }

            return null;
        }

        private static void setIt(Slot node, string key, IEnumerable<string> remainsParts,
            JToken token, bool isArray = false, bool appendToArray = false)
        {
            object? old;
            node.Values.TryGetValue(key, out old);

            switch (token.Type)
            {
                case JTokenType.Boolean:
                {
                    var v = token.ToObject<bool>();
                    object av = v;
                    if (isArray) av = new bool[] { };
                    if (setValueInternal(av, key, remainsParts, node, appendToArray, v))
                        OnSetHandler?.Invoke(node, key, old, node.Values[key]);
                    break;
                }
                case JTokenType.Date:
                {
                    var v = token.ToObject<DateTime>();
                    object av = v;
                    if (isArray) av = new DateTime[] { };
                    if (setValueInternal(av, key, remainsParts, node, appendToArray, v))
                        OnSetHandler?.Invoke(node, key, old, node.Values[key]);
                    break;
                }
                case JTokenType.TimeSpan:
                {
                    var v = token.ToObject<TimeSpan>();
                    object av = v;
                    if (isArray) av = new TimeSpan[] { };
                    if (setValueInternal(av, key, remainsParts, node, appendToArray, v))
                        OnSetHandler?.Invoke(node, key, old, node.Values[key]);
                    break;
                }
                case JTokenType.Float:
                {
                    var v = token.ToObject<double>();
                    object av = v;
                    if (isArray) av = new double[] { };
                    if (setValueInternal(av, key, remainsParts, node, appendToArray, v))
                        OnSetHandler?.Invoke(node, key, old, node.Values[key]);
                    break;
                }
                case JTokenType.Integer:
                {
                    var v = token.ToObject<long>();
                    object av = v;
                    if (isArray) av = new long[] { };
                    if (setValueInternal(av, key, remainsParts, node, appendToArray, v))
                        OnSetHandler?.Invoke(node, key, old, node.Values[key]);
                    break;
                }
                case JTokenType.String:
                {
                    var v = token.ToObject<string>();
                    object av = v;
                    if (isArray) av = new string[] { };
                    if (setValueInternal(av, key, remainsParts, node, appendToArray, v))
                        OnSetHandler?.Invoke(node, key, old, node.Values[key]);
                    break;
                }
                case JTokenType.None:
                case JTokenType.Undefined:
                case JTokenType.Null:
                    break;
                case JTokenType.Object:
                    break;
                case JTokenType.Array:
                    break;
                case JTokenType.Constructor:
                    break;
                case JTokenType.Property:
                    break;
                case JTokenType.Comment:
                    break;
                case JTokenType.Raw:
                    break;
                case JTokenType.Bytes:
                    break;
                case JTokenType.Guid:
                    break;
                case JTokenType.Uri:
                    break;
                default:
                {
                    var v = token.ToObject<object>();
                    // ReSharper disable once SuggestVarOrType_SimpleTypes
                    object av = v;
                    if (isArray) av = new object[] { };
                    if (setValueInternal(av, key, remainsParts, node, appendToArray, v))
                        OnSetHandler?.Invoke(node, key, old, node.Values[key]);
                    break;
                }
            }
        }


        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public static Action<Slot, string, object?, object?>? OnSetHandler { get; set; }


        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private static bool setValueInternal<T>(object? dv, string key,
            IEnumerable<string>? parts, Slot node, bool appendToArray = false, params T[] val)
        {
            // var old = node.Values[key];
            switch (dv)
            {
                #region scalars

                case bool _:
                    foreach (var v in val)
                    {
                        node.Values[key] = v;
                    }

                    return true;

                case string _:
                    foreach (var v in val)
                    {
                        node.Values[key] = v;
                    }

                    return true;

                case short _:
                    foreach (var v in val)
                    {
                        node.Values[key] = v;
                    }

                    return true;
                case int _:
                    foreach (var v in val)
                    {
                        node.Values[key] = v;
                    }

                    return true;
                case long _:
                    foreach (var v in val)
                    {
                        node.Values[key] = v;
                    }

                    return true;

                case float _:
                    foreach (var v in val)
                    {
                        node.Values[key] = v;
                    }

                    return true;
                case double _:
                    foreach (var v in val)
                    {
                        node.Values[key] = v;
                    }

                    return true;
                case decimal _:
                    foreach (var v in val)
                    {
                        node.Values[key] = v;
                    }

                    return true;

                #endregion

                case DateTime _:
                    foreach (var v in val)
                    {
                        node.Values[key] = v;
                    }

                    return true;
                case TimeSpan _:
                    foreach (var v in val)
                    {
                        node.Values[key] = v;
                    }

                    return true;
                case DateTimeOffset _:
                    foreach (var v in val)
                    {
                        node.Values[key] = v;
                    }

                    return true;

                #region scalar array

                case string[] v:
                    foreach (var it in val)
                    {
                        appendIts(node, key, v, toT<string>(it), appendToArray);
                    }

                    return true;

                case bool[] v:
                    foreach (var it in val)
                    {
                        appendIts(node, key, v, toT<bool>(it), appendToArray);
                    }

                    return true;

                case short[] v:
                    foreach (var it in val)
                    {
                        appendIts(node, key, v, toT<short>(it), appendToArray);
                    }

                    return true;
                case int[] v:
                    foreach (var it in val)
                    {
                        appendIts(node, key, v, toT<int>(it), appendToArray);
                    }

                    return true;
                case long[] v:
                    foreach (var it in val)
                    {
                        appendIts(node, key, v, toT<long>(it), appendToArray);
                    }

                    return true;

                case float[] v:
                    foreach (var it in val)
                    {
                        appendIts(node, key, v, toT<float>(it), appendToArray);
                    }

                    return true;
                case double[] v:
                    foreach (var it in val)
                    {
                        appendIts(node, key, v, toT<double>(it), appendToArray);
                    }

                    return true;
                case decimal[] v:
                    foreach (var it in val)
                    {
                        appendIts(node, key, v, toT<decimal>(it), appendToArray);
                    }

                    return true;

                #endregion
            }

            Console.WriteLine("1_1");
            Cmdr.Instance.Logger?.logDebug(
                $"[W][setter on {dv?.GetType()}, dv={dv}, new val={val}]: key = {parts?.ToStringEx()}");
            return false;
        }

        private static T toT<T>(object? it)
        {
#pragma warning disable CS8602, CS8653
            var tv = default(T);
#pragma warning restore CS8602, CS8653
            if (it is T tst) tv = tst;
            else
            {
                var t = Convert.ChangeType(it, typeof(T));
                if (t is T ts) tv = ts;
            }

            return tv;
        }

        private static void appendIts<T>(Slot node, string key, // IEnumerable<string>? parts,
            T[] v, T val, bool appendToArray = false)
        {
            if (appendToArray == false)
                Array.Clear(v, 0, v.Length);
            else if (node.Values.ContainsKey((key)))
            {
                var tv = Convert.ChangeType(node.Values[key], typeof(T[]));
                if (tv == null)
#pragma warning disable CS8600
                    v = default(T[]);
#pragma warning restore CS8600
                else v = (T[]) tv;
            }

// #pragma warning disable CS8619
            // ReSharper disable once LoopCanBeConvertedToQuery
            //foreach (var value in val)
            //{
            //    v = v.Append(value).ToArray();
            //}
            v = v.Append(val).ToArray();
// #pragma warning restore CS8619

            node.Values[key] = v;
        }

        #endregion


        public void Delete(string key, bool recursive = true)
        {
            //
        }

        public bool HasKeys(IEnumerable<string> keys)
        {
            var (ok, _, _) = hasKeys(WrapKeys(keys), Root);
            return ok;
        }

        public bool HasDottedKey(string dottedKey)
        {
            var (ok, _, _) = hasKeys(WrapKeys(dottedKey), Root);
            return ok;
        }

        public (Slot?, string valueKey) FindByKeys(IEnumerable<string> keys)
        {
            var (_, s, valueKey) = hasKeys(WrapKeys(keys), Root);
            return (s, valueKey);
        }

        public (Slot?, string valueKey) FindByDottedKey(string dottedKey)
        {
            var (_, s, valueKey) = hasKeys(WrapKeys(dottedKey), Root);
            return (s, valueKey);
        }


        private static string lastDot(string s)
        {
            var a = s.Split('.');
            return a.Length > 0 ? a[^1] : string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parts"></param>
        /// <param name="node"></param>
        /// <returns>
        /// var (found, slotNode, valueKey):
        /// when matched slot found:
        ///  1. valueKey is empty: just a slot. For example: "tags.mode" => true, slotNode, string.Empty | means that: "mode" sub-command of "tag" command
        ///  2. valueKey is valid string: a value entry in the returned slot. For example: "tags.mode.addr" => true, slotNode, "addr" | means that: "--addr" in "tags mode" sub-command
        /// </returns>
        private (bool, Slot?, string valueKey) hasKeys(IEnumerable<string> parts, Slot? node)
        {
            var enumerable = parts as string[] ?? parts.ToArray();
            var path = string.Join('.', enumerable);

            if (_fastMap.ContainsKey(path)) return (true, _fastMap[path], lastDot(path));

            while (node != null)
            {
                if (enumerable.Length < 1) return (false, null, string.Empty);
                if (enumerable.Length == 1)
                {
                    var key = enumerable[0];
                    var yes = node.Values.ContainsKey(key);
                    if (!yes)
                    {
                        yes = node.Children.ContainsKey(key);
                        if (!yes)
                            return (false, null, key);
                        return (true, node, string.Empty);
                    }

                    // if (_fastMap.ContainsKey(path)) ;
                    _fastMap.Add(path, node);
                    return (true, node, key);
                }

                var part = enumerable[0];
                enumerable = enumerable.Skip(1).ToArray();
                if (!node.Children.ContainsKey(part))
                {
                    return (false, null, part);
                }

                node = node.Children[part];
            }


            // if (node == null) return false;
            //
            // foreach (var (key, slot) in node.Children)
            // {
            //     var a = parts.ToList();
            //     a.Add(key);
            //     dumpTo(tw, slot, level + 1, a.ToArray());
            // }
            //
            // foreach (var (key, val) in node.Values)
            // {
            //     var a = parts.ToList();
            //     a.Add(key);
            //     var path = string.Join(".", a.ToArray());
            //     tw.WriteLineAsync($"{path,-45}{val}");
            // }

            return (false, null, string.Empty);
        }


        #region Singleton Pattern

        private Store()
        {
        }

        // ReSharper disable once RedundantDefaultMemberInitializer
        private static Store _instance = null!;

        // ReSharper disable once InconsistentNaming
        private static readonly object _lock = new object();

        [SuppressMessage("ReSharper", "UseObjectOrCollectionInitializer")]
        [SuppressMessage("ReSharper", "InvertIf")]
        // ReSharper disable once UnusedMember.Global
        public static Store Instance
        {
            get
            {
                // Re|Sharper disable InvertIf
                if (_instance == null)
                    lock (_lock)
                    {
                        if (_instance == null) _instance = new Store();
                    }
                // Re|Sharper restore InvertIf

                return _instance;
            }
        }

        #endregion
    }

    // public class Entry<T>
    // {
    //     public string Key { get; set; }
    //     public T Value { get; set; }
    // }
}