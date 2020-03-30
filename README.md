# Cmdr.Core

Useful POSIX command line arguments parser for .Net. Hierarchy configurations Store for app.

**WIP**

The Pre-release is coming soon.

- dotNet Core 3.0+
- dotNet Standard 2.1+
- dotNet 4.8+ [?] (*NOT SURE*)


## NuGet

```bash
PM> Install-Package HzNS.Cmdr.Core -Version 1.0.29
# Or CLI
$ dotnet add package HzNS.Cmdr.Core --version 1.0.29
```


## Features

cmdr has rich features:

- [x] **POSIX Compatible** (Unix [*getopt*(3)](http://man7.org/linux/man-pages/man3/getopt.3.html))
- [x] **[IEEE Standard](http://pubs.opengroup.org/onlinepubs/9699919799/basedefs/V1_chap12.html)** Compartiblities
- [x] builds multi-level command and sub-commands
- [x] builds short, long and alias options with kinds of data types
- [x] defines commands and options via fluent api style
- [x] full featured `Options Store` for hosted any application configurations
  - watchable external config file and child directory `conf.d`
  - watchable option value merging event: while option value modified in external config file and loaded automatically.
  - watchable option value modifying event: while option value modified (from config file, or programmatically)
  - connectable with external configuration center

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
    _see also: option store and hierarchy data_

- [x] Supports for `-D+`, `-D-` to enable/disable a bool option.

- [x] Automatic help screen generation (*Generates and prints well-formatted help message*)

- [x] Sortable commands and options/flags. Or sorted by alphabetic order.

- [x] Predefined commands and flags:

  - Help: `-h`, `-?`, `--help`, `--info`, `--usage`, `--helpme`, ...
  - Version & Build Info: `--version`/`--ver`/`-V`, `--build-info`/`-#`
    - Simulating version at runtime with `—version-sim 1.9.1`
    - [ ] generally, `conf.AppName` and `conf.Version` are originally.
    - `~~tree`: list all commands and sub-commands.
    - `--config <location>`: specify the location of the root config file.
    - `version` command available.
  - Verbose & Debug: `—verbose`/`-v`, `—debug`/`-D`, `—quiet`/`-q`

- [x] Groupable commands and options/flags.

  Sortable group name with `[0-9A-Za-z]+\..+` format, eg:

  - `1001.c++`, `1100.golang`, `1200.java`, …;
  - `abcd.c++`, `b999.golang`, `zzzz.java`, …;

- [x] Supports for unlimited multi-level sub-commands.

- [x] Overrides by environment variables.

  *priority level:* `defaultValue -> config-file -> env-var -> command-line opts`

- [x] `Option Store` - Unify option value extraction:

- [x] Walkable

  - Customizable `Painter` interface to loop *each* command and flag.
  - Walks on all commands with `Walk(walker)`.

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
    
  - [x] Watch `conf.d` directory
  
  - `RegisterExternalConfigurationsLoader(loader, ...)`
    

- [x] Handlers

  - Global Handlers: `RootCommand.OnPre/Post/Action()/OnSet()` will be triggered before/after the concrete `Command.OnPre/Post/Action()/OnSet()`
  - Command Actions: `OnPreAction/OnAction/OnPostAction/OnSet`
  - Flag Actions: `OnPreAction/OnAction/OnPostAction/OnSet`
  - Parsing Events:
    - `bool OnDuplicatedCommandChar(worker, cmd, isShort, matchingString)`
    - `bool OnDuplicatedFlagChar(worker, cmd, flag, isShort, matchingString)`
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

## `Option Store` - Hierarchy Configuration Data Store

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

### CMDR EnvVars

#### `CMDR_DUMP`

enable Store entries dumping at the end of help screen.

#### `CMDR_DEBUG`

= `Worker.EnableCmdrLogDebug`

allows the display output in `defaultOnSet`.

#### `CMDR_TRACE`

= `Worker.EnableCmdrLogTrace`

allows the worker `logDebug()`.

#### `CMDR_VERBOSE`

allows more logging output.





## LICENSE

MIT




