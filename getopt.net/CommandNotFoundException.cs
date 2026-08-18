using System;
using System.Collections.Generic;
using System.Linq;

namespace getopt.net {

    /// <summary>
    /// Thrown when a command cannot be found and no plausible spelling correction exists.
    /// </summary>
    public class CommandNotFoundException: ParseException {

        /// <summary>
        /// Creates a command-not-found exception.
        /// </summary>
        /// <param name="command">The command supplied by the user.</param>
        /// <param name="possibleCommands">Possible intended command names.</param>
        public CommandNotFoundException(string command, IEnumerable<string>? possibleCommands = null): base(command, $"Command '{command}' was not found.") {
            Command = command;
            PossibleCommands = Array.AsReadOnly((possibleCommands ?? Enumerable.Empty<string>()).ToArray());
        }

        /// <summary>
        /// Gets the command supplied by the user.
        /// </summary>
        public string Command { get; }

        /// <summary>
        /// Gets possible intended commands. This list is empty when no plausible match exists.
        /// </summary>
        public IReadOnlyList<string> PossibleCommands { get; }

    }
}
