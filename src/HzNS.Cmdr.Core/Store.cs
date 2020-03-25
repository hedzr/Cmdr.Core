using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HzNS.Cmdr.Exception;
using HzNS.Cmdr.Painter;
using HzNS.Cmdr.Tool.Ext;
using HzNS.Cmdr.Tool.ObjectCloner;

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
                print($"{val?.ToStringEx()}\n", DefaultPainter.ColorNormal);
            }
        }

        
        public IEnumerable<string> WrapKeys(IEnumerable<string> keys)
        {
            return Prefixes.Concat(keys);
        }


        public IEnumerable<string> WrapKeys(string dottedKey)
        {
            return Prefixes.Concat(dottedKey.Split('.'));
        }

        
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
                return (T)Convert.ChangeType(bv, typeof(T));
            }
            
            if(Cmdr.Instance.EnableAutoBoxingWhenExtracting)
                return (T)Convert.ChangeType(v, typeof(T));
            
            throw new CmdrException(
                $"type info mismatch, cannot get value from option store. expect: {typeof(T)}, the underlying data type is: {v.GetType()}.");
        }

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
                        : (val.Length > 0 ? val[0] : (object?) true);
                    if (!yes)
                    {
                        node.Values[key] = val.Length == 1 ? dv : val;
                        return null;
                    }

                    var old = node.Values[key]?.DeepClone();
                    setValueInternal(dv, key, enumerable.Skip(1).ToArray(), node, val);
                    return old;
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

        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private static void setValueInternal<T>(object? dv, string key,
            IEnumerable<string>? parts, Slot node, params T[] val)
        {
            switch (dv)
            {
                case bool _:
                    foreach (var v in val)
                    {
                        node.Values[key] = v;
                    }

                    return;
                case string _:
                    foreach (var v in val)
                    {
                        node.Values[key] = v;
                    }

                    return;
                case string[] v:
                    if (val.Length == 0)
                        // v = new string[] { };
                        Array.Clear(v, 0, v.Length);

#pragma warning disable CS8619
                    // ReSharper disable once LoopCanBeConvertedToQuery
                    foreach (var value in val)
                    {
                        v = v.Append(value as string).ToArray();
                    }
#pragma warning restore CS8619

                    node.Values[key] = v;
                    return;
            }

            Console.WriteLine("1_1");
        }


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