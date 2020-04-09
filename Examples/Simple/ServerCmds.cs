using System;
using HzNS.Cmdr;
using HzNS.Cmdr.Base;

namespace Simple
{
    public class ServerCmd : BaseCommand
    {
        // ReSharper disable once StringLiteralTypo
        public ServerCmd() : base("s", "server", new string[] { }, "Communicating Protocol Server", "", "")
        {
            this.AddCommand(new ServerStartCmd())
                .AddCommand(new ServerStopCmd())
                .AddCommand(new ServerStatusCmd())
                .AddCommand(new ServerReloadCmd())
                .AddCommand(new ServerRestartCmd());

            this.AddFlag(new Flag<int>
            {
                Short = "p", Long = "port", EnvVars = new[] {"PORT"},
                Description = "Server Port",
            }, false);
        }
    }

    public class ServerStartCmd : BaseCommand
    {
        // ReSharper disable once StringLiteralTypo
        public ServerStartCmd() : base("s", "start", new[] {"run", "go"},
            "start server (in a detached session)",
            "server will be started in a detached session, or you can run it at foreground with command 'run' or. such as:\n" +
            "    $ {app} {parents} run\n" +
            "    $ {app} {parents} start -f",
            "'run'/'-f' make the server running at foreground.")
        {
            Action = (worker, sender, remainArgs) =>
            {
                // Class1.Hello();
                Console.WriteLine(
                    $"[HIT] server start, '{worker.ParsedCommand?.HitTitle}'. remains: '{string.Join(",", remainArgs)}'");
            };


            this.AddFlag(new Flag<int>
            {
                Short = "f", Long = "foreground", EnvVars = new[] {"FG"},
                Description = "start server in the current tty/console/terminal.",
            }, false);
        }
    }

    public class ServerStopCmd : BaseCommand
    {
        // ReSharper disable once StringLiteralTypo
        public ServerStopCmd() : base("t", "stop", new string[] {"halt", "shutdown"}, "stop server", "", "")
        {
        }
    }

    public class ServerStatusCmd : BaseCommand
    {
        // ReSharper disable once StringLiteralTypo
        public ServerStatusCmd() : base("st", "status", new string[] {"info"}, "print server status", "", "")
        {
        }
    }

    public class ServerReloadCmd : BaseCommand
    {
        // ReSharper disable once StringLiteralTypo
        public ServerReloadCmd() : base("r", "reload", new string[] { }, "reload server", "", "")
        {
        }
    }

    public class ServerRestartCmd : BaseCommand
    {
        // ReSharper disable once StringLiteralTypo
        public ServerRestartCmd() : base("", "restart", new string[] { }, "restart server", "", "")
        {
        }
    }
}