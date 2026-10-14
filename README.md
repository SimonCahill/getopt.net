# getopt.net - A GNU and POSIX getopt port to .net

This repository contains the code for my port of the GNU getopt functionality found on most Unix-like systems.

getopt.net is written entirely in C# and is a "cleanroom port"; although not necessary it made the project that much more fun 😊


![getopt.net logo](https://raw.githubusercontent.com/SimonCahill/getopt.net/master/img/getopt.net-logo-1280x128.png)


![Build Workflow](https://github.com/SimonCahill/getopt.net/actions/workflows/build.yaml/badge.svg)
![Test Workflow](https://github.com/SimonCahill/getopt.net/actions/workflows/run-tests.yaml/badge.svg)
[![Nuget Version](https://img.shields.io/nuget/v/getopt.net-bsd?logo=nuget)](https://www.nuget.org/packages/getopt.net-bsd/)
[![GitHub all releases](https://img.shields.io/github/downloads/SimonCahill/getopt.net/total?logo=github)](https://github.com/SimonCahill/getopt.net/releases)
[![Nuget Downloads](https://img.shields.io/nuget/dt/getopt.net-bsd?logo=nuget)](https://www.nuget.org/packages/getopt.net-bsd/)
![GitHub](https://img.shields.io/github/license/SimonCahill/getopt.net)
[![CodeQL](https://github.com/SimonCahill/getopt.net/actions/workflows/codeql.yml/badge.svg)](https://github.com/SimonCahill/getopt.net/actions/workflows/codeql.yml)

# Installation

There are several methods of installing and using getopt.net in your project.

1. Add the repository as a submodule, checkout a tag and include it as a project reference in your solution
2. Use the NuGet package manager: `install-package getopt.net-bsd` Note the `-bsd` ending which shows the license used and **not** system requirements! getopt.net was already in use 😕
3. Use the dotnet command-line tools: `dotnet add package getopt.net-bsd`

[NuGet page](https://www.nuget.org/packages/getopt.net-bsd/)

# Features

### Optional main commands and subcommands

Applications can define a single level of bare main commands. Each command owns a normal getopt option set, so all existing GNU/POSIX, Windows, PowerShell and parameter-file behavior remains available inside the command. Global options are parsed before the command; after a command is selected, only that command's options are active.

```csharp
var getopt = new GetOpt {
    AppArgs = args,
    Options = new[] {
        new Option("verbose", ArgumentType.None, 'v', "Enable verbose output.")
    },
    ShortOpts = "v",
    Commands = new[] {
        new MainCommand(
            "config",
            "Manage application configuration.",
            "cp",
            new Option("create", ArgumentType.None, 'c', "Create configuration."),
            new Option("purge", ArgumentType.None, 'p', "Purge configuration.")
        )
    }
};

while (getopt.GetNextOpt(out var optionArgument) is var option && option != -1) {
    // getopt.SelectedCommand is "config" while handling --create or --purge.
    // option contains 'c' for: myapp --verbose config --create
}
```

`SelectedCommand` is populated as soon as the bare command is consumed. A command is not required, even when `Commands` is configured, which permits global invocations such as `myapp --version`.

Command and long-option names remain case-sensitive by default. Set `CaseInsensitiveMatching = true` to use culture-independent case-insensitive matching; short options always remain case-sensitive.

When `IgnoreInvalidOptions` is `false`, an unknown command raises `CommandMisspeltException` if a plausible spelling correction is available, or `CommandNotFoundException` otherwise. Both exceptions expose the entered `Command` and a read-only `PossibleCommands` list. When invalid options are ignored, an unknown command behaves like other invalid input: `GetNextOpt` returns `GetOpt.InvalidOptChar` and places the entered command in `optionArgument`.

### Full support for getopt-like command-line options

**Separate options**

 - `--help`
    - `-h`
 - `--config=/path/to/config` POSIX separator
    - `--config /path/to/config` GNU separator
    - `-c /path/to/config` POSIX separator
    - `-c/path/to/config` GNU extension

**Compound options**
 - `--console, -C`
 - `--test, -t`
 - `--config, -c`
 - `-Ctc/path/to/config` Options with *required* or *optional* arguments go last!

### Support for options using the Windows convention

> **NOTE**: It is possible to use the Windows argument separator (`:`) with **all** conventions.  
> To enable this, you must set `AllowWindowsConventions` to `true`.

**Separate options**

 - `/help`
    - `/h`
 - `/config=/path/to/config` POSIX separator
    - `/config /path/to/config` GNU separator
    - `/config:/path/to/config` Windows separator
    - `/c /path/to/config` POSIX separator
    - `/c/path/to/config` GNU extension

**Compound options**
 - `/console, /C`
 - `/test, /t`
 - `/config, /c`
 - `/Ctc/path/to/config` Options with *required* or *optional* arguments go last!

### Support for Powershell-style options

**Separate options**

 - `-help`
    - `-h`
 - `-config=/path/to/config` POSIX separator
    - `-config /path/to/config` GNU separator
    - `-config:/path/to/config` Windows separator
    - `-c /path/to/config` POSIX separator
    - `-c/path/to/config` GNU extension

**Compound options**
 - `-console, -C`
 - `-test, -t`
 - `-config, -c`
 - `-Ctc/path/to/config` Options with *required* or *optional* arguments go last!

### Support for paramfiles

Some applications, such as GCC, allow passing of paramfile arguments.
A paramfile is a line-separated text file which contains one option (and argument) per line.
Each line of the paramfile is parsed as if it were passed to getopt.net directly.

Syntax:
```
myapp @/path/to/paramfile
```

### The standard getopt shortopt-string format is supported:

`:` denotes a **required** argument!

`;` denotes an **optional** argument!

If none of the above is present after a character in `ShortOpts`, then **no argument** is required.

```csharp
getopt.ShortOpts = "abC:dE:f:GhIjkLmnop:q:r;";
```

#### POSIXly correct behaviour
If `getopt.ShortOpts` is prefixed by a `+`, or the environment variable `POSIXLY_CORRECT` is set, then getopt.net will stop processing more options as soon when the first non-option string is found.

If `getopt.ShortOpts` is prefixed by a `-`, then each non-option string will be treated as if it were the argument to an option with the value `1`.


### Customisation is available with long opts:
```csharp
getopt.Options = new[] {
    new Option { Name = "help", ArgumentType = ArgumentType.None, Value = 'h' }, // brace-initialiser
    new Option("config", ArgumentType.Required, 'c'), // standard constructor
    new Option("version", ArgumentType.Optional, 'v')
};
```

### Fallback to long opts (if available!)
Most developers will have experienced this at some point when using `getopt`; you added an option to your long opts, but forgot it in your shortopt string.
getopt.net improves this behaviour and will check the `Options` array to see if the option you've provided is there.

### Customisable behaviour
getopt.net can be configured to not throw exceptions if that's your thing.
Just set the `IgnoreXXX` options to `true`, and getopt.net will ignore bad user input!

If `IgnoreInvalidOptions` is enabled, entering an unknown option won't throw an exception, but instead a `!` will be returned.
Non-option arguments do not throw when this setting is disabled; they are returned according to the configured parsing mode.
If `IgnoreMissingArguments` is enabled, forgetting to add a **required** argument won't thow an exception either! Instead, `?` will be returned.

The exceptions *do* contain more info, however.

### Help text generation
getopt.net can generate a help text for you, by simply calling `getopt.GenerateHelpText()`.

When commands are configured, generated help adds `[command] [command options]` to the usage and a `Commands` section. Each command's getopt options are indented by one additional four-space tab stop below their main command.

The behaviour of the help text generator can be customised to suit your needs.
By default, getopt.net will not output application name, version or copyright information. This must be provided with the `HelpTextConfig` object.

If no application name is provided, getopt.net will attempt to read the application name from the Assembly.  
Should this fail, getopt.net will identify your program as `unknown`.

The default configuration outputs options and switches using the GNU/POSIX convention and **doesn't** print the conventions supported by your application.

Here's an example:

```cs
var getopt = new GetOpt {
    AppArgs = args,
    Options = new[] {
        new Option("help",      ArgumentType.None,      'h', "Displays this help text."),
        new Option("version",   ArgumentType.None,      'v', "Displays the version of this program."),
        new Option("file",      ArgumentType.Required,  'f', "Reads the file back to stdout. The file is read into a local buffer and then printed out. Also I created this really long description to show that getopt.net can handle long descriptions."),
    },
    ShortOpts = "hvf:t;", // the last option isn't an error!
    AllowParamFiles = true,
    AllowWindowsConventions = true,
    AllowPowershellConventions = true
};

static void PrintHelp(GetOpt getopt) {
    Console.WriteLine(getopt.GenerateHelpText(new HelpTextConfig {
        ApplicationName = "getopt.net reference",
        ApplicationVersion = "v1.0.0",
        FooterText = "This is a reference implementation of getopt.net in C#.",
        OptionConvention = OptionConvention.GnuPosix, // Change me to different conventions
        ShowSupportedConventions = true // I'm false by default, but enabling me shows which conventions your app supports
    }));
}
```

produces the following output:

```
getopt.net reference v1.0.0 © 2023 Simon Cahill (contact@simonc.eu)

Usage:
    getopt.net reference [options]

Supported option conventions:
    Windows (/): yes
    Powershell (-): yes
    Gnu/Posix (-, --): yes

Switches:
    -h, --help    Displays this help text.
    -v, --version Displays the version of this program.

Options:
                  Reads the file back to stdout. The file is read into a local buffer and then
                  printed out. Also I created this really long description to show that getopt.net
    -f, --file    can handle long descriptions.

This is a reference implementation of getopt.net in C#.
```

# Usage:

For a more detailled description of using getopt.net, please consult the Wiki.

## Typed C# usage (v1.2.0)

The `getopt.net.Typed` namespace adds fluent and attribute-based APIs in the same NuGet package. Both use the existing getopt parser and accept the same command-line conventions. Existing `GetOpt` applications require no changes.

### Fluent configuration

Declare plain options classes and bind their properties in code:

```csharp
using getopt.net.Typed;

public class AppOptions {

    public bool Verbose { get; set; }

    public string Output { get; set; } = "output.txt";

    public int Retries { get; set; } = 3;

    public ConfigOptions? Config { get; set; }

}

public class ConfigOptions {

    public bool Create { get; set; }

    public string[] Arguments { get; set; } = Array.Empty<string>();

}
```

Inside your application's entry point:

```csharp
var parser = new ParserBuilder<AppOptions>()
    .AddFlag(options => options.Verbose, "verbose", 'v', "Enable verbose output.")
    .AddOption(options => options.Output, "output", 'o', "Output file.")
    .AddOption(options => options.Retries, "retries", 'r')
    .AddCommand(options => options.Config, "config", command => command
        .AddFlag(options => options.Create, "create", 'c')
        .AddArguments(options => options.Arguments), "Manage configuration.")
    .Build();

AppOptions options = parser.Parse(args);
```

For `myapp -v --output=result.txt config --create input.txt`, `options.Verbose` is true, `options.Output` is `"result.txt"`, and `options.Config` contains `Create = true` and the operand `"input.txt"`. Global options precede the command. A global-only invocation leaves `Config` null; `config` alone still creates its model.

### Attribute configuration

Alternatively, put the declarations on the model itself:

```csharp
using getopt.net.Typed;

public class AppOptions {

    [CliOption("verbose", ShortName = 'v', Description = "Enable verbose output.")]
    public bool Verbose { get; set; }

    [CliOption("output", ShortName = 'o', Description = "Output file.")]
    public string Output { get; set; } = "output.txt";

    [CliOption("retries", ShortName = 'r')]
    public int Retries { get; set; } = 3;

    [CliCommand("config", Description = "Manage configuration.")]
    public ConfigOptions? Config { get; set; }

}

public class ConfigOptions {

    [CliOption("create", ShortName = 'c')]
    public bool Create { get; set; }

    [CliArguments]
    public string[] Arguments { get; set; } = Array.Empty<string>();

}
```

Then use `CommandLine.Parse<AppOptions>(args)` or create a reusable parser with `CommandLine.CreateParser<AppOptions>()`. Both approaches return the same `Parser<AppOptions>` type. Fluent configuration reads attributes only when `.UseAttributes()` is explicitly called; duplicate bindings are rejected.

### Input errors and help

`Parse` throws `TypedParseException`, which derives from `ParseException`. `TryParse` returns the first input error without exposing a partially populated model:

```csharp
var parser = CommandLine.CreateParser<AppOptions>();
if (!parser.TryParse(args, out var options, out var error)) {
    Console.Error.WriteLine(error!.Message);
    Console.Error.WriteLine(parser.GenerateHelpText(new getopt.net.HelpTextConfig {
        ApplicationName = "myapp"
    }));

    return;
}

Console.WriteLine(options!.Output);
```

Errors expose `Kind`, `Name`, `Value` and `PossibleCommands`. Unknown options and missing values are errors in the typed API; legacy error settings remain unchanged. Configuration errors, exceptions from model constructors or setters, and file I/O failures still throw. Help generation does not print anything or reserve help/version aliases.

### Binding rules and conventions

- Use concrete classes with public parameterless constructors and public readable/writable properties. Command properties must initially be null. One level of commands is supported.
- Long names are explicit and have no prefixes. Short aliases are optional ASCII letters or digits. Names and aliases must be unique within each scope.
- Boolean values default to flags. Use `argumentType: ArgumentType.Required` in fluent configuration or `ArgumentType = ArgumentType.Required` on an attribute to accept an explicit boolean value instead. Import `getopt.net` for `ArgumentType`.
- Non-boolean values require an argument by default, but the option itself is optional. Set `required: true` or `Required = true` to require its presence.
- Use `ArgumentType.Optional` for optional arguments. Absent options and omitted optional values retain property initializers. An explicitly empty string remains empty; empty nullable values become null.
- Repeated scalar values use the last occurrence. `T[]` and `List<T>` collect occurrences after initialized defaults, without splitting commas.
- Supported values include strings, primitive numbers, booleans, enums, `Guid`, `DateTime`, `DateTimeOffset`, `TimeSpan`, nullable forms and types with string-capable `TypeConverter` implementations. Conversion uses invariant culture; enum names ignore case.
- Bind non-options with `AddArguments` or `[CliArguments]` on a `string[]`. Unbound non-options are errors. `--` ends option parsing by default.
- Configure `AllowWindowsConventions`, `AllowPowershellConventions`, `AllowParamFiles`, `CaseInsensitiveMatching` and `DoubleDashStopsParsing` through `ParserSettings`. Short aliases remain case-sensitive. Existing parameter-file ordering and `POSIXLY_CORRECT` behavior are retained.
- Each parse creates fresh models and copies the input array. Built parsers can be reused concurrently; later builder or settings changes do not affect them. Application-provided constructors, setters and converters remain responsible for their own external side effects.

For example, enable alternate conventions identically for either declaration style:

```csharp
var settings = new ParserSettings {
    AllowWindowsConventions = true,
    AllowPowershellConventions = true,
    AllowParamFiles = true
};

var attributedParser = CommandLine.CreateParser<AppOptions>(settings);
var fluentBuilder = new ParserBuilder<AppOptions>(settings);
```

## Basic usage in C♯

```csharp

using getopt.net;

static void Main(string[] args) {

    var getopt = new GetOpt {
        Options = new[] {
            new Option("help",    ArgumentType.None, 'h'),
            new Option("version", ArgumentType.None, 'v'),
            // or, alternatively
            new Option { Name = "config", ArgumentType = ArgumentType.Required, Value = 'c' }
        },
        ShortOpts = "hvc:",
        AppArgs = args, // REQUIRED
        OnlyShortOpts = false,
        // AllowWindowsConventions = true, // enable this for Windows-style options
        // other options here
    };

    int opt = 0;
    // GetNextOpt may throw exceptions, depending on your settings!
    while ((opt = getopt.GetNextOpt(out var optArg)) != -1) {
        switch (opt) {
            case 'h':
                // print help or something
                break;
            case 'c':
                // do something with optArg
                break;
        }
    }
}
```

## Basic usage in VB.Net

```vb
Imports getopt.net

module Program

    Dim _progOptions() As [Option] = {
        New [Option]("help", ArgumentType.None, "h"c),
        New [Option]("version", ArgumentType.None, "v"c),
        New [Option]("file", ArgumentType.Required, "f"c)
    }

    Dim _progShortOptions As String = "hvf:"

    sub Main(args as string())
        Dim getopt = New GetOpt With {
            .AppArgs = args,
            .Options = _progOptions,
            .ShortOpts = _progShortOptions,
            ' .AllowWindowsConventions = true ' enable me for Windows-style options!
        }

        Dim optChar = 0
        Dim optArg As String = Nothing
        Dim fileToRead As String = Nothing

        While optChar <> -1
            optChar = getopt.GetNextOpt(optArg)

            Select Case optChar
                Case Convert.ToInt32("h"c)
                    ' do something
                    Return
                Case Convert.ToInt32("v"c)
                    ' do something else
                    Return
                Case Convert.ToInt32("f"c)
                    ' do something with optArg
            End Select
        End While
    end sub

end module
```

# Bugs and errors

If you encounter a bug, please add a [GitHub Issue](https://github.com/SimonCahill/getopt.net/issues) and/or create a fork of the project and create a pull request.
