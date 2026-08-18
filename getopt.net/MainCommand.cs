using System;

namespace getopt.net {

    /// <summary>
    /// Represents a top-level command and the getopt-style options that belong to it.
    /// </summary>
    public class MainCommand {

        /// <summary>
        /// Creates an empty command for use with object initializers.
        /// </summary>
        public MainCommand() { }

        /// <summary>
        /// Creates a command with its supported options.
        /// </summary>
        /// <param name="name">The bare command name, without option prefixes.</param>
        /// <param name="description">The command description used by the help generator.</param>
        /// <param name="shortOpts">The getopt short-option string used within this command.</param>
        /// <param name="options">The long options used within this command.</param>
        public MainCommand(string name, string description = "", string? shortOpts = null, params Option[] options) {
            Name = name;
            Description = description;
            ShortOpts = shortOpts;
            Options = options ?? Array.Empty<Option>();
        }

        /// <summary>
        /// Gets or sets the bare command name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the command description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the getopt short-option string used within this command.
        /// </summary>
        public string? ShortOpts { get; set; }

        /// <summary>
        /// Gets or sets the long options used within this command.
        /// </summary>
        public Option[] Options { get; set; } = Array.Empty<Option>();

    }
}
