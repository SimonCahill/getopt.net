# getopt.net - A GNU and POSIX getopt port to .net

This repository contains the code for my port of the GNU getopt functionality found on most Unix-like systems.

getopt.net is written entirely in C# and is a "cleanroom port"; although not necessary it made the project that much more fun :)

<p align="center" >
    <img src="./img/getopt.net-logo-128.png" alt="getopt.net-logo" >
</p>

![Build Workflow](https://github.com/SimonCahill/getopt.net/actions/workflows/build.yaml/badge.svg)
![Test Workflow](https://github.com/SimonCahill/getopt.net/actions/workflows/run-tests.yaml/badge.svg)
[![Nuget Version](https://img.shields.io/nuget/v/getopt.net-bsd?logo=nuget)](https://www.nuget.org/packages/getopt.net-bsd/)
[![GitHub all releases](https://img.shields.io/github/downloads/SimonCahill/getopt.net/total?logo=github)](https://github.com/SimonCahill/getopt.net/Releases)
[![Nuget Downloads](https://img.shields.io/nuget/dt/getopt.net-bsd?logo=nuget)](https://www.nuget.org/packages/getopt.net-bsd/)
![GitHub](https://img.shields.io/github/license/SimonCahill/getopt.net)

# Installation

There are several methods of installing and using getopt.net in your project.

1. Add the repository as a submodule, checkout a tag and include it as a project reference in your solution
2. Use the NuGet package manager: `install-package getopt.net-bsd` Note the `-bsd` ending which shows the license used and **not** system requirements! getopt.net was already in use :c

[NuGet page](https://www.nuget.org/packages/getopt.net-bsd/)

# Features

### Full support for getopt-like command-line options

 - `--long-opts`
 - `--long-opts=with_options`
 - `--long-opts with_options`
 - `-ShortOpTs`
 - `-ShortsWithOpTi0n5`
 - `-s with_opts`

### Support for options using the Windows convention

 - `/h` (short opts)
 - `/long-opts`
 - `/hxcfsdf` (GNU-style short opts!)
 - `/long-opts:with-win-style-args`
 - `/long-opts with-args-separated-by-space`
 - `/long-opts=with-posix-separator`
 - `/fmyfile` (short opts with parameters!)

### The standard getopt shortopt-string format is supported:
```csharp
getopt.ShortOpts = "abC:dE:f:GhIjkLmnop:q:r:";
```

### Customisation is available with long opts:
```csharp
getopt.Options = new[] {
    new Option { Name = "help", ArgumentType = ArgumentType.None, Value = 'h' },
    new Option("config", ArgumentType.Required, 'c'),
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
If `IgnoreMissingArguments` is enabled, forgetting to add a **required** argument won't thow an exception either! Instead, `?` will be returned.

The exceptions *do* contain more info, however.

# Usage:

For a more detailled description of using getopt.net, please consult the Wiki.

## Basic usage in C#

```csharp

using getopt.net;

static void Main(string[] args) {

    var getopt = new Getopt {
        Options = new[] {
            new Option("help",    ArgumentType.None, 'h'),
            new Option("version", ArgumentType.None, 'v'),
            // or, alternatively
            new Option { Name = "config", ArgumentType.Required, 'c' }
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
            //
        }
    }
}
```

## Basic usage in VB.Net

```vbnet
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
