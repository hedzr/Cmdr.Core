#nullable enable
using System.Diagnostics.CodeAnalysis;

namespace HzNS.Cmdr.Base
{
    [SuppressMessage("ReSharper", "ConvertToAutoPropertyWhenPossible")]
    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public abstract class BaseRootCommand : BaseCommand, IRootCommand
    {
        public IAppInfo AppInfo { get; set; }

        // ReSharper disable once PublicConstructorInAbstractClass
        public BaseRootCommand(IAppInfo appInfo)
        {
            AppInfo = appInfo;
        }

        // // ReSharper disable once PublicConstructorInAbstractClass
        // private BaseRootCommand(string shortTitle, string longTitle, string[] aliases, string description,
        //     string descriptionLong, string examples, IAppInfo? appInfo = null) : base(shortTitle, longTitle, aliases,
        //     description, descriptionLong, examples)
        // {
        //     if (appInfo != null) AppInfo = appInfo;
        // }

        public IRootCommand AddAppInfo(IAppInfo appInfo)
        {
            // throw new System.NotImplementedException();

            AppInfo = appInfo;
            return this;
        }
    }
}