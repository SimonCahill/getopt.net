using System;

namespace getopt.net {

    /// <summary>
    /// Event arguments for option-related events.
    /// </summary>
    public class OptionEventArgs: EventArgs {

        /// <summary>
        /// Constructor accepting the option string and its argument (if any).
        /// </summary>
        /// <param name="option">The option string.</param>
        /// <param name="argument">The option argument, or null if none.</param>
        public OptionEventArgs(Option option, string? argument) {
            Option = option;
            Argument = argument;
        }

        /// <summary>
        /// The option string.
        /// </summary>
        public Option Option { get; set; }

        /// <summary>
        /// The option argument, or null if none.
        /// </summary>
        public string? Argument { get; set; }

        /// <summary>
        /// Indicates whether the option has an argument.
        /// </summary>
        public bool HasArgument => Argument is not null;

        /// <summary>
        /// Indicates whether parsing should continue after this event.
        /// </summary>
        public bool ContinueParsing { get; set; } = true;


    }
}