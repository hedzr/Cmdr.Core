using System;
using System.Collections.Generic;
using System.Diagnostics;
using HzNS.Cmdr;
using HzNS.Cmdr.Action;
using HzNS.Cmdr.Base;
using HzNS.Cmdr.Builder;
using HzNS.MdxLib.MDict;

namespace mdx.Cmd
{
    public class RootCmd : BaseRootCommand, IAction
    {
        public void Invoke(Worker w, IEnumerable<string> remainsArgs)
        {
            foreach (var filename in remainsArgs)
            {
                if (string.IsNullOrEmpty(filename)) continue;

                using var l = new MDictLoader(filename);
                try
                {
                    l.Process();
                    Console.WriteLine($"header: {l.DictHeader}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    // throw;
                }
                finally
                {
                    w.ParsedCount++;
                    w.log.Information($"#{w.ParsedCount} parsed.");

                    // l.Dispose();
                }
            }

            if (w.ParsedCount == 0)
                w.log.Warning("Nothing to parsed.");
        }
    }
}