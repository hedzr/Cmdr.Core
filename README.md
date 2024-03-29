# Cmdr.Core

[![CircleCI](https://circleci.com/gh/hedzr/Cmdr.Core/tree/master.svg?style=shield)](https://app.circleci.com/pipelines/github/hedzr/Cmdr.Core) 
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/HzNS.Cmdr.Core)](https://www.nuget.org/packages/HzNS.Cmdr.Core/)



Useful POSIX command line arguments parser for dotNet. Hierarchical configurations Store for app.

Supported:

- dotNet 6 (since v1.3+)
- dotNet Core 3.1 (since v1.1+)
- dotNet Standard 2.1+ (since v1.1+)
- ~~dotNet 4.8+ [?] (*NOT SURE*)~~

> NOTED .NET 5 has been ignored.

## NuGet

```bash
PM> Install-Package HzNS.Cmdr.Core -Version 1.0.29
# Or CLI
$ dotnet add package HzNS.Cmdr.Core --version 1.0.29
```

Please replace `1.0.29` with the newest version (stable or pre-release), see the nuget badge icon.


## Features

`Cmdr.Core` has rich features:

- [x] **POSIX Compatible** (Unix [*getopt*(3)](http://man7.org/linux/man-pages/man3/getopt.3.html))
- [x] **[IEEE Standard](http://pubs.opengroup.org/onlinepubs/9699919799/basedefs/V1_chap12.html)** Compartiblities
- [x] builds multi-level command and sub-commands
- [x] builds short, long and alias options with kinds of data types
- [x] defines commands and options via fluent api style
- [x] full featured `Options Store` for hosting any application configurations
  - watchable external config file and child directory `conf.d`.
  - watchable option value merging event: while option value modified in external config file, it'll be loaded and merged automatically.
  - watchable option value modifying event: while option value modified (from config file, or programmatically)
  - connectable with external configuration-center

### More

- Unix [*getopt*(3)](http://man7.org/linux/man-pages/man3/getopt.3.html) representation but without its programmatic interface.

  - [x] Options with short names (`-h`)
  - [x] Options with long names (`--help`)
  - [x] Options with aliases (`--helpme`, `--usage`, `--info`)
  - [x] Options with and without arguments (bool v.s. other type)
  - [x] Options with optional arguments and default values
  - [x] Multiple option groups each containing a set of options
  - [x] Supports the compat short options `-aux` == `-a -u -x`, `-vvv` == `-v -v -v` (HitCount=3)
  - [x] Supports namespaces for (nested) option groups
    _see also: option store and hierarchical data_

- [x] Supports for `-D+`, `-D-` to enable/disable a bool option.

- [x] Supports for PassThrough by `--`. (Passing remaining command line arguments after -- (optional))

- [x] Automatic help screen generation (*Generates and prints well-formatted help message*)

- [x] Predefined commands and flags:

  - Help: `-h`, `-?`, `--help`, `--info`, `--usage`, `--helpme`, ...
  - Version & Build Info: `--version`/`--ver`/`-V`, `--build-info`/`-#`
    - Simulating version at runtime with `—version-sim 1.9.1`
    - [ ] generally, `conf.AppName` and `conf.Version` are originally.
    - `--tree`: list all commands and sub-commands.
    - `--config <location>`: specify the location of the root config file.
    - `version` command available.
  - Verbose & Debug: `—verbose`/`-v`, `—debug`/`-D`, `—quiet`/`-q`

- [x] Sortable commands and options/flags: sorted by alphabetic order or not (`worker.SortByAlphabeticAscending`).

- [x] Grouped commands and options/flags.
  
  Group Title may have a non-displayable prefix for sorting, separated by '.'.
  
  Sortable group name can be `[0-9A-Za-z]+\..+` format typically, or any string tailed '.', eg:

  - `1001.c++`, `1100.golang`, `1200.java`, …;
  - `abcd.c++`, `b999.golang`, `zzzz.java`, …;

- [x] Supports for unlimited multi-level sub-commands.

- [x] Overrides by environment variables.

  *priority level:* `defaultValue -> config-file -> env-var -> command-line opts`

- [x] `Option Store` - Unify option value extraction.

- [x] Walkable

  - Customizable `Painter` interface to loop *each* command and flag.
  - Walks on all commands with `Walk(from, commandWalker, flagWalker)`.

- [x] Supports `-I/usr/include -I=/usr/include` `-I /usr/include -I:/usr` option argument specifications
  Automatically allows those formats (applied to long option too):

  - `-I file`, `-Ifile`, and `-I=files`
  - `-I 'file'`, `-I'file'`, and `-I='files'`
  - `-I "file"`, `-I"file"`, and `-I="files"`

- [x] Supports for **PassThrough** by `--`. (*Passing remaining command line arguments after -- (optional)*)

- [x] Predefined external config file locations:

  - `/etc/<appname>/<appname>.yml` and `conf.d` sub-directory.
  - `/usr/local/etc/<appname>/<appname>.yml` and `conf.d` sub-directory.
  - `$HOME/.config/<appname>/<appname>.yml` and `conf.d` sub-directory.
  - `$HOME/.<appname>/<appname>.yml` and `conf.d` sub-directory.
  - the predefined locations are:

    ```go
    predefinedLocations: []string{
    	"./ci/etc/%s/%s.yml",       // for developer
    	"/etc/%s/%s.yml",           // regular location: /etc/$APPNAME/$APPNAME.yml
    	"/usr/local/etc/%s/%s.yml", // regular macOS HomeBrew location
    	"$HOME/.config/%s/%s.yml",  // per user: $HOME/.config/$APPNAME/$APPNAME.yml
    	"$HOME/.%s/%s.yml",         // ext location per user
    	"$THIS/%s.yml",             // executable's directory
    	"%s.yml",                   // current directory
    },
    ```
    
  - [x] Watch `conf.d` directory, the name is customizable (worker.).
  
  - `RegisterExternalConfigurationsLoader(loader, ...)`
    

- [x] Handlers

  - Global Handlers: `RootCommand.OnPre/Post/Action(), OnSet()` will be triggered before/after the concrete `Command.OnPre/Post/Action()/OnSet()`
  - Command Actions: `Command.OnPreAction/OnAction/OnPostAction(), OnSet`
  - Flag Actions: `Flag.OnPreAction/OnAction/OnPostAction(), OnSet`
  - Parsing Events:
    - `bool OnDuplicatedCommandChar(worker, cmd, isShort, matchingString)`
    - `bool OnDuplicatedFlagChar(worker, cmd, flag, isShort, matchingString)`
    - `bool OnCommandCannotMatched(ICommand parsedCommand, string matchingArg)`
    - `bool OnCommandCannotMatched(ICommand parsingCommand, string fragment, bool isShort, string matchingArg)`
    - `bool OnSuggestingForCommand(object worker, Dictionary&lt;string, ICommand&gt; dataset, string token)`
    - `bool OnSuggestingForFlag(object worker, Dictionary&lt;string, IFlag&gt; dataset, string token)`
    - 
    - ...
  - More...

- Unhandled Exception
  `cmdr` handled `AppDomain.CurrentDomain.UnhandledException` for better display. But you can override it always:
  ```c#
  static int Main(string[] args) {
      AppDomain.CurrentDomain.UnhandledException+=(sender,e)=>{};
      Cmdr.NewWorker(...).Run();
  }
  ```

- Smart suggestions for wrong command and flags

  based on [Jaro-Winkler distance](https://en.wikipedia.org/wiki/Jaro%E2%80%93Winkler_distance).
  ![](https://user-images.githubusercontent.com/12786150/79058363-a1895e00-7c9f-11ea-94b2-249ee920eaf9.png)






## `Option Store` - Hierarchical Configurations Store

Standard primitive types and non-primitive types.

#### `Get()`, `GetAs<T>()`
#### `Set<T>()`, `SetWithoutPrefix<T>()`
#### `Delete()`
#### `HasKeys()`, `HasKeysWithoutPrefix()`

```c#
var exists = Cmdr.Instance.Store.HasKeys("tags.mode.s1.s2");
var exists = Cmdr.Instance.Store.HasKeys(new string[] { "tags", "mode", "s1", "s2" });
var exists = Cmdr.Instance.Store.HasKeysWithoutPrefix(new string[] { "app", "tags", "mode", "s1", "s2" });
Console.WriteLine(Cmdr.Instance.Store.Prefix);
```

#### `FindBy()`

```c#
var (slot, valueKey) = Cmdr.Instance.Store.FindBy("tags.mode.s1.s2");
if (slot != null){
  if (string.IsNullOrWhiteSpace(valueKey)) {
    // a child slot node matched
  } else {
    // a value entry matched, inside a slot node
  }
}
```

#### `Walk()`

#### `GetAsMap()`

return a `SlotEntries` map so that you can yaml it:

```c#
  // NOTE: Cmdr.Instance.Store == worker.OptionsStore
  var map = worker.OptionsStore.GetAsMap("tags.mode");
  // worker.log.Information("tag.mode => {OptionsMap}", map);
  {
      var serializer = new SerializerBuilder().Build();
      var yaml = serializer.Serialize(map);
      Console.WriteLine(yaml);
  }
```




## CMDR EnvVars

#### `CMDR_DUMP`

enable Store entries dumping at the end of help screen.

##### `CMDR_DUMP_NO_STORE`, `CMDR_DUMP_NO_HIT`

To prevent the store dump, or hit options dump.

#### `CMDR_DEBUG`

= `Worker.EnableCmdrLogDebug`

allows the display output in `defaultOnSet`.

#### `CMDR_TRACE`

= `Worker.EnableCmdrLogTrace`

allows the worker `logDebug()`.

#### `CMDR_VERBOSE`

allows more logging output.





## Getting Start

### Fluent API

Basically, the Main program looks like:

```c#
static int Main(string[] args) => 
  Cmdr.NewWorker(RootCommand.New(
    new AppInfo(),  // your app information, desc, ...
    buildRootCmd(), // to attach the sub-commands and options to the RootCommand
    workerOpts,     // to customize the Cmdr Worker
  ))
  .Run(args, postRun);
```

Your first app with `Cmdr.Core` could be:

<details>
	<summary> Expand to source codes </summary>


```c#
namespace Simple
{
    class Program
    {
        static int Main(string[] args) => Cmdr.NewWorker(

                #region RootCmd Definitions

                RootCommand.New(
                    new AppInfo
                    {
                        AppName = "tag-tool",
                        Author = "hedzr",
                        Copyright = "Copyright © Hedzr Studio, 2020. All Rights Reserved.",
                    },
                    (root) =>
                    {
                        root.Description = "description here";
                        root.DescriptionLong = "long description here";
                        root.Examples = "examples here";

                        // for "dz"
                        _a = 0;

                        root.AddCommand(new Command
                            {
                                Long = "dz", Short = "dz", Description = "test divide by zero",
                                Action = (worker, opt, remainArgs) => { Console.WriteLine($"{B / _a}"); },
                            })
                            .AddCommand(new Command {Short = "t", Long = "tags", Description = "tags operations"}
                                .AddCommand(new TagsAddCmd())
                                .AddCommand(new TagsRemoveCmd())
                                // .AddCommand(new TagsAddCmd { }) // for dup-test
                                .AddCommand(new TagsListCmd())
                                .AddCommand(new TagsModifyCmd())
                                .AddCommand(new TagsModeCmd())
                                .AddCommand(new TagsToggleCmd())
                                .AddFlag(new Flag<string>
                                {
                                    DefaultValue = "consul.ops.local",
                                    Long = "addr", Short = "a", Aliases = new[] {"address", "host"},
                                    Description = "Consul IP/Host and/or Port: HOST[:PORT] (No leading 'http(s)://')",
                                    PlaceHolder = "HOST[:PORT]",
                                    Group = "Consul",
                                })
                                .AddFlag(new Flag<string>
                                {
                                    DefaultValue = "",
                                    Long = "cacert", Short = "", Aliases = new string[] {"ca-cert"},
                                    Description = "Consul Client CA cert)",
                                    PlaceHolder = "FILE",
                                    Group = "Consul",
                                })
                                .AddFlag(new Flag<string>
                                {
                                    DefaultValue = "",
                                    Long = "cert", Short = "", Aliases = new string[] { },
                                    Description = "Consul Client Cert)",
                                    PlaceHolder = "FILE",
                                    Group = "Consul",
                                })
                                .AddFlag(new Flag<bool>
                                {
                                    DefaultValue = false,
                                    Long = "insecure", Short = "k", Aliases = new string[] { },
                                    Description = "Ignore TLS host verification",
                                    Group = "Consul",
                                })
                            );

                        root.OnSet = (worker, flag, oldValue, newValue) =>
                        {
                            if (worker.OptionStore.GetAs<bool>("quiet")) return;
                            if (Cmdr.Instance.Store.GetAs<bool>("verbose") &&
                                flag.Root?.FindFlag("verbose")?.HitCount > 1)
                                Console.WriteLine($"--> [{Cmdr.Instance.Store.GetAs<bool>("quiet")}][root.onSet] {flag} set: {oldValue?.ToStringEx()} -> {newValue?.ToStringEx()}");
                        };
                    }
                ), // <- RootCmd Definitions

                #endregion

                #region Options for Worker

                (w) =>
                {
                    //
                    // w.UseSerilog((configuration) => configuration.WriteTo.Console().CreateLogger())
                    //

                    // w.EnableCmdrGreedyLongFlag = true;
                    // w.EnableDuplicatedCharThrows = true;
                    // w.EnableEmptyLongFieldThrows = true;

                    w.RegisterExternalConfigurationsLoader(ExternalConfigLoader);
                    
                    w.OnDuplicatedCommandChar = (worker, command, isShort, matchingArg) => false;
                    w.OnDuplicatedFlagChar = (worker, command, flag, isShort, matchingArg) => false;
                    w.OnCommandCannotMatched = (parsedCommand, matchingArg) => false;
                    w.OnFlagCannotMatched = (parsingCommand, fragment, isShort, matchingArg) => false;
                    w.OnSuggestingForCommand = (worker, dataset, token) => false;
                    w.OnSuggestingForFlag = (worker, dataset, token) => false;
                }

                #endregion

            )
            .Run(args, () =>
            {
                // Postrun here
                
                // Wait for the user to quit the program.

                // Console.WriteLine($"         AssemblyVersion: {VersionUtil.AssemblyVersion}");
                // Console.WriteLine($"             FileVersion: {VersionUtil.FileVersion}");
                // Console.WriteLine($"    InformationalVersion: {VersionUtil.InformationalVersion}");
                // Console.WriteLine($"AssemblyProductAttribute: {VersionUtil.AssemblyProductAttribute}");
                // Console.WriteLine($"      FileProductVersion: {VersionUtil.FileVersionInfo.ProductVersion}");
                // Console.WriteLine();

                // Console.WriteLine("Press 'q' to quit the sample.");
                // while (Console.Read() != 'q')
                // {
                //     //
                // }

                return 0;
            });

        private static void ExternalConfigLoader(IBaseWorker w, IRootCommand root)
        {
            // throw new NotImplementedException();
        }

        
        private static int _a = 9;
        private const int B = 10;
    }
}
```

</details>


### Declarative API

Since v1.0.139, we added the declarative API for compatibility with some others command-line argument parser libraries.

A sample at: [SimpleAttrs](https://github.com/hedzr/Cmdr.Core/tree/master/src/SampleAttrs).

The codes might be:

<details>
	<summary> Expand to source codes </summary>


```c#
class Program
{
    static int Main(string[] args) => Cmdr.Compile<SampleAttrApp>(args);
}

[CmdrAppInfo(appName: "SimpleAttrs", author: "hedzr", copyright: "copyright")]
public class SampleAttrApp
{
    [CmdrOption(longName: "count", shortName: "c", "cnt")]
    [CmdrDescriptions(description: "a counter", descriptionLong: "", examples: "")]
    [CmdrRange(min: 0, max: 10)]
    [CmdrRequired]
    public int Count { get; }

    [CmdrCommand(longName: "tags", shortName: "t")]
    [CmdrGroup(@group: "")]
    [CmdrDescriptions(description: "tags operations")]
    public class TagsCmd
    {
        [CmdrCommand(longName: "mode", shortName: "m")]
        [CmdrDescriptions(description: "set tags' mode", descriptionLong: "", examples: "")]
        public class ModeCmd
        {
            [CmdrAction]
            public void Execute(IBaseWorker w, IBaseOpt cmd, IEnumerable<string> remainArgs)
            {
                Console.WriteLine($"Hit: {cmd}, Remains: {remainArgs}. Count: {Cmdr.Instance.Store.GetAs<int>(key: "count")}");
             }

            [CmdrOption(longName: "count2", shortName: "c2", "cnt2")]
            [CmdrDescriptions(description: "a counter", descriptionLong: "", examples: "", placeHolder: "COUNT")]
            public int Count { get; }

            [CmdrOption(longName: "ok", shortName: "ok")]
            [CmdrDescriptions(description: "boolean option", descriptionLong: "", examples: "")]
            [CmdrHidden]
            public bool OK { get; }
            
            [CmdrOption(longName: "addr", shortName: "a", "address")]
            [CmdrDescriptions(description: "string option", descriptionLong: "", examples: "", placeHolder: "HOST[:PORT]")]
            public string Address { get; }
        }
    }
}
```

</details>





### Logger

The external logger has been removed from `Cmdr.Core`.

But you can always enable one or customize yours. In the `HzNS.Cmdr.Logger.Serilog` package/project, we've given an implements and it's simple to use:

1. Add `HzNS.Cmdr.Logger.Serilog` at first:

```bash
dotnet add package HzNS.Cmdr.Logger.Serilog --version 1.0.6
```

2. Modify the program entry:

```c#
    Cmdr.NewWorker(RootCommand.New(new AppInfo {AppName = "mdxTool", AppVersion = "1.0.0"}, (root) =>
            {
                root.AddCommand(new Command {Short = "t", Long = "tags", Description = "tags operations"});
            }), // <- RootCmd
            // Options ->
            (w) =>
            {
                w.SetLogger(HzNS.Cmdr.Logger.Serilog.SerilogBuilder.Build((logger) =>
                {
                    // These following flags will be loaded from envvars such as '$CMDR_TRACE', ...
                    // logger.EnableCmdrLogInfo = false;
                    // logger.EnableCmdrLogTrace = false;
                }));

                // w.EnableDuplicatedCharThrows = true;
            })
        .Run(args);
```




## ACKNOWNLEDGES

### [Colorify](https://github.com/deinsoftware/colorify)

I have to copy some codes from Colorify for the dotnetcore devenv.

There's some reason. But I will be pleasure to re-integrate the original or put an issue later (soon).



## Thanks to JODL

JODL (JetBrains OpenSource Development License) is good:

[![rider](https://gist.githubusercontent.com/hedzr/447849cb44138885e75fe46f1e35b4a0/raw/bedfe6923510405ade4c034c5c5085487532dee4/logo-rider.svg)](https://www.jetbrains.com/?from=hedzr/Cmdr.Core)
[![jetbrains](https://gist.githubusercontent.com/hedzr/447849cb44138885e75fe46f1e35b4a0/raw/bedfe6923510405ade4c034c5c5085487532dee4/jetbrains.svg)](https://www.jetbrains.com/?from=hedzr/Cmdr.Core)




## LICENSE

MIT




