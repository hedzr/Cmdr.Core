using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HzNS.Cmdr.Base;
using HzNS.Cmdr.Exception;

namespace HzNS.Cmdr.Builder
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public abstract class BuiltinOptions
    {
        public static void InsertAll(ICommand root)
        {
            addVersionCommands(root);
            addHelpCommands(root);
            addVerboseCommands(root);
        }

        private static void addVersionCommands(ICommand root)
        {
            root.AddCommand(new Command
            {
                Long = "version", Short = "", Aliases = new[] {"ver", "versions"},
                Description = "Show the version of this app.",
                Hidden = true,
                Group = Worker.SysMgmtGroup,
                Action = (worker, remainArgs) => worker.ShowVersions(worker, remainArgs.ToArray()),
            });
            root.AddFlag(new Flag<bool>
            {
                DefaultValue = false, Long = "version", Short = "V", Aliases = new[] {"ver", "versions"},
                Description = "Show the version of this app.",
                Hidden = true,
                Group = Worker.SysMgmtGroup,
                PreAction = delegate(Worker worker, IEnumerable<string> remainArgs)
                {
                    worker.ShowVersions(worker, remainArgs.ToArray());
                    return false;
                },
            });
            root.AddFlag(new Flag<string>
            {
                DefaultValue = "", Long = "version-sim", Short = "vs", Aliases = new[] {"ver-sim", "version-simulate"},
                Description = "Simulate a faked version number for this app.",
                Hidden = true,
                Group = Worker.SysMgmtGroup,
                // ReSharper disable once UnusedAnonymousMethodSignature
                Action = delegate(Worker worker, IEnumerable<string> remainArgs)
                {
                    // conf.Version = GetStringR("version-sim");
                    // Set("version", conf.Version); // set into option 'app.     version' too.
                },
                EnvVars = new[] {"VERSION"},
            });
            root.AddFlag(new Flag<bool>
            {
                DefaultValue = false, Long = "build-info", Short = "#", Aliases = new[] {"bi"},
                Description = "Show the building information of this app.",
                Hidden = true,
                Group = Worker.SysMgmtGroup,
                PreAction = (worker, remainArgs) =>
                {
                    worker.ShowBuildInfo(worker, remainArgs.ToArray());
                    return false;
                },
            });
        }

        private static void addHelpCommands(ICommand root)
        {
            root.AddFlag(new Flag<bool>
            {
                DefaultValue = false, Long = "help", Short = "h", Aliases = new[] {"info", "usage", "helpme"},
                Description = "Show this help screen",
                Hidden = true,
                Group = Worker.SysMgmtGroup,
                PreAction = (worker, remainArgs) => throw new WantHelpScreenException(remainArgs.ToArray()),
            });
            root.AddFlag(new Flag<bool>
            {
                DefaultValue = false, Long = "help-ext", Short = "?",
                Description = "Show this help screen",
                Hidden = true,
                Group = Worker.SysMgmtGroup,
                PreAction = (worker, remainArgs) => throw new WantHelpScreenException(remainArgs.ToArray()),
            });
            root.AddFlag(new Flag<bool>
            {
                DefaultValue = false, Long = "help-zsh", Short = "", Aliases = new[] {"zsh"},
                Description = "show help with zsh format, or others",
                Hidden = true,
                Group = Worker.SysMgmtGroup,
                ToggleGroup = "shell-mode",
            });
            root.AddFlag(new Flag<bool>
            {
                DefaultValue = false, Long = "help-bash", Short = "", Aliases = new[] {"bash"},
                Description = "show help with bash format, or others",
                Hidden = true,
                Group = Worker.SysMgmtGroup,
                ToggleGroup = "shell-mode",
            });
            root.AddFlag(new Flag<bool>
            {
                DefaultValue = true, Long = "help-auto", Short = "", Aliases = new[] {"auto-detect"},
                Description = "show help with zsh/bash/... format, or others",
                Hidden = true,
                Group = Worker.SysMgmtGroup,
                ToggleGroup = "shell-mode",
            });

            root.AddFlag(new Flag<bool>
            {
                DefaultValue = true, Long = "tree", Short = "", Aliases = new string[] { },
                Description = "show a tree for all commands",
                Hidden = true,
                Group = Worker.SysMgmtGroup,
                PreAction = ((worker, remainArgs) =>
                {
                    worker.DumpTreeForAllCommands(worker, remainArgs.ToArray());
                    return false;
                })
            });

            root.AddFlag(new Flag<string>
            {
                DefaultValue = "",
                PlaceHolder = "[Locations of config files]",
                Long = "config", Short = "", Aliases = new[] {"config-location"},
                Description = "load config files from where you specified",
                Examples =
                    "        $ {{.AppName}} --configci/etc/demo-yy ~~debug\r\n" +
                    "          try loading config from 'ci/etc/demo-yy', noted that assumes a child folder   'conf.d' should be exists\r\n" +
                    "        $ {{.AppName}} --config=ci/etc/demo-yy/any.yml ~~debug\r\n" +
                    "          try loading config from 'ci/etc/demo-yy/any.yml', noted that assumes a child  folder 'conf.d' should be exists\r\n",
                Group = Worker.SysMgmtGroup,
            });
        }

        private static void addVerboseCommands(ICommand root)
        {
            root.AddFlag(new Flag<bool>
            {
                DefaultValue = false, Long = "verbose", Short = "v",
                Description = "Show more information.",
                Group = Worker.SysMgmtGroup,
                EnvVars = new[] {"VERBOSE"},
            });
            root.AddFlag(new Flag<bool>
            {
                DefaultValue = false, Long = "quiet", Short = "q",
                Description = "Quiet mode, no information to output.",
                Group = Worker.SysMgmtGroup,
                EnvVars = new[] {"QUIET"},
            });
            root.AddFlag(new Flag<bool>
            {
                DefaultValue = false, Long = "debug", Short = "D",
                Description = "Get into debug mode.",
                Group = Worker.SysMgmtGroup,
                EnvVars = new[] {"DEBUG"},
            });
        }
    }
}