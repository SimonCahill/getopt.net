namespace getopt.net {

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
        /// Requests the engine's POSIX stop condition, also enabled by the POSIXLY_CORRECT environment variable.
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

            var index = Array.FindIndex(list, o => o.Name?.Equals(optName, comparison) == true);

            return index >= 0 ? list[index] : null;
        }

        /// <summary>
        /// Finds an option in the list <paramref name="list" /> with the <see cref="Option.Value" /> <paramref name="optVal" />.
        /// </summary>
        /// <param name="list">The list of options to search.</param>
        /// <param name="optVal">The value to search for.</param>
        /// <returns>The <see cref="Option" /> with the <see cref="Option.Value" /> <paramref name="optVal" />, or <code >null</code> if no option was found matching the name.</returns>
        public static Option? FindOptionOrDefault(this Option[]? list, char optVal) {
            if (list is null) { return null; }

            var index = Array.FindIndex(list, o => o.Value == optVal);
            return index >= 0 ? list[index] : null;
        }

        /// <summary>
        /// Creates a short opt string from an array of <see cref="Option"/> objects.
        /// </summary>
        /// <param name="list">The options to convert.</param>
        /// <returns>The aliases and argument-mode markers in declaration order, or an empty string for a null or empty option list.</returns>
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
        /// <returns>The mode prefix followed by aliases and argument markers, or an empty string for a null or empty option list.</returns>
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
            return GenerateHelpText(getopt, generatorOptions, null);
        }

        /// <summary>
        /// Renders shared help while optionally using typed metadata to omit undeclared short aliases.
        /// </summary>
        /// <param name="getopt">The parser definitions and enabled conventions to describe.</param>
        /// <param name="generatorOptions">Formatting and application metadata, or null for defaults.</param>
        /// <param name="shortAlias">An alias resolver for typed definitions, or null to preserve legacy formatting.</param>
        /// <returns>Formatted help, or an empty string when no options or commands exist.</returns>
        /// <exception cref="ArgumentNullException">The parser is null.</exception>
        internal static string GenerateHelpText(GetOpt getopt, HelpTextConfig? generatorOptions, Func<Option, char?>? shortAlias) {
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
                if (shortAlias is not null) {
                    AppendHelpLine(sBuilder, Tab, TypedOptionLabel(opt, shortAlias, shortOptPrefix, longOptPrefix, longestName), opt.Description ?? string.Empty, config.MaxWidth);
                    continue;
                }

                sBuilder.AppendLine($"{Tab}{shortOptPrefix}{(char)opt.Value}, {longOptPrefix}{opt.Name?.PadRight(longestName)}{opt.Description ?? string.Empty}");
            }
            sBuilder.AppendLine();

            sBuilder.AppendLine("Options:");
            foreach (var opt in options.Where(o => o.ArgumentType != ArgumentType.None)) {
                if (shortAlias is not null) {
                    AppendHelpLine(sBuilder, Tab, TypedOptionLabel(opt, shortAlias, shortOptPrefix, longOptPrefix, longestName), opt.Description ?? string.Empty, config.MaxWidth);
                    continue;
                }

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
                        var optionLabel = shortAlias is null
                            ? $"{shortOptPrefix}{(char)option.Value}, {longOptPrefix}{option.Name?.PadRight(longestCommandOptionName)}"
                            : TypedOptionLabel(option, shortAlias, shortOptPrefix, longOptPrefix, longestCommandOptionName);
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

        /// <summary>
        /// Builds an option label using only its declared aliases.
        /// </summary>
        /// <param name="option">The option whose long name is displayed.</param>
        /// <param name="shortAlias">Resolves its declared short alias, or null for a long-only option.</param>
        /// <param name="shortPrefix">The short-option prefix for the selected help convention.</param>
        /// <param name="longPrefix">The long-option prefix for the selected help convention.</param>
        /// <param name="width">The padding width for the long name.</param>
        /// <returns>A padded option label with an optional short alias and separator.</returns>
        private static string TypedOptionLabel(Option option, Func<Option, char?> shortAlias, string shortPrefix, string longPrefix, int width) {
            var alias = shortAlias(option);

            return $"{(alias.HasValue ? shortPrefix + alias.Value + ", " : string.Empty)}{longPrefix}{option.Name?.PadRight(width)}";
        }

        /// <summary>
        /// Wraps a help description while aligning continuation lines beneath its first character.
        /// </summary>
        /// <param name="builder">The destination help-text buffer.</param>
        /// <param name="indentation">Indentation for the option or command scope.</param>
        /// <param name="label">The already padded option or command label.</param>
        /// <param name="description">The description to wrap.</param>
        /// <param name="maxWidth">The desired total width; a label wider than this limit is still emitted in full.</param>
        /// <remarks>
        /// Long words are split when no whitespace boundary fits. At least one description character is emitted per wrapped line.
        /// </remarks>
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
        /// <returns>The entry assembly's name, or <c>Unknown</c> when the entry assembly or its name is unavailable.</returns>
        public static string GetApplicationName() {
            return System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "Unknown";
        }

    }
}
