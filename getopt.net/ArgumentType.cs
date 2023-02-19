using System;

namespace getopt.net {

    /// <summary>
    /// Enumeration containing the argument types possible for getopt.
    /// </summary>
    public enum ArgumentType {

        /// <summary>
        /// No argument is required for the option.
        /// </summary>
        None,

        /// <summary>
        /// The option has a mandatory argument.
        /// </summary>
        Required,

        /// <summary>
        /// Arguments are optional for this argument.
        /// </summary>
        Optional

    }
}

