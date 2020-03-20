using System;
using System.Collections.Generic;

namespace HzNS.Cmdr.Base
{
    public interface IBaseOpt
    {
        string Long { get; set; }
        
        string Short { get; set; }
        string[] Aliases { get; set; }

        string Description { get; set; }
        string DescriptionLong { get; set; }
        string Examples { get; set; }

        string Group { get; set; }
        string ToggleGroup { get; set; }

        bool Hidden { get; set; }
        
        string[] EnvVars { get; set; }

        Func<Worker, IEnumerable<string>, bool>? PreAction { get; set; }
        Action<Worker, IEnumerable<string>>? PostAction { get; set; }
        Action<Worker, IEnumerable<string>>? Action { get; set; }
        Action<Worker, object?, object?>? OnSet { get; set; }

        /// <summary>
        /// To point to the owner of an option, an ICommand object.
        /// </summary>
        ICommand? Owner { get; set; }

        bool Match(string s, bool isLongOpt = false);
    }
}