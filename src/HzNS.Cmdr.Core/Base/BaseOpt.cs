#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace HzNS.Cmdr.Base
{
    [SuppressMessage("ReSharper", "ConvertToAutoPropertyWhenPossible")]
    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public abstract class BaseOpt : IBaseOpt
    {
        // ReSharper disable once PublicConstructorInAbstractClass
        public BaseOpt()
        {
        }

        // ReSharper disable once PublicConstructorInAbstractClass
        public BaseOpt(string shortTitle, string longTitle, string[] aliases, string description,
            string descriptionLong, string examples)
        {
            Short = shortTitle;
            Long = longTitle;
            Aliases = aliases;
            Description = description;
            DescriptionLong = descriptionLong;
            Examples = examples;
        }


        public string Long { get; set; } = "";
        public string Short { get; set; } = "";
        public string[] Aliases { get; set; } = { };
        public string Description { get; set; } = "";
        public string DescriptionLong { get; set; } = "";
        public string Examples { get; set; } = "";
        public string Group { get; set; } = "";
        public bool Hidden { get; set; } = false;
        public string[] EnvVars { get; set; } = { };

        public Func<IBaseWorker, IBaseOpt, IEnumerable<string>, bool>? PreAction { get; set; }
        public Action<IBaseWorker, IBaseOpt, IEnumerable<string>>? PostAction { get; set; }
        public Action<IBaseWorker, IBaseOpt, IEnumerable<string>>? Action { get; set; }
        public Action<IBaseWorker, IBaseOpt, object?, object?>? OnSet { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public ICommand? Owner { get; set; } = null;

        public IRootCommand? FindRoot()
        {
            // ReSharper disable once UseNullPropagation
            // ReSharper disable once ConvertIfStatementToReturnStatement
            // if (Owner == null) return null;
            // return Owner.FindRoot();

            var o = this is ICommand ? (ICommand?) this : Owner;
            while (o?.Owner != null && o.Owner != o) o = o?.Owner;
            return (IRootCommand?) o;
        }

        public IFlag? FindFlag(string dottedKey, IBaseOpt? from = null)
        {
            var r = from ?? FindRoot();
            return r?.FindFlag(dottedKey);
        }

        public bool Walk(ICommand? from = null,
            Func<ICommand, ICommand, int, bool>? commandsWatcher = null,
            Func<ICommand, IFlag, int, bool>? flagsWatcher = null)
        {
            var r = from ?? FindRoot();
            return r?.Walk(r, commandsWatcher, flagsWatcher) ?? false;
        }

        public bool Match(string s, bool isLongOpt = false, bool aliasAsLong = true)
        {
            if (isLongOpt)
            {
                // ReSharper disable once InvertIf
                if (aliasAsLong)
                    if (Aliases.Any(title => s == title))
                        return true;
                return s == Long;
            }
            else
            {
                if (s == Short) return true;
            }

            return false;
        }

        public bool Match(string str, int pos, int len, bool isLong = false, bool aliasAsLong = true)
        {
            var s = str.Substring(pos, len);
            return Match(s, isLong, aliasAsLong);
        }

        // ReSharper disable once InconsistentNaming
        private static bool equals(ref string matchingInputFragment, string input, int pos,
            bool enableCmdrGreedyLongFlag, bool incrementalGreedy, params string[] a)
        {
            foreach (var it in a)
            {
                if (string.IsNullOrWhiteSpace(it)) continue;
                if (matchingInputFragment == it) return true;


                // ReSharper disable once InvertIf
                if (pos + it.Length <= input.Length)
                {
                    var st = string.Empty;
                    if (enableCmdrGreedyLongFlag)
                    {
                        if (incrementalGreedy)
                        {
                            if (matchingInputFragment.Length > it.Length)
                                st = input.Substring(pos, it.Length);
                        }
                        else
                        {
                            if (matchingInputFragment.Length > it.Length)
                                st = input.Substring(input.Length - it.Length, it.Length);
                        }
                    }
                    else
                    {
                        if (matchingInputFragment.Length < it.Length)
                            st = input.Substring(pos, it.Length);
                    }

                    // ReSharper disable once InvertIf
                    if (st == it)
                    {
                        matchingInputFragment = st;
                        return true;
                    }
                }
            }

            return false;
        }

        public bool Match(ref string s, string input, int pos, bool isLong = false, bool aliasAsLong = true,
            bool enableCmdrGreedyLongFlag = false, bool incremental = true)
        {
            if (isLong)
            {
                // ReSharper disable once InvertIf
                if (aliasAsLong)
                {
                    if (equals(ref s, input, pos, enableCmdrGreedyLongFlag, incremental, Aliases))
                        return true;
                }

                if (equals(ref s, input, pos, enableCmdrGreedyLongFlag, incremental, Long))
                    return true;
            }
            else
            {
                if (equals(ref s, input, pos, enableCmdrGreedyLongFlag, incremental, Short))
                    return true;
            }

            return false;
        }

        public IEnumerable<string> ToKeys()
        {
            var list = new List<string>();
            var t = this;
            do
            {
                if (!string.IsNullOrWhiteSpace(t.Long))
                    list.Insert(0, t.Long);
#pragma warning disable CS8600
                if (t.Owner != t)
                    t = t.Owner as BaseOpt;
#pragma warning restore CS8600
            } while (t != null);

            return list;
        }

        public string ToDottedKey()
        {
            var list = ToKeys();
            return string.Join('.', list);
        }
    }
}