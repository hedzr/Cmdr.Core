using System;
using System.Collections.Generic;
using HzNS.Cmdr;
using HzNS.Cmdr.Base;
using HzNS.Cmdr.CmdrAttrs;

namespace SampleAttrs
{
    [CmdrAppInfo(appName: "SimpleAttrs", author: "hedzr", copyright: "copyright")]
    // ReSharper disable once IdentifierTypo
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SampleAttrApp
    {
        // internal static int Main(string[] args) => Cmdr.Compile<Prog1>(args);

        [CmdrOption(longName: "count", shortName: "c", "cnt")]
        [CmdrDescriptions(description: "a counter", descriptionLong: "",examples: "")]
        [CmdrRange(min: 0, max: 10)]
        [CmdrRequired]
        public int Count { get; }

        [CmdrCommand(longName: "tags", shortName: "t")]
        [CmdrGroup(@group: "")]
        [CmdrDescriptions(description: "tags operations")]
        public class TagsCmd
        {
            [CmdrCommand(longName: "mode", shortName: "m")]
            [CmdrDescriptions(description: "set tags' mode", descriptionLong: "",examples: "")]
            public class ModeCmd
            {
                [CmdrAction]
                public void Execute(IBaseWorker w, IBaseOpt cmd, IEnumerable<string> remainArgs)
                {
                    // throw new System.NotImplementedException();
                    Console.WriteLine(
                        value: $"Hit: {cmd}, Remains: {remainArgs}. Count: {Cmdr.Instance.Store.GetAs<int>(key: "count")}");
                    // for (var i = 0; i < Count; i++)
                    // {
                    //     // Prompt.GetPassword("Enter your password: ");
                    // }
                }
            }
        }
    }
}