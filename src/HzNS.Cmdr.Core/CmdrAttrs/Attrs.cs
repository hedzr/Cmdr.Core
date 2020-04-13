using System;
using System.Diagnostics.CodeAnalysis;
using HzNS.Cmdr.CmdrAttrs.Internal;

namespace HzNS.Cmdr.CmdrAttrs
{
    [AttributeUsage(AttributeTargets.Class |
                    AttributeTargets.Struct, AllowMultiple = true)]
    public class CmdrAppInfo : Attribute
    {
        public string AppName;
        public string Author;
        public string Copyright;
        public string Tags;

        public CmdrAppInfo(string? appName = null, string? author = null, string? copyright = null, string? tags = null)
        {
            AppName = appName ?? throw new ArgumentNullException(nameof(appName));
            Author = author ?? string.Empty; // throw new ArgumentNullException(nameof(author));
            Copyright = copyright ?? string.Empty; // throw new ArgumentNullException(nameof(copyright));
            Tags = tags ?? string.Empty; // throw new ArgumentNullException(nameof(tags));
        }
    }


    [AttributeUsage(AttributeTargets.Class |
                    AttributeTargets.Struct, AllowMultiple = true)]
    public class CmdrCommand : BaseOptAttr
    {
        public CmdrCommand(string longName, string? shortName = null, params string[] aliases) : base(longName, shortName,
            aliases)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Field |
                    AttributeTargets.Enum |
                    AttributeTargets.Property, AllowMultiple = true)]
    public class CmdrOption : BaseOptAttr
    {
        // public Type Type { get; set; }
        
        public CmdrOption(string longName, string? shortName=null, params string[] aliases) : base(longName, shortName,
            aliases)
        {
            // this.Type = type;
        }
    }

    // [AttributeUsage(AttributeTargets.Class |
    //                        AttributeTargets.Struct, AllowMultiple = true)]
    // public class RootCommand : BaseOptAttr
    // {
    //     public RootCommand(string longName, string shortName, params string[] aliases) : base(longName, shortName,
    //         aliases)
    //     {
    //     }
    // }

    [AttributeUsage(AttributeTargets.Class |
                    AttributeTargets.Struct |
                    AttributeTargets.Field |
                    AttributeTargets.Enum |
                    AttributeTargets.Property, AllowMultiple = true)]
    public class CmdrDescriptions : Attribute
    {
        public string Description;
        public string DescriptionLong;
        public string Examples;
        public string TailArgs;
        public string PlaceHolder;

        public CmdrDescriptions(string description, string? descriptionLong = null, string? examples = null,
            string? tailArgs = null,string? placeHolder = null)
        {
            Description = description;
            DescriptionLong = descriptionLong ?? string.Empty;
            Examples = examples ?? string.Empty;
            TailArgs = tailArgs ?? string.Empty;
            PlaceHolder = placeHolder ?? string.Empty;
        }
    }

    [AttributeUsage(AttributeTargets.Class |
                    AttributeTargets.Struct |
                    AttributeTargets.Field |
                    AttributeTargets.Enum |
                    AttributeTargets.Property, AllowMultiple = true)]
    public class CmdrGroup : Attribute
    {
        public string GroupName;
        public string ToggleGroupName;

        public CmdrGroup(string? group = null, string? toggleGroup = null)
        {
            ToggleGroupName = toggleGroup ?? string.Empty;
            GroupName = toggleGroup ?? (group ?? string.Empty);
        }
    }


    [AttributeUsage(AttributeTargets.Method)]
    public class CmdrAction : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class CmdrPreAction : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class CmdrPostAction : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class CmdrOnSetAction : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class CmdrHidden : Attribute
    {
        public bool HiddenFlag;

        public CmdrHidden(bool hidden)
        {
            HiddenFlag = hidden;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class CmdrEnvVars : Attribute
    {
        public string[] VariableNames;

        public CmdrEnvVars(params string[] names)
        {
            VariableNames = names;
        }
    }


    // [AttributeUsage(AttributeTargets.Field |
    //                 AttributeTargets.Enum |
    //                 AttributeTargets.Property)]
    // public class CmdrDefaultValue : Attribute
    // {
    //     public object Value { get; set; }
    //     public string PlaceHolder { get; set; }
    //
    //     public CmdrDefaultValue(bool required = true, string? placeHolder = null)
    //     {
    //         Value = required;
    //         PlaceHolder = placeHolder ?? string.Empty;
    //     }
    // }

    [AttributeUsage(AttributeTargets.Field |
                    AttributeTargets.Enum |
                    AttributeTargets.Property)]
    public class CmdrRequired : Attribute
    {
        public bool State { get; set; }

        public CmdrRequired(bool required = true)
        {
            State = required;
        }
    }

    [AttributeUsage(AttributeTargets.Field |
                    AttributeTargets.Enum |
                    AttributeTargets.Property)]
    public class CmdrRange : Attribute
    {
        public int Min { get; set; }
        public int Max { get; set; }
        public int Step { get; set; } = 1;

        public CmdrRange(int min, int max, int step = 1)
        {
            Min = min;
            Max = max;
            Step = step;
        }
    }


    #region Internal namespace

    namespace Internal
    {
        [AttributeUsage(AttributeTargets.Class |
                        AttributeTargets.Struct, AllowMultiple = true)]
        public class BaseOptAttr : Attribute
        {
            public string Long;
            public string Short;
            public string[] Aliases;

            protected BaseOptAttr(string longName, string? shortName = null, params string[] aliases)
            {
                Long = longName;
                Short = shortName ?? string.Empty;
                Aliases = aliases;
            }
        }
    }

    #endregion
}