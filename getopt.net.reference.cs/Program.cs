using System;

namespace getopt.net.reference.cs {

    using System.IO;

    internal class Program {

        static int Main(string[] args) {
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

            var optChar = 0;
            var fileToRead = string.Empty;

            while ((optChar = getopt.GetNextOpt(out var optArg)) != -1) {
                switch (optChar) {
                    case 'h':
                        PrintHelp(getopt);
                        return 0;
                    case 'v':
                        PrintVersion();
                        return 0;
                    case 'f':
                        fileToRead = optArg;
                        break;
                    case 't':
                        Console.WriteLine($"You passed the option 't' with the argument { (optArg ?? "(no argument supplied)") }");
                        break;
                }
            }

            if (string.IsNullOrEmpty(fileToRead)) {
                Console.Error.WriteLine("No file to read. Exiting...");
                return 1;
            }

            if (!File.Exists(fileToRead)) {
                Console.Error.WriteLine($"{ fileToRead } doesn't exist! Exiting..");
                return 2;
            }

            Console.WriteLine($"Got file: { fileToRead }");
            Console.WriteLine(File.ReadAllText(fileToRead));
            return 0;
        }

        static void PrintHelp(GetOpt getopt) {
            Console.WriteLine(getopt.GenerateHelpText(new HelpTextConfig {
                ApplicationName = "getopt.net reference",
                ApplicationVersion = "v1.0.0",
                FooterText = "This is a reference implementation of getopt.net in C#.",
                OptionConvention = OptionConvention.GnuPosix,
                ShowSupportedConventions = true
            }));
        }

        static void PrintVersion() {
            Console.WriteLine("myapp v0.8.0");
        }
    }
}