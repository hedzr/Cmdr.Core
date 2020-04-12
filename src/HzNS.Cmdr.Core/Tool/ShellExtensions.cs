using System;
using System.Diagnostics;

namespace HzNS.Cmdr.Tool
{
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