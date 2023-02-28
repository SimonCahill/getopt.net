namespace getopt.net.reference.cs {

    internal class Program {

        static int Main(string[] args) {
            var getopt = new GetOpt {
                AppArgs = args,
                Options = new[] {
                    new Option("help",      ArgumentType.None,      'h'),
                    new Option("version",   ArgumentType.None,      'v'),
                    new Option("file",      ArgumentType.Required,  'f')
                },
                ShortOpts = "hvf:"
            };

            var optChar = 0;
            var fileToRead = string.Empty;

            while ((optChar = getopt.GetNextOpt(out var optArg)) != -1) {
                switch (optChar) {
                    case 'h':
                        PrintHelp();
                        return 0;
                    case 'v':
                        PrintVersion();
                        return 0;
                    case 'f':
                        fileToRead = optArg;
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

        static void PrintHelp() {
            Console.WriteLine(
            """
            myapp (C#) v1.0.0
            This app displays the usage of getopt.net in C#.

            Usage:
                myapp [-h/--help] [-v/--version]
                myapp -f/path/to/file
                myapp -f /path/to/file
                myapp --file=/path/to/file
                myapp --file /path/to/file
            
            Arguments:
                --help,     -h      Displays this menu and exits
                --version,  -v      Displays the version and exits
                --file=<>,  -f<>    Reads the file back to stdout.
            """);
        }

        static void PrintVersion() {
            Console.WriteLine("myapp v1.0.0");
        }
    }
}