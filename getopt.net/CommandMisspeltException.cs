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

        /// <summary>
        /// Initializes the error from an already materialized suggestion list.
        /// </summary>
        /// <param name="command">The entered command spelling.</param>
        /// <param name="possibleCommands">Suggested command names in declaration order.</param>
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

        /// <summary>
        /// Formats an unknown-command error with the nearest suggested spellings.
        /// </summary>
        /// <param name="command">The entered command spelling.</param>
        /// <param name="possibleCommands">Suggested command names to include in the message.</param>
        /// <returns>A human-readable message containing the command and its suggestions.</returns>
        private static string BuildMessage(string command, IEnumerable<string> possibleCommands) {
            var commands = possibleCommands.ToArray();
            return $"Command '{command}' was not found. Did you mean: {string.Join(", ", commands)}?";
        }

    }
}
