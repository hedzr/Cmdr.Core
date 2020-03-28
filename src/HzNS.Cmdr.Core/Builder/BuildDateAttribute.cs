#nullable enable
using System;
using System.Globalization;

namespace HzNS.Cmdr.Builder
{
    /// <summary>
    /// for VersionUtil.BuildTimestamp.getter.
    /// 
    /// https://docs.microsoft.com/en-us/dotnet/standard/attributes/writing-custom-attributes
    /// https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/attributes/attributeusage
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    internal class BuildDateAttribute : Attribute
    {
        public BuildDateAttribute(string buildTimestamp)
        {
            DateTime = DateTime.ParseExact(buildTimestamp, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None);
            // GitHash = gitHash;
        }

        public DateTime DateTime { get; }
        // public string GitHash { get; } 
    }
}