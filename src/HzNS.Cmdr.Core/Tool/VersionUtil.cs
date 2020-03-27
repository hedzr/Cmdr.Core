using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using HzNS.Cmdr.Builder;

namespace HzNS.Cmdr.Tool
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public abstract class VersionUtil
    {
        private class Foo
        {
        }

        // ReSharper disable once InconsistentNaming
        private static readonly Foo _foo = new Foo();

        /// <summary>
        /// Where other assemblies that reference your assembly will look.
        /// If this number changes, other assemblies have to update their
        /// references to your assembly.
        /// </summary>
        public static string AssemblyVersion => _foo.GetType().Assembly.GetName().Version.ToString();

        /// <summary>
        /// Used for deployment. You can increase this number for every
        /// deployment. It is used by setup programs. Use it to mark assemblies
        /// that have the same AssemblyVersion, but are generated from
        /// different builds.
        ///
        /// In Windows, it can be viewed in the file properties.
        ///
        ///     If possible, let it be generated by MSBuild. The AssemblyFileVersion
        /// is optional. If not given, the AssemblyVersion is used.
        ///
        ///     I use the format: major.minor.revision.build, where I use revision
        /// for development stage (Alpha, Beta, RC and RTM), service packs and
        /// hot fixes. 
        /// </summary>
        public static string? FileVersion => GetFileVersion(_foo.GetType().Assembly);

        /// <summary>
        /// The Product version of the assembly. This is the version you would
        /// use when talking to customers or for display on your website.
        /// This version can be a string, like '1.0 Release Candidate'.
        /// </summary>
        public static string InformationalVersion => GetInformationalVersion(_foo.GetType().Assembly);

        public static string PackageVersion => GetInformationalVersion(_foo.GetType().Assembly);

        public static string AppEntryFileVersion => GetFileVersion(Assembly.GetEntryAssembly());

        public static DateTime LinkerTimestampUtc => GetLinkerTimestampUtc(_foo.GetType().Assembly);
        
        
        public static string? GetFileVersion(Assembly? assembly)
        {
            return assembly?.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
        }

        public static string? GetInformationalVersion(Assembly? assembly)
        {
            return assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        }

        public static string? GetPackageVersion(Assembly? assembly)
        {
            return null; //assembly?.GetCustomAttribute<NugPackageVersion>().InformationalVersion;
        }

        /// <summary>
        /// fit for PE executable.
        /// see also: https://www.meziantou.net/getting-the-date-of-build-of-a-dotnet-assembly-at-runtime.htm
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static DateTime GetLinkerTimestampUtc(Assembly assembly)
        {
            var location = assembly.Location;
            return GetLinkerTimestampUtc(location);
        }

        public static DateTime GetLinkerTimestampUtc(string filePath)
        {
            const int peHeaderOffset = 60;
            const int linkerTimestampOffset = 8;
            var bytes = new byte[2048];

            using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                file.Read(bytes, 0, bytes.Length);
            }

            var headerPos = BitConverter.ToInt32(bytes, peHeaderOffset);
            var secondsSince1970 = BitConverter.ToInt32(bytes, headerPos + linkerTimestampOffset);
            var dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return dt.AddSeconds(secondsSince1970);
        }

        public static DateTime BuildTimestamp => GetBuildDate(_foo.GetType().Assembly);

        /// <summary>
        /// get custom attribute from assembly.
        ///
        /// <code>[assembly: BuildDateAttribute("20180901204042")]</code>
        /// https://www.meziantou.net/getting-the-date-of-build-of-a-dotnet-assembly-at-runtime.htm
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        private static DateTime GetBuildDate(Assembly assembly)
        {
            var attribute = assembly.GetCustomAttribute<BuildDateAttribute>();
            return attribute?.DateTime ?? default(DateTime);
        }
        
        
        public static string CommitHashPrb => GetCommitHashPrb(_foo.GetType().Assembly);

        private static string GetCommitHashPrb(Assembly assembly)
        {
            var attr = assembly.GetCustomAttribute<AssemblyMetadataAttribute>();
            // attr.Key
            return attr?.ToString() ?? string.Empty;
        }

        public static string AssemblyProductAttribute
        {
            get
            {
                var attr = GetAssemblyProductAttribute(_foo.GetType().Assembly);
                return attr?.Product ?? string.Empty;
            }
        }

        public static AssemblyProductAttribute? GetAssemblyProductAttribute(Assembly assembly)
        {
            var attributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
            AssemblyProductAttribute? attribute = null;
            if (attributes.Length > 0)
            {
                attribute = attributes[0] as AssemblyProductAttribute;
            }

            return attribute;
        }
        
        public static FileVersionInfo FileVersionInfo => FileVersionInfo.GetVersionInfo(_foo.GetType().Assembly.Location);
    }

    public static class ShellExtensions
    {
        public static string Run(this string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");
            Process process;
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.MacOSX:
                case PlatformID.Unix:
                    process = new Process()
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            // https://loune.net/2017/06/running-shell-bash-commands-in-net-core/
                            FileName = "/bin/bash",
                            Arguments = $"-c \"{escapedArgs}\"",
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                        }
                    };
                    break;
                default:
                    process = new Process()
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = $"-c \"{escapedArgs}\"",
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                        }
                    };
                    break;
            }

            process.Start();
            var result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return result;
        }

        // string ref1()
        // {
        //     // https://stackoverflow.com/questions/1469764/run-command-prompt-commands
        //     var process = new System.Diagnostics.Process
        //     {
        //         StartInfo = new System.Diagnostics.ProcessStartInfo
        //         {
        //             WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
        //             // startInfo.FileName = "cmd.exe";
        //             // startInfo.Arguments = "/C copy /b Image1.jpg + Archive.rar Image2.jpg";
        //             FileName = "git",
        //             Arguments = "rev-parse HEAD",
        //             // get full version tag in git repo:
        //             // git describe --long
        //         }
        //     };
        //     process.Start();
        //     var output = process.StandardOutput.ReadToEnd();
        //     return output;
        // }
    }
}