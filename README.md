# getopt.net - A GNU getopt port to .net

This repository contains the code for my port of the GNU getopt functionality found on most Unix-like systems.

getopt.net is written entirely in C# and is a "cleanroom port"; although not necessary it made the project that much more fun :)

# Usage:

At the time of writing this, I have not figured out all the details of using the library!
Anything written here, until noted otherwise, is **subject to change!**

## Basic usage

```csharp

using getopt.net;

static void Main(string[] args) {

    var getopt = new Getopt {
        Options = new[] {
            new Option("help",    ArgumentType.None, null, 'h'),
            new Option("version", ArgumentType.None, null, 'v'),
            new Option("config",  Argument.Required, null, 'c')
	},
        ShortOpts = "hvc:",
        DoubleDashStopsParsing = true, // optional
        AppArgs = args, // REQUIRED
        OnlyShortOpts = false,
        // other options here
    };

    int opt = 0;
    // GetNextOpt may throw exceptions, depending on your settings!
    while (getopt.GetNextOpt(out var optArg) != -1) {
        switch (opt) {
            case 'h':
                // print help or something
                break;
            //
        }
    }
}

```
