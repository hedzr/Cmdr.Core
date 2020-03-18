using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HzNS.Cmdr.Builder;

namespace HzNS.Cmdr.Base
{
    [SuppressMessage("ReSharper", "ConvertToAutoPropertyWhenPossible")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class BaseRootCommand : BaseCommand, IRootCommand
    {
        public IAppInfo AppInfo;

        public IRootCommand AddAppInfo(IAppInfo appInfo)
        {
            // throw new System.NotImplementedException();
            AppInfo = appInfo;
            return this;
        }
    }
}