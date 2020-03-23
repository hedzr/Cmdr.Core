using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace HzNS.Cmdr.Store
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

        public void Dump()
        {
            dumpTo(Console.Out, Root);
        }

        public void dumpTo(TextWriter tw, Slot? node, int level = 0, params string[] parts)
        {
            if (node == null) return;

            foreach (var (key, slot) in node.Children)
            {
                var a = parts.ToList();
                a.Add(key);
                dumpTo(tw, slot, level + 1, a.ToArray());
            }

            foreach (var (key, val) in node.Values)
            {
                var a = parts.ToList();
                a.Add(key);
                var path = string.Join(".", a.ToArray());
                tw.WriteLineAsync($"{path,-45}{val}");
            }
        }

        public void Set<T>(string key, params T[] val)
        {
            // if (val == null)
            // {
            //     Delete(key, true);
            //     return;
            // }

            var parts = Prefixes.Concat(key.Split('.'));
            setValue(parts, Root, val);
        }


        private static void setValue<T>(IEnumerable<string> parts, Slot? node, params T[] val)
        {
            while (node != null)
            {
                var enumerable = parts as string[] ?? parts.ToArray();
                if (enumerable.Length < 1) return;
                if (enumerable.Length == 1)
                {
                    var key = enumerable[0];
                    var yes = node.Values.ContainsKey(key);
                    var dv = yes
                        ? node.Values[key]
                        : (val.Length > 0 ? val[0] : (object?) true);
                    if (!yes)
                    {
                        node.Values[key] = dv;
                        return;
                    }

                    setValueInternal(dv, key, enumerable.Skip(1).ToArray(), node, val);
                    return;
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
        }

        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private static void setValueInternal<T>(object? dv, string key, IEnumerable<string>? parts, Slot node,
            params T[] val)
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
                        Array.Clear(v, 0, v.Length);

                    foreach (var value in val)
                    {
                        v.Append(value as string);
                    }

                    return;
            }

            Console.WriteLine("1_1");
        }


        public void Delete(string key, bool recursive = true)
        {
            //
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