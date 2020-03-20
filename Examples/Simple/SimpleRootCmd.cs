using System;
using HzNS.Cmdr.Base;

namespace Simple
{
    public class SimpleRootCmd : BaseRootCommand
    {
        // public void Invoke(Worker w, IEnumerable<string> remainsArgs)
        // {
        //     foreach (var filename in remainsArgs)
        //     {
        //         if (string.IsNullOrEmpty(filename)) continue;
        //
        //         w.log.Information($"loading {filename} ...");
        //         // using var l = new MDictLoader(filename);
        //         try
        //         {
        //             // l.Process();
        //             // Console.WriteLine($"header: {l.DictHeader}");
        //         }
        //         catch (Exception ex)
        //         {
        //             Debug.WriteLine(ex.ToString());
        //             // throw;
        //         }
        //         finally
        //         {
        //             w.ParsedCount++;
        //             // w.log.Information($"#{w.ParsedCount} parsed.");
        //
        //             // l.Dispose();
        //         }
        //     }
        //
        //     if (w.ParsedCount == 0)
        //         w.log.Warning("Nothing to parsed.");
        // }

        private SimpleRootCmd(IAppInfo appInfo) : base(appInfo)
        {
        }

        public static SimpleRootCmd New(IAppInfo appInfo, params Action<SimpleRootCmd>[] opts)
        {
            var r = new SimpleRootCmd(appInfo);
            
            foreach (var opt in opts)
            {
                opt(r);
            }

            return r;
        }
    }
}