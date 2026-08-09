Option Strict On

Imports System
Imports System.IO

Imports getopt.net

Module Program

    Dim _progOptions() As [Option] = {
        New [Option]("help", ArgumentType.None, "h"c, "Displays this help text."),
        New [Option]("version", ArgumentType.None, "v"c, "Displays the version of this program."),
        New [Option]("file", ArgumentType.Required, "f"c, "Reads a file back to stdout.")
    }

    Dim _commands() As MainCommand = {
        New MainCommand With {
            .Name = "config",
            .Description = "Manages the application's configuration.",
            .ShortOpts = "cp",
            .Options = New [Option]() {
                New [Option]("create", ArgumentType.None, "c"c, "Creates a new configuration."),
                New [Option]("purge", ArgumentType.None, "p"c, "Deletes the existing configuration.")
            }
        }
    }

    Dim _progShortOptions As String = "hvf:t;" ' the last option isn't an error!

    Sub Main(args As String())
        Dim getopt = New GetOpt With {
            .AppArgs = args,
            .Options = _progOptions,
            .Commands = _commands,
            .ShortOpts = _progShortOptions,
            .AllowParamFiles = True,
            .AllowPowershellConventions = True,
            .AllowWindowsConventions = True,
            .CaseInsensitiveMatching = True,
            .IgnoreInvalidOptions = False
        }

        Dim optChar = 0
        Dim optArg As String = Nothing
        Dim fileToRead As String = Nothing
        Dim commandHandled As Boolean = False

        Try
            While optChar <> -1
                optChar = getopt.GetNextOpt(optArg)

                Select Case optChar
                    Case Convert.ToInt32("h"c) ' this is a god awful syntax.
                        PrintHelp(getopt)
                        Return
                    Case Convert.ToInt32("v"c)
                        PrintVersion()
                        Return
                    Case Convert.ToInt32("f"c)
                        If optArg Is Nothing Then
                            Console.Error.WriteLine("Missing input file!")
                            Environment.ExitCode = 1
                            Return
                        End If
                        fileToRead = optArg
                    Case Convert.ToInt32("t"c)
                        Console.WriteLine($"You passed the option 't' with the argument { If(optArg, "(no argument supplied)") }")
                    Case Convert.ToInt32("c"c)
                        If getopt.SelectedCommand IsNot Nothing AndAlso getopt.SelectedCommand.Name = "config" Then
                            Console.WriteLine("Creating a new configuration...")
                            commandHandled = True
                        End If
                    Case Convert.ToInt32("p"c)
                        If getopt.SelectedCommand IsNot Nothing AndAlso getopt.SelectedCommand.Name = "config" Then
                            Console.WriteLine("Purging the existing configuration...")
                            commandHandled = True
                        End If
                End Select
            End While
        Catch exception As CommandMisspeltException
            Console.Error.WriteLine($"Unknown command '{exception.Command}'. Did you mean: {String.Join(", ", exception.PossibleCommands)}?")
            Environment.ExitCode = 3
            Return
        Catch exception As CommandNotFoundException
            Console.Error.WriteLine($"Unknown command '{exception.Command}'.")
            Environment.ExitCode = 3
            Return
        End Try

        If getopt.SelectedCommand IsNot Nothing Then
            If Not commandHandled Then
                Console.Error.WriteLine($"No action was provided for the '{getopt.SelectedCommand.Name}' command.")
                Environment.ExitCode = 1
            End If
            Return
        End If

        If (fileToRead Is Nothing) Then
            Console.Error.WriteLine("Nothing to read. Exiting...")
            Environment.ExitCode = 1
            Return
        End If

        If Not File.Exists(fileToRead) Then
            Console.Error.WriteLine("The file " & fileToRead & " doesn't exist!")
            Environment.ExitCode = 2
            Return
        End If

        Console.WriteLine("Got file: " & fileToRead)
        Console.WriteLine(File.ReadAllText(fileToRead))
    End Sub

    Sub PrintHelp(getopt As GetOpt)
        Console.WriteLine(getopt.GenerateHelpText(New HelpTextConfig With {
            .ApplicationName = "getopt.net reference (VB)",
            .ApplicationVersion = "v1.1.0",
            .FooterText = "Examples: myapp config --create | myapp config --purge",
            .OptionConvention = OptionConvention.GnuPosix,
            .ShowSupportedConventions = True
        }))
    End Sub

    Sub PrintVersion()
        Console.WriteLine("myapp (VB) v1.1.0")
    End Sub
End Module
