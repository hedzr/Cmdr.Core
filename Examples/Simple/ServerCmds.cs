using System;
using HzNS.Cmdr.Base;

namespace Simple
{
    public class ServerStartCmd : BaseCommand
    {
        // ReSharper disable once StringLiteralTypo
        public ServerStartCmd() : base("s", "start", new string[] { "run", "go" }, "start server", "", "")
        {
            Action = (worker, sender, remainArgs) =>
            {
                // Class1.Hello();
                Console.WriteLine($"[HIT] mode settings. remains: '{string.Join(",", remainArgs)}'");
            };
        }
    }

    public class ServerStopCmd : BaseCommand
    {
        // ReSharper disable once StringLiteralTypo
        public ServerStopCmd() : base("t", "stop", new string[] { "halt", "shutdown" }, "stop server", "", "")
        {
        }
    }

    public class ServerStatusCmd : BaseCommand
    {
        // ReSharper disable once StringLiteralTypo
        public ServerStatusCmd() : base("st", "status", new string[] { "info" }, "print server status", "", "")
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