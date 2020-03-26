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

        private readonly SortedDictionary<string, Slot> _fastMap = new SortedDictionary<string, Slot>();


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

        #endregion


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

        private static void setIt(Slot node, string key, IEnumerable<string> remainsParts, JToken token,
            bool isArray = false, bool appendToArray = false)
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
                        var z = it is string s ? s : Convert.ChangeType(it, typeof(string)) as string ?? string.Empty;
                        appendIts(node, key, v, z, appendToArray);
                    }

                    return true;

                case bool[] v:
                    foreach (var it in val)
                    {
                        appendIts(node, key, v, it is bool s ? s : (bool?) Convert.ChangeType(it, typeof(bool)) ?? false,
                            appendToArray);
                    }

                    return true;

                case short[] v:
                    foreach (var it in val)
                    {
                        appendIts(node, key, v, it is short s ? s : (short) Convert.ChangeType(it, typeof(short)),
                            appendToArray);
                    }

                    return true;
                case int[] v:
                    foreach (var it in val)
                    {
                        appendIts(node, key, v, it is int s ? s : (int) Convert.ChangeType(it, typeof(int)),
                            appendToArray);
                    }

                    return true;
                case long[] v:
                    foreach (var it in val)
                    {
                        appendIts(node, key, v, it is long s ? s : (long) Convert.ChangeType(it, typeof(long)),
                            appendToArray);
                    }

                    return true;

                case float[] v:
                    foreach (var it in val)
                    {
                        appendIts(node, key, v, it is float s ? s : (float) Convert.ChangeType(it, typeof(float)),
                            appendToArray);
                    }

                    return true;
                case double[] v:
                    foreach (var it in val)
                    {
                        appendIts(node, key, v, it is double s ? s : (double) Convert.ChangeType(it, typeof(double)),
                            appendToArray);
                    }

                    return true;
                case decimal[] v:
                    foreach (var it in val)
                    {
                        appendIts(node, key, v, it is decimal s ? s : (decimal) Convert.ChangeType(it, typeof(decimal)),
                            appendToArray);
                    }

                    return true;

                #endregion
            }

            Console.WriteLine("1_1");
            Cmdr.Instance.Logger?.logDebug(
                $"[W][setter on {dv?.GetType()}, dv={dv}, new val={val}]: key = {parts?.ToStringEx()}");
            return false;
        }

        private static void appendIts<T>(Slot node, string key, // IEnumerable<string>? parts,
            T[] v, T val, bool appendToArray = false)
        {
            if (appendToArray == false)
                Array.Clear(v, 0, v.Length);
            else if (node.Values.ContainsKey((key)))
                v = (T[]) Convert.ChangeType(node.Values[key], typeof(T[]));

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
                        return (false, null, key);
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

    public class Slot
    {
        public Slot()
        {
            Children = new Dictionary<string, Slot>();
            Values = new Dictionary<string, object?>();
        }

        // ReSharper disable once CollectionNeverUpdated.Global
        public Dictionary<string, Slot> Children { get; }
        public Dictionary<string, object?> Values { get; }
    }

    // public class Entry<T>
    // {
    //     public string Key { get; set; }
    //     public T Value { get; set; }
    // }
}