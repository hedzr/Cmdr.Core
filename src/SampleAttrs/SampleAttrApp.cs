using System;
using System.Collections.Generic;
using HzNS.Cmdr;
using HzNS.Cmdr.Base;
using HzNS.Cmdr.CmdrAttrs;

namespace SampleAttrs
{
    [CmdrAppInfo("SimpleAttrs")]
    // ReSharper disable once IdentifierTypo
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SampleAttrApp
    {
        // internal static int Main(string[] args) => Cmdr.Compile<Prog1>(args);

        [CmdrOption("count", "c", "cnt")]
        [CmdrRange(0, 10)]
        [CmdrRequired]
        public int Count { get; }

        [CmdrCommand("tags", "t")]
        [CmdrGroup("")]
        [CmdrDescriptions("tags operations")]
        public class TagsCmd
        {
            [CmdrCommand("mode", "m")]
            public class ModeCmd
            {
                [CmdrAction]
                public void Execute(IBaseWorker w, IBaseOpt cmd, IEnumerable<string> remainArgs)
                {
                    // throw new System.NotImplementedException();
                    Console.WriteLine(
                        $"Hit: {cmd}, Remains: {remainArgs}. Count: {Cmdr.Instance.Store.GetAs<int>("count")}");
                    // for (var i = 0; i < Count; i++)
                    // {
                    //     // Prompt.GetPassword("Enter your password: ");
                    // }
                }
            }
        }
    }
}