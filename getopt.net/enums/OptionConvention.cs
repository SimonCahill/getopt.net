using System;

namespace getopt.net {

    /// <summary>
    /// An enumeration of all supported option conventions supported by the <see cref="DescriptionGenerator" /> class.
    /// </summary>
    public enum OptionConvention {
        /// <summary>
        /// Print the help text using options formatted with POSIX/GNU conventions.
        /// </summary>
        PosixGnuConvention = 0,

        /// <summary>
        /// Print the help text using options formatted with Windows conventions.
        /// </summary>
        WindowsConvention = 1,

        /// <summary>
        /// Print the help text using options formatted with Powershell conventions.
        /// </summary>
        PowershellConvention = 2
    }
}

