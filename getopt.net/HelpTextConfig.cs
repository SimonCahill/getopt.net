using System;

namespace getopt.net {

    /// <summary>
    /// A class that contains configuration options for the help text generator.
    /// </summary>
    public sealed class HelpTextConfig {

        /// <summary>
        /// Whether to show the supported conventions in the help text.
        /// </summary>
        public bool ShowSupportedConventions { get; set; } = false;

        /// <summary>
        /// The maximum width of the help text.
        /// </summary>
        public int MaxWidth { get; set; } = 100;

        /// <summary>
        /// The option convention to use.
        /// </summary>
        public OptionConvention OptionConvention { get; set; } = OptionConvention.GnuPosix;

        /// <summary>
        /// The name of the application.
        /// </summary>
        public string? ApplicationName { get; set; }

        /// <summary>
        /// The version of the application.
        /// </summary>
        public string? ApplicationVersion { get; set; }

        /// <summary>
        /// A footer text to be displayed under the help text.
        /// </summary>
        public string? FooterText { get; set; }

        /// <summary>
        /// Gets a default configuration.
        /// </summary>
        public static HelpTextConfig Default => GnuConfig();

        /// <summary>
        /// Gets a configuration for the GNU/POSIX convention.
        /// </summary>
        public static HelpTextConfig GnuConfig() => new HelpTextConfig {
            MaxWidth = 160,
            OptionConvention = OptionConvention.GnuPosix,
            ApplicationName = null,
            ApplicationVersion = null,
            FooterText = null
        };

        /// <summary>
        /// Gets a configuration for the Windows convention.
        /// </summary>
        public static HelpTextConfig WindowsConfig() => new HelpTextConfig {
            MaxWidth = 160,
            OptionConvention = OptionConvention.Windows,
            ApplicationName = null,
            ApplicationVersion = null,
            FooterText = null
        };

        /// <summary>
        /// Gets a configuration for the Powershell convention.
        /// </summary>
        public static HelpTextConfig PowershellConfig() => new HelpTextConfig {
            MaxWidth = 160,
            OptionConvention = OptionConvention.Powershell,
            ApplicationName = null,
            ApplicationVersion = null,
            FooterText = null
        };

    }

    /// <summary>
    /// An enumeration of option conventions.
    /// </summary>
    [Flags]
    public enum OptionConvention {

        /// <summary>
        /// The GNU/POSIX convention.
        /// </summary>
        GnuPosix,

        /// <summary>
        /// The Windows convention.
        /// </summary>
        Windows,

        /// <summary>
        /// The Powershell convention.
        /// </summary>
        Powershell,

    }

}