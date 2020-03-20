using System;
using HzNS.Cmdr.Base;

namespace HzNS.Cmdr
{
    public class RootCommand : BaseRootCommand
    {
        private RootCommand(IAppInfo appInfo) : base(appInfo)
        {
        }

        public static RootCommand New(IAppInfo appInfo, params Action<RootCommand>[] opts)
        {
            var r = new RootCommand(appInfo);

            foreach (var opt in opts)
            {
                opt(r);
            }

            return r;
        }
    }
}