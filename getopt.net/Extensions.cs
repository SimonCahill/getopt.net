using System;

namespace getopt.net {

    using System.Linq;
    using System.Text;

    /// <summary>
    /// This enumeration contains different prefixes for the generation of shortopt strings.
    /// </summary>
    public enum OptStringPrefix {
        /// <summary>
        /// No prefix.
        /// </summary>
        None,

        /// <summary>
        /// If the first character of optstring is '+' or the environment variable POSIXLY_CORRECT is set, then option processing stops as soon as a nonoption argument is encountered.
        /// </summary>
        Plus = '+',

        /// <summary>
        /// If the first character of optstring is '-', then each nonoption argv-element is handled as if it were the argument of an option with character code 1.
        /// </summary>
        Minus = '-'
    }

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
            return FindOptionOrDefault(list, optName, StringComparison.InvariantCulture);
        }

        /// <summary>
        /// Finds an option using the requested string comparison.
        /// </summary>
        /// <param name="list">The list of options to search.</param>
        /// <param name="optName">The name of the argument to search for.</param>
        /// <param name="comparison">The comparison used for option names.</param>
        /// <returns>The matching option, or <code>null</code>.</returns>
        public static Option? FindOptionOrDefault(this Option[] list, string optName, StringComparison comparison) {
            if (string.IsNullOrEmpty(optName)) { throw new ArgumentNullException(nameof(optName), "optName must not be null!"); }

            foreach (var option in list) {
                if (option.Name?.Equals(optName, comparison) == true) { return option; }
            }

            return null;
        }

        /// <summary>
        /// Finds an option in the list <paramref name="list" /> with the <see cref="Option.Value" /> <paramref name="optVal" />.
        /// </summary>
        /// <param name="list">The list of options to search.</param>
        /// <param name="optVal">The value to search for.</param>
        /// <returns>The <see cref="Option" /> with the <see cref="Option.Value" /> <paramref name="optVal" />, or <code >null</code> if no option was found matching the name.</returns>
        public static Option? FindOptionOrDefault(this Option[]? list, char optVal) {
            if (list is null) { return null; }

            foreach (var option in list) {
                if (option.Value == optVal) { return option; }
            }

            return null;
        }

        /// <summary>
        /// Creates a short opt string from an array of <see cref="Option"/> objects.
        /// </summary>
        /// <param name="list">The options to convert.</param>
        /// <returns><code>null</code> if the option list is empty or null. A string contain a shortopt-form string representing all the options from <paramref name="list"/>.</returns>
        public static string ToShortOptString(this Option[] list) {
            if (list is null || list.Length == 0) { return string.Empty; }

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
        /// Creates a short opt string from an array of <see cref="Option"/> objects.
        /// </summary>
        /// <param name="list">The options to convert.</param>
        /// <param name="prefix">The prefix to use for the shortopt string.</param>
        /// <returns><code>null</code> if the option list is empty or null. A string contain a shortopt-form string representing all the options from <paramref name="list"/>.</returns>
        public static string ToShortOptString(this Option[] list, OptStringPrefix prefix) {
            if (list is null || list.Length == 0) { return string.Empty; }

            var sBuilder = new StringBuilder();
            if (prefix != OptStringPrefix.None) {
                sBuilder.Append((char)prefix);
            }

            sBuilder.Append(list.ToShortOptString());

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
        /// Commands:
        ///     command    Description
        ///         -s, --subcommand    Description
        ///
        /// footerText
        /// </code>
        ///
        /// If <see cref="HelpTextConfig.ApplicationName"/> or <see cref="HelpTextConfig.ApplicationVersion" /> is null or empty, the first line will be omitted and the assembly name will be used in the usage.
        /// If <see cref="HelpTextConfig.FooterText" /> is null or empty, the footer will be omitted.
        ///
        /// The switches section will only contain options with the ArgumentType set to <see cref="ArgumentType.None" />.
        /// The options section will only contain options with the ArgumentType set to <see cref="ArgumentType.Optional" /> or <see cref="ArgumentType.Required" />.
        /// When <see cref="GetOpt.Commands"/> is configured, the commands section lists each main command and indents its options by one additional tab stop.
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
            const string Tab = "    ";
            if (getopt is null) { throw new ArgumentNullException(nameof(getopt), "getopt must not be null!"); }

            var options = getopt.Options ?? Array.Empty<Option>();
            var commands = getopt.Commands ?? Array.Empty<MainCommand>();
            if (options.Length == 0 && commands.Length == 0) { return string.Empty; }

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

            var shortOptPrefix = config.OptionConvention == OptionConvention.Windows ? "/" : "-";
            var longOptPrefix = config.OptionConvention switch
            {
                OptionConvention.Windows => "/",
                OptionConvention.GnuPosix => "--",
                _ => "-"
            };

            sBuilder.AppendLine("Usage:");
            sBuilder.AppendLine(
                commands.Length == 0
                    ? $"{Tab}{config.ApplicationName ?? GetApplicationName()} [options]"
                    : $"{Tab}{config.ApplicationName ?? GetApplicationName()} [options] [command] [command options]"
            );
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

            var longestName = options.Length == 0 ? 0 : options.Max(o => o.Name?.Length ?? 0);
            // Align longestName to the next multiple of 4
            longestName = (longestName + 3) / 4 * 4;

            sBuilder.AppendLine("Switches:");
            foreach (var opt in options.Where(o => o.ArgumentType == ArgumentType.None)) {
                sBuilder.AppendLine($"{Tab}{shortOptPrefix}{(char)opt.Value}, {longOptPrefix}{opt.Name?.PadRight(longestName)}{opt.Description ?? string.Empty}");
            }
            sBuilder.AppendLine();

            sBuilder.AppendLine("Options:");
            foreach (var opt in options.Where(o => o.ArgumentType != ArgumentType.None)) {
                var line = $"{Tab}{shortOptPrefix}{(char)opt.Value}, {longOptPrefix}{opt.Name?.PadRight(longestName)}{opt.Description ?? string.Empty}";

                // If line is > config.MaxWidth, split it into multiple lines and align the description
                if (line.Length > config.MaxWidth) {
                    var desc = opt.Description ?? string.Empty;
                    var beginWhitespace = new string(
                        ' ',
                        Tab.Length +
                        shortOptPrefix.Length +
                        1 +
                        longOptPrefix.Length +
                        (opt.Name?.PadRight(longestName).Length ?? 0) +
                        2 // these last two are the missing space and comma between the long and short opt
                    );

                    while (desc.Length > config.MaxWidth - longestName - 10) {
                        var split = desc.Substring(0, config.MaxWidth - longestName - 10);
                        var splitIndex = split.LastIndexOf(' ');

                        sBuilder.AppendLine($"{beginWhitespace}{split.Substring(0, splitIndex)}");
                        desc = desc.Substring(splitIndex + 1);
                    }

                    sBuilder.AppendLine($"{Tab}{shortOptPrefix}{(char)opt.Value}, {longOptPrefix}{opt.Name?.PadRight(longestName)}{desc}");
                } else {
                    sBuilder.AppendLine(line);
                }

                sBuilder.AppendLine();
            }
            sBuilder.AppendLine();

            if (commands.Length > 0) {
                var longestCommandName = commands.Max(command => command.Name?.Length ?? 0);
                var commandOptions = commands.SelectMany(command => command.Options ?? Array.Empty<Option>()).ToArray();
                var longestCommandOptionName = commandOptions.Length == 0 ? 0 : commandOptions.Max(option => option.Name?.Length ?? 0);
                longestCommandOptionName = (longestCommandOptionName + 3) / 4 * 4;

                sBuilder.AppendLine("Commands:");
                foreach (var command in commands) {
                    AppendHelpLine(sBuilder, Tab, command.Name.PadRight(longestCommandName), command.Description, config.MaxWidth);

                    foreach (var option in command.Options ?? Array.Empty<Option>()) {
                        var optionLabel = $"{shortOptPrefix}{(char)option.Value}, {longOptPrefix}{option.Name?.PadRight(longestCommandOptionName)}";
                        AppendHelpLine(sBuilder, Tab + Tab, optionLabel, option.Description ?? string.Empty, config.MaxWidth);
                    }
                }
                sBuilder.AppendLine();
            }

            if (!string.IsNullOrEmpty(config.FooterText)) {
                sBuilder.AppendLine(config.FooterText);
            }

            return sBuilder.ToString();
        }

        private static void AppendHelpLine(StringBuilder builder, string indentation, string label, string description, int maxWidth) {
            var descriptionPrefix = $"{indentation}{label} ";
            if (string.IsNullOrEmpty(description) || descriptionPrefix.Length + description.Length <= maxWidth) {
                builder.AppendLine(descriptionPrefix + description);
                return;
            }

            var availableWidth = Math.Max(1, maxWidth - descriptionPrefix.Length);
            var remaining = description;
            var firstLine = true;
            while (remaining.Length > availableWidth) {
                var splitIndex = remaining.LastIndexOf(' ', availableWidth);
                if (splitIndex <= 0) { splitIndex = availableWidth; }

                builder.Append(firstLine ? descriptionPrefix : new string(' ', descriptionPrefix.Length));
                builder.AppendLine(remaining.Substring(0, splitIndex));
                remaining = remaining.Substring(splitIndex).TrimStart();
                firstLine = false;
            }

            builder.Append(firstLine ? descriptionPrefix : new string(' ', descriptionPrefix.Length));
            builder.AppendLine(remaining);
        }

        /// <summary>
        /// Gets the name of the application.
        /// </summary>
        public static string GetApplicationName() {
            return System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "Unknown";
        }

    }
}
