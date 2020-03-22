using System;
using System.Diagnostics.CodeAnalysis;

namespace HzNS.Cmdr.Tool
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class DeprecatedAttribute : Attribute
    {
        /// <summary>
        /// A version string like 'v1.2.1'
        /// </summary>
        public string Since { get; set; }

        public string Reason { get; set; }
        public string Suggestion { get; set; }

        public DeprecatedAttribute(string? since = null, string? reason = null, string? suggestion = null)
        {
            Since = since ?? throw new ArgumentNullException(nameof(since));
            Reason = reason ?? ""; //throw new ArgumentNullException(nameof(reason));
            Suggestion = suggestion ?? ""; //throw new ArgumentNullException(nameof(suggestion));
        }
    }
}