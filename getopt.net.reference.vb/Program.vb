Option Strict On

Imports System
Imports System.IO

Imports getopt.net

Module Program

    Dim _progOptions() As [Option] = {
        New [Option]("help", ArgumentType.None, "h"c),
        New [Option]("version", ArgumentType.None, "v"c),
        New [Option]("file", ArgumentType.Required, "f"c)
    }

    Dim _progShortOptions As String = "hvf:"

    Sub Main(args As String())
        Dim getopt = New GetOpt With {
            .AppArgs = args,
            .Options = _progOptions,
            .ShortOpts = _progShortOptions
        }

        Dim optChar = 0
        Dim optArg As String = Nothing
        Dim fileToRead As String = Nothing

        While optChar <> -1
            optChar = getopt.GetNextOpt(optArg)

            Select Case optChar
                Case Convert.ToInt32("h"c) ' this is a god awful syntax.
                    PrintHelp()
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
            End Select
        End While

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

    Sub PrintHelp()
        Console.WriteLine("
myapp (VB) v1.0.0
Displays the usage of getopt.net in VB

Usage:
    myapp [-h] [-v]
    myapp -f/path/to/file
    myapp -f /path/to/file
    myapp --file=/path/to/file
    myapp --file /path/to/file

Arguments:
    --help,         -h      Displays this text and exits
    --version,      -v      Displays the version and exits
    --file=[file],  -f      Reads a file and outputs its contents
        ")
    End Sub

    Sub PrintVersion()
        Console.WriteLine("myapp (VB) v1.0.0")
    End Sub
End Module
