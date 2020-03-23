using System;
using System.Collections.Generic;
using HzNS.Cmdr.Base;
using HzNS.Cmdr.Internal.Base;

namespace HzNS.Cmdr.Cmd
{
    /// <summary>
    /// Just a sample Command class to avoid resharper warning:
    /// suspicious type check: there is no type in the solution which is inherited...
    /// </summary>
    public class SampleRootCmd : BaseRootCommand, IAction, IPreAction, IPostAction, IOnSet
    {
        public void PreInvoke(IBaseWorker w, IEnumerable<string> remainsArgs)
        {
            Console.WriteLine($"{w}: {remainsArgs}");
        }

        public void Invoke(IBaseWorker w, IEnumerable<string> remainsArgs)
        {
            Console.WriteLine($"{w}: {remainsArgs}");
        }

        public void PostInvoke(IBaseWorker w, IEnumerable<string> remainsArgs)
        {
            Console.WriteLine($"{w}: {remainsArgs}");
        }

        public void OnSetHandler(IBaseWorker w, object newValue, object oldValue)
        {
            throw new NotImplementedException();
        }

        private SampleRootCmd(IAppInfo appInfo) : base(appInfo)
        {
        }

        public static SampleRootCmd New(IAppInfo appInfo, params Action<SampleRootCmd>[] opts)
        {
            var r = new SampleRootCmd(appInfo);

            foreach (var opt in opts)
            {
                opt(r);
            }

            return r;
        }
    }
}