using System;
using System.Collections.Generic;
using System.Linq;

namespace getopt.net {

    /// <summary>
    /// Thrown when a command cannot be found but one or more plausible spelling corrections exist.
    /// </summary>
    public class CommandMisspeltException: ParseException {

        /// <summary>
        /// Creates a misspelt-command exception.
        /// </summary>
        /// <param name="command">The command supplied by the user.</param>
        /// <param name="possibleCommands">Possible intended command names.</param>
        public CommandMisspeltException(string command, IEnumerable<string> possibleCommands): this(command, possibleCommands.ToArray()) { }

        private CommandMisspeltException(string command, string[] possibleCommands): base(command, BuildMessage(command, possibleCommands)) {
            Command = command;
            PossibleCommands = Array.AsReadOnly(possibleCommands);
        }

        /// <summary>
        /// Gets the command supplied by the user.
        /// </summary>
        public string Command { get; }

        /// <summary>
        /// Gets the nearest plausible intended commands.
        /// </summary>
        public IReadOnlyList<string> PossibleCommands { get; }

        private static string BuildMessage(string command, IEnumerable<string> possibleCommands) {
            var commands = possibleCommands.ToArray();
            return $"Command '{command}' was not found. Did you mean: {string.Join(", ", commands)}?";
        }

    }
}
