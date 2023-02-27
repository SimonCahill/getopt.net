# getopt.net - A GNU getopt port to .net

This repository contains the code for my port of the GNU getopt functionality found on most Unix-like systems.

getopt.net is written entirely in C# and is a "cleanroom port"; although not necessary it made the project that much more fun :)

![Test Workflow](https://github.com/SimonCahill/getopt.net/actions/workflows/build.yaml/badge.svg)
![Test Workflow](https://github.com/SimonCahill/getopt.net/actions/workflows/run-tests.yaml/badge.svg)

# Installation

There are several methods of installing and using getopt.net in your project.

1. Add the repository as a submodule, checkout a tag and include it as a project reference in your solution
2. Use the NuGet package manager: `install-package getopt.net-bsd` Note the `-bsd` ending which shows the license used and **not** system requirements! getopt.net was already in use :c

[NuGet page](https://www.nuget.org/packages/getopt.net-bsd/)

# Features

### Full support for getopt-like command-line options:

 - `--long-opts`
 - `--long-opts=with_options`
 - `--long-opts with_options`
 - `-ShortOpTs`
 - `-ShortsWithOpTi0n5`
 - `-s with_opts`

### The standard getopt shortopt-string format is supported:
```csharp
getopt.ShortOpts = "abC:dE:f:GhIjkLmnop:q:r:";
```

### Customisation is available with long opts:
```csharp
getopt.Options = new[] {
    new Option { Name = "help", ArgumentType = ArgumentType.None, Flag = IntPtr.Zero, Value = 'h' },
    new Option("config", ArgumentType.Required, IntPtr.Zero, 'c'),
    new Option("version", ArgumentType.Optional, IntPtr.Zero, 'v')
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

At the time of writing this, I have not figured out all the details of using the library!
Anything written here, until noted otherwise, is **subject to change!**

# Things that __don't__ work

I forgot to implement the whole "provide a flag pointer" functionality.
I'll do that later; it needs unsafe code anyway, so it'll have to be in its own little method.

## Basic usage

```csharp

using getopt.net;

static void Main(string[] args) {

    var getopt = new Getopt {
        Options = new[] {
            new Option("help",    ArgumentType.None, IntPtr.Zero, 'h'),
            new Option("version", ArgumentType.None, IntPtr.Zero, 'v'),
            // or, alternatively
            new Option { Name = "config", ArgumentType.Required, IntPtr.Zero, 'c' }
        },
        ShortOpts = "hvc:",
        DoubleDashStopsParsing = true, // optional
        AppArgs = args, // REQUIRED
        OnlyShortOpts = false,
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

# Bugs and errors

If you encounter a bug, please add a [GitHub Issue](https://github.com/SimonCahill/getopt.net/issues) and/or create a fork of the project and create a pull request.
