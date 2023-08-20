using System;

namespace getopt.net {

    /// <summary>
    /// A description generator class which takes all <see cref="Option"/> values from a <see cref="GetOpt"/>
    /// instance and generates a "help text" for use by your application.
    /// </summary>
    partial class DescriptionGenerator {

        #region "Argument Convention"
        /// <summary>
        /// Gets the long option prefix for a given convention.
        /// </summary>
        /// <param name="convention">The convention for which to return the prefix.</param>
        /// <returns>
        ///  - "--" for GNU/POSIX convention long opts
        ///  - "/" for Windows convention long opts
        ///  - "-" for Powershell convention long opts
        /// </returns>
        public static string GetLongOptPrefixFor(OptionConvention convention) {
            switch (convention) {
                case OptionConvention.PosixGnuConvention:
                    return "--";
                case OptionConvention.WindowsConvention:
                    return "/";
                case OptionConvention.PowershellConvention:
                    return "-";
                default: throw new ArgumentOutOfRangeException(nameof(convention), "The convention must be a valid OptionConvention enumeration!");
            }
        }

        /// <summary>
        /// Gets the short option prefix for a given convention.
        /// </summary>
        /// <param name="convention">The convention for which to return the prefix.</param>
        /// <returns>
        ///  - "-" for GNU/POSIX, and Powershell convention short opts
        ///  - "/" for Windows convention short opts.
        /// </returns>
        public static string GetShortOptPrefixFor(OptionConvention convention) {
            switch (convention) {
                case OptionConvention.PosixGnuConvention:
                case OptionConvention.PowershellConvention:
                    return "-";
                case OptionConvention.WindowsConvention:
                    return "/";
                default: throw new ArgumentOutOfRangeException(nameof(convention), "The convention must be a valid OptionConvention enumeration!");
            }
        }
        #endregion

    }

}