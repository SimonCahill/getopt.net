using System;

namespace getopt.net {

    using System.Linq;
    using System.Text;

    /// <summary >
    /// This class contains extension methods specific to getopt.net.
    /// If these extension methods help you in your program, you're free to use them too!
    /// </summary>
    public static class Extensions {

        /// <summary>
        /// Finds an option with the name <paramref name="optName" />
        /// </summary>
        /// <param name="list">The list of options to search.</param>
        /// <param name="optName">The name of the argument to search for.</param>
        /// <returns>The <see cref="Option" /> with the name <paramref name="optName" />, or <code >null</code> if no option was found matching the name.</returns>
        public static Option? FindOptionOrDefault(this Option[] list, string optName) {
            if (string.IsNullOrEmpty(optName)) { throw new ArgumentNullException(nameof(optName), "optName must not be null!"); }

            return Array.Find(list, o => o.Name?.Equals(optName, StringComparison.InvariantCulture) == true);
        }

        /// <summary>
        /// Finds an option in the list <paramref name="list" /> with the <see cref="Option.Value" /> <paramref name="optVal" />.
        /// </summary>
        /// <param name="list">The list of options to search.</param>
        /// <param name="optVal">The value to search for.</param>
        /// <returns>The <see cref="Option" /> with the <see cref="Option.Value" /> <paramref name="optVal" />, or <code >null</code> if no option was found matching the name.</returns>
        public static Option? FindOptionOrDefault(this Option[]? list, char optVal) => list?.FirstOrDefault(o => o.Value == optVal);

        /// <summary>
        /// Creates a short opt string from an array of <see cref="Option"/> objects.
        /// </summary>
        /// <param name="list">The options to convert.</param>
        /// <returns><code>null</code> if the option list is empty or null. A string contain a shortopt-form string representing all the options from <paramref name="list"/>.</returns>
        public static string? ToShortOptString(this Option[]? list) {
            if (list is null || list.Length == 0) { return null; }

            var sBuilder = new StringBuilder();

            foreach (var opt in list) {
                sBuilder.Append((char)opt.Value);
                switch (opt.ArgumentType) {
                    case ArgumentType.Required:
                        sBuilder.Append(':');
                        break;
                    case ArgumentType.Optional:
                        sBuilder.Append(';');
                        break;
                    default: break;
                }
            }

            return sBuilder.ToString();
        }

        /// <summary>
        /// Generates a help text from the arguments contained in <paramref name="getopt" />. See <see cref="GetOpt.Options" /> for more information.
        /// The method also takes a <see cref="HelpTextConfig" /> object to generate a help text. (<paramref name="generatorOptions" />)
        /// If <paramref name="generatorOptions" /> is null, it will be assigned the value <see cref="HelpTextConfig.Default" />.
        /// 
        /// The help text is generated in the following format:
        /// <code>
        /// programName programVersion
        /// 
        /// Usage:
        ///     programName [options]
        /// 
        /// Switches:
        ///     ...
        /// 
        /// Options:
        ///    ...
        /// 
        /// footerText
        /// </code>
        /// 
        /// If <see cref="HelpTextConfig.ApplicationName"/> or <see cref="HelpTextConfig.ApplicationVersion" /> is null or empty, the first line will be omitted and the assembly name will be used in the usage.
        /// If <see cref="HelpTextConfig.FooterText" /> is null or empty, the footer will be omitted.
        /// 
        /// The switches section will only contain options with the ArgumentType set to <see cref="ArgumentType.None" />.
        /// The options section will only contain options with the ArgumentType set to <see cref="ArgumentType.Optional" /> or <see cref="ArgumentType.Required" />.
        /// 
        /// Each line containing the description of an option will be formatted as follows:
        /// <code>
        /// -s, --long-switch    Description of the switch
        /// </code>
        /// 
        /// where the lines will be justified to the longest name.
        /// </summary>
        /// <param name="getopt">The instance of <see cref="GetOpt"/> to use.</param>
        /// <param name="generatorOptions">(Optional) Customised generator configuration.</param>
        /// <returns>A string value containing the help text.</returns>
        public static string GenerateHelpText(this GetOpt getopt, HelpTextConfig? generatorOptions = null) {
            if (getopt is null) { throw new ArgumentNullException(nameof(getopt), "getopt must not be null!"); }

            var options = getopt.Options;
            if (options is null || options.Length == 0) { return string.Empty; }

            var config = generatorOptions ?? HelpTextConfig.Default;

            var sBuilder = new StringBuilder();

            if (!string.IsNullOrEmpty(config.ApplicationName) && !string.IsNullOrEmpty(config.ApplicationVersion)) {
                sBuilder.Append($"{config.ApplicationName} {config.ApplicationVersion}");
                if (config.CopyrightDate is not null && !string.IsNullOrEmpty(config.CopyrightHolder)) {
                    sBuilder.Append($" © {config.CopyrightDate.Value.Year} {config.CopyrightHolder}");
                }
                sBuilder.AppendLine()
                        .AppendLine();
            }

            string shortOptPrefix = config.OptionConvention == OptionConvention.Windows ? "/" : "-";
            string longOptPrefix = config.OptionConvention == OptionConvention.Windows ? "/" : config.OptionConvention == OptionConvention.GnuPosix ? "--" : "-";

            sBuilder.AppendLine("Usage:");
            sBuilder.AppendLine($"\t{config.ApplicationName ?? GetApplicationName()} [options]");
            if (config.ShowSupportedConventions) {
                sBuilder.AppendLine(
                    $"""

                    Supported option conventions:
                        Windows (/): {(getopt.AllowWindowsConventions ? "yes" : "no")}
                        Powershell (-): {(getopt.AllowPowershellConventions ? "yes" : "no")}
                        Gnu/Posix (-, --): yes
                    """
                );
            }
            sBuilder.AppendLine();

            var longestName = options.Max(o => o.Name?.Length ?? 0);

            sBuilder.AppendLine("Switches:");
            foreach (var opt in options.Where(o => o.ArgumentType == ArgumentType.None)) {
                sBuilder.AppendLine($"\t{shortOptPrefix}{(char)opt.Value}, {longOptPrefix:longestName}{opt.Name}\t{opt.Description}");
            }
            sBuilder.AppendLine();

            sBuilder.AppendLine("Options:");
            foreach (var opt in options.Where(o => o.ArgumentType == ArgumentType.Optional || o.ArgumentType == ArgumentType.Required)) {
                var line = $"\t{shortOptPrefix}{(char)opt.Value}, {longOptPrefix:longestName}{opt.Name}\t{opt.Description ?? string.Empty}";
                // If line is > config.MaxWidth, split it into multiple lines and align the description
                if (line.Length > config.MaxWidth) {
                    var desc = opt.Description ?? string.Empty;
                    while (desc.Length > config.MaxWidth - longestName - 10) {
                        var split = desc.Substring(0, config.MaxWidth - longestName - 10);
                        var splitIndex = split.LastIndexOf(' ');
                        sBuilder.AppendLine($"\t{new string(' ', longestName + 9)}{split.Substring(0, splitIndex)}");
                        desc = desc.Substring(splitIndex + 1);
                    }
                    sBuilder.AppendLine($"\t{shortOptPrefix}{(char)opt.Value}, {longOptPrefix:longestName}{opt.Name}\t{desc}");
                } else {
                    sBuilder.AppendLine(line);
                }

                sBuilder.AppendLine();
            }
            sBuilder.AppendLine();

            if (!string.IsNullOrEmpty(config.FooterText)) {
                sBuilder.AppendLine(config.FooterText);
            }

            return sBuilder.ToString();
        }

        /// <summary>
        /// Gets the name of the application.
        /// </summary>
        public static string GetApplicationName() {
            return System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "Unknown";
        }

    }
}

