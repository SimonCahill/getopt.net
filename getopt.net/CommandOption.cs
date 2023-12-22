using System;

namespace getopt.net {

    /// <summary>
    /// Represents a single argument received via command-line options.
    /// </summary>
    public class CommandOption {

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CommandOption() { }

        /// <summary>
        /// Object constructor - instantiates a new instance of this class.
        /// </summary>
        /// <param name="optChar">The value of the option passed.</param>
        /// <param name="optArg">The option's argument (if any)</param>
        public CommandOption(int optChar, object? optArg = null) {
            ArgumentType = optArg?.GetType();
            OptionArgument = optArg;
        }

        /// <summary>
        /// The option character that was received.
        /// </summary>
        public int OptChar { get; set; }

        /// <summary>
        /// The type of the argument that was received.
        /// 
        /// Possible types are int, string, bool, double
        /// </summary>
        public Type? ArgumentType { get; set; }

        /// <summary>
        /// The actual argument passed to the application.
        /// </summary>
        public object? OptionArgument { get; set; }

    }
}
