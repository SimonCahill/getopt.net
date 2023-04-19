using System;

namespace getopt.net {

    using System.Text.RegularExpressions;

    /// <summary>
    /// GetOpt-like class for handling getopt-like command-line arguments in .net.
    /// 
    /// This class contains all the properties and logic required for parsing getopt-like command-line arguments.
    /// 
    /// A detailled description of the usage of this class can be found in the <see href="https://github.com/SimonCahill/getopt.net/wiki" />.
    /// </summary>
    public partial class GetOpt {

        /// <summary>
        /// The character that is returned when an option is missing a required argument.
        /// </summary>
        public const char MissingArgChar    = '?';

        /// <summary>
        /// The character that is returned when an invalid option is returned.
        /// </summary>
        public const char InvalidOptChar    = '!';

        /// <summary>
        /// The character that is returned when a non-option value is encountered and it is not the argument to an option.
        /// </summary>
        public const char NonOptChar        = (char)1;

        /// <summary>
        /// This is the string getopt.net looks for when <see cref="DoubleDashStopsParsing" /> is enabled.
        /// </summary>
        public const string DoubleDash      = "--";

        /// <summary>
        /// A single dash character.
        /// This is the character that is searched for, when parsing POSIX-/GNU-like options.
        /// </summary>
        public const char SingleDash        = '-';

        /// <summary>
        /// A single slash.
        /// This is the char that is searched for when parsing arguments with the Windows convention.
        /// </summary>
        public const char SingleSlash       = '/';

        /// <summary>
        /// The argument separator used by Windows.
        /// </summary>
        public const char WinArgSeparator   = ':';

        /// <summary>
        /// The argument separator used by POSIX / GNU getopt.
        /// </summary>
        public const char GnuArgSeparator = '=';

        /// <summary>
        /// The regex used by <see cref="ArgumentSplitter"/> to split arguments into a key-value pair.
        /// </summary>
        public const string ArgSplitRegex = @"([\s]|[=])";

        /// <summary>
        /// An optional list of long options to go with the short options.
        /// </summary>
        public Option[] Options { get; set; } = Array.Empty<Option>();

        /// <summary>
        /// The short opts to use.
        /// </summary>
        public string? ShortOpts { get; set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether or not "--" stops parsing.
        /// Default: <code >true</code>
        /// </summary>
        public bool DoubleDashStopsParsing { get; set; } = true;

        /// <summary>
        /// Gets or sets the arguments to parse.
        /// </summary>
        public string[] AppArgs { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets a value indicating whether or not to only parse short options.
        /// Default: <code >false</code>
        /// </summary>
        public bool OnlyShortOpts { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether or not to ignore empty values.
        /// Default: <code  >true</code>
        /// </summary>
        public bool IgnoreEmptyOptions { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not to ignore missing arguments.
        /// Default: <code >false</code>
        /// </summary>
        /// <remarks >
        /// If this is set to <code >true</code> and a required argument is missing, '?' will be returned.
        /// </remarks>
        public bool IgnoreMissingArgument { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether or not invalid arguments should be ignored or not.
        /// Default: <code >true</code>
        /// </summary>
        /// <remarks >
        /// If this is set to <code >true</code> and an invalid argument is found, then '!' will be returned.
        /// </remarks>
        public bool IgnoreInvalidOptions { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not empty <see cref="AppArgs"/> are ignored or throw an exception.
        /// Default: <code >true</code>
        /// </summary>
        public bool IgnoreEmptyAppArgs { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not option parsing shall stop or not.
        /// Default: <code >false</code>
        /// </summary>
        /// <remarks >
        /// When this is set to <code >true</code>, all remaining arguments in AppArgs will be returned without being parsed.
        /// </remarks>
        public bool StopParsingOptions { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether or not Windows argument conventions are allowed.
        /// Default: <code >false</code>
        /// </summary>
        /// <remarks >
        /// By convention, Windows-like options begin with a slash (/).
        /// Options with arguments are separated by a colon ':'.
        /// </remarks>
        public bool AllowWindowsConventions { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether or not Powershell-style arguments are allowed.
        /// This option doesn't conflict with the GNU/POSIX or Windows-style argument parsing and is simply an addition.
        /// </summary>
        /// <remarks >
        /// This option is disabled by default.
        /// 
        /// Powershell-style arguments are similar to GNU/POSIX long options, however they begin with a single dash (-).
        /// </remarks>
        public bool AllowPowershellConventions { get; set; } = false;


        /// <summary>
        /// Either enables or disabled exceptions entirely.
        /// For more specific control over exceptions, see the other options provided by GetOpt.
        /// </summary>
        /// <value><code >true</code> if exceptions are enabled, <code >false</code> otherwise.</value>
        public bool AllExceptionsDisabled {
            get => IgnoreEmptyAppArgs && IgnoreEmptyOptions && IgnoreMissingArgument && IgnoreInvalidOptions && IgnoreEmptyAppArgs;
            set {
                IgnoreEmptyAppArgs = IgnoreEmptyOptions = IgnoreMissingArgument = IgnoreInvalidOptions = IgnoreEmptyAppArgs = value;
            }
        }

        /// <summary>
        /// Gets the current index of the app arguments being parsed.
        /// </summary>
        public int CurrentIndex => m_currentIndex;

        /// <summary>
        /// Default constructor; it is recommended to use this constructor
        /// and to use brace-initialiser-lists to instantiate/configure each instance of this object.
        /// </summary>
        public GetOpt() { }

        /// <summary>
        /// Specialised constructor.
        /// </summary>
        /// <param name="appArgs">The arguments passed to the application.</param>
        /// <param name="shortOpts"></param>
        /// <param name="options"></param>
        public GetOpt(string[] appArgs, string shortOpts, params Option[] options) {
            AppArgs = appArgs;
            ShortOpts = shortOpts;
            Options = options;
        }

        /// <summary>
        /// The current index while traversing <see cref="AppArgs" />
        /// </summary>
        protected int m_currentIndex = 0;

        /// <summary>
        /// The current position in a multi-option string such as "-xvzRf" when parsing short options.
        /// </summary>
        protected int m_optPosition = 1; // this applies to short opts only, where [0] == '-'

        /// <summary>
        /// Compiled Regex.
        /// </summary>
        /// <returns>A pre-compiled and optimised regular expression object.</returns>
#if NET7_0_OR_GREATER
        [GeneratedRegex(ArgSplitRegex, RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        protected static partial Regex ArgumentSplitter();
#else
        protected static Regex ArgumentSplitter() => new(ArgSplitRegex, RegexOptions.IgnoreCase | RegexOptions.Compiled);
#endif

        /// <summary>
        /// Resets the option position to 1.
        /// </summary>
        protected void ResetOptPosition() => m_optPosition = 1;

        /// <summary>
        /// Gets a value indicating whether or not non-options should be handled as if they were the argument of an option with the character code 1.
        /// </summary>
        /// <remarks >
        /// From the getopt man page:
        /// 
        /// > If the first character of optstring is '-', then each nonoption
        /// > argv-element is handled as if it were the argument of an option
        /// > with character code 1.  (This is used by programs that were
        /// > written to expect options and other argv-elements in any order
        /// > and that care about the ordering of the two.)
        /// </remarks>
        /// <returns></returns>
        protected bool MustReturnChar1() => ShortOpts?.Length > 0 && ShortOpts[0] == '-';

        /// <summary>
        /// If the first character of
        /// ShortOpts is '+' or the environment variable POSIXLY_CORRECT is
        /// set, then option processing stops as soon as a nonoption argument
        /// is encountered.
        /// </summary>
        /// <returns><code >true</code> if parsing stops when the first non-option string is found.</returns>
        protected bool MustStopParsing() => ShortOpts?.Length > 0 && ShortOpts[0] == '+' || Environment.GetEnvironmentVariable("POSIXLY_CORRECT") is not null;

        /// <summary>
        /// Gets the next option in the list.
        /// </summary>
        /// <param name="outOptArg">Out var; the argument for the option (if applicable).</param>
        /// <returns>The next option.</returns>
        /// <exception cref="ParseException">If ignoring errors is disabled (default) and an error occurs.</exception>
        public int GetNextOpt(out string? outOptArg) {
            if (AppArgs.Length == 0) {
                if (!IgnoreEmptyAppArgs) {
                    throw new ParseException("No arguments found for parsing!");
                } else {
                    outOptArg = null;
                    return -1;
                }
            }

            outOptArg = null; // pre-set this here so we don't have to set it during every condition

            if (CurrentIndex >= AppArgs.Length) { return -1; }

            if (string.IsNullOrEmpty(AppArgs[m_currentIndex])) {
                if (!IgnoreEmptyOptions) {
                    throw new ParseException("Encountered null or empty argument!");
                } else { return 0; }
            } else if (DoubleDashStopsParsing && AppArgs[CurrentIndex].Equals(DoubleDash, StringComparison.InvariantCultureIgnoreCase)) {
                m_currentIndex++;
                StopParsingOptions = true;
                return GetNextOpt(out outOptArg);
            }

            // Check here if StopParsingOptions is true;
            // if so, then simply return NonOptChar and set outOptArg to the value of the argument
            if (StopParsingOptions) {
                outOptArg = AppArgs[CurrentIndex];
                m_currentIndex++;
                return NonOptChar;
            }

            if (IsLongOption(AppArgs[m_currentIndex])) {
                if (Options.Length == 0) { throw new ParseException("Cannot parse long option! No option list provided!"); }
                return ParseLongOption(out outOptArg);
            } else if (IsShortOption(AppArgs[CurrentIndex])) {
                // check if both arg lists are empty
                if (string.IsNullOrEmpty(ShortOpts) && Options.Length == 0) { throw new ParseException("Cannot parse short option! No option list provided!"); }
                return ParseShortOption(out outOptArg);
            }

            if (IgnoreInvalidOptions) {
                outOptArg = AppArgs[CurrentIndex];
                m_currentIndex++;
                if (MustReturnChar1()) { return NonOptChar; }
                else { return InvalidOptChar; }
            } else {
                throw new ParseException(AppArgs[CurrentIndex], "Unexpected option argument!");
            }
        }

        /// <summary>
        /// Parses long options
        /// </summary>
        /// <param name="optArg">Out var; the option's argument (if applicable)</param>
        /// <returns>
        /// If the option was found and no (ignored) errors were detected, the <see cref="Option.Value"/> of the argument.
        /// If an invalid option was found, '!' is returned.
        /// If a required argument is missing, '?' is returned.
        /// </returns>
        /// <exception cref="ParseException">If ignoring of errors is disabled and a parsing error occurs.</exception>
        protected int ParseLongOption(out string? optArg) {
            if (HasArgumentInOption(out var optName, out optArg)) {
                AppArgs[m_currentIndex] = optName;
            }

            AppArgs[m_currentIndex] = StripDashes(true);

            var nullableOpt = Options.FindOptionOrDefault(AppArgs[m_currentIndex]);
            if (nullableOpt is null) {
                ++m_currentIndex;
                if (!IgnoreInvalidOptions) {
                    throw new ParseException(AppArgs[m_currentIndex], "Invalid option found!");
                }

                return InvalidOptChar;
            }

            var opt = (Option)nullableOpt;
            switch (opt.ArgumentType) {
                case ArgumentType.Required:
                    if (optArg == null && (IsLongOption(AppArgs[CurrentIndex + 1]) || IsShortOption(AppArgs[CurrentIndex + 1]))) {
                        ++m_currentIndex;
                        if (IgnoreMissingArgument) {
                            return MissingArgChar;
                        } else {
                            throw new ParseException(AppArgs[CurrentIndex], "Missing required argument!");
                        }
                    } else if (optArg != null) { break; }

                    optArg = AppArgs[CurrentIndex + 1];
                    if (MustStopParsing()) { // POSIX behaviour desired
                        m_currentIndex = AppArgs.Length;
                        break;
                    }

                    ++m_currentIndex;
                    break;
                case ArgumentType.None:
                default: // this case will handle cases where developers carelessly cast integers to the enum type
                    optArg = null;
                    break;
                case ArgumentType.Optional:
                    // DRY this off at some point
                    if (optArg == null && !IsLongOption(AppArgs[CurrentIndex + 1]) && !IsShortOption(AppArgs[CurrentIndex + 1])) {
                        optArg = AppArgs[CurrentIndex + 1];
                        ++m_currentIndex;
                    }
                    break;
            }

            ++m_currentIndex;
            return opt.Value;
        }

        /// <summary>
        /// Attempts to retrieve the argument for the current short option.
        /// 
        /// This is only a helper method to keep the code cleaner.
        /// </summary>
        /// <param name="arg">A reference to the current optArg parameter in <see cref="ParseShortOption(out string?)"/></param>
        /// <param name="incrementCurrentIndex" >Whether or not to increment <see cref="m_currentIndex"/></param>
        /// <returns><code>true</code> if an argument was found for the current option. <code>false</code> otherwise.</returns>
        protected bool TryGetArgumentForShortOption(ref string? arg, out bool incrementCurrentIndex) {
            incrementCurrentIndex = false; // pre-set this

            if (m_optPosition + 1 < AppArgs[CurrentIndex].Length) {
                arg = AppArgs[CurrentIndex].Substring(m_optPosition + 1);
                incrementCurrentIndex = true;
                return true;
            }

            if (CurrentIndex + 1 >= AppArgs.Length) {
                return false;
            }

            if (!IsLongOption(AppArgs[CurrentIndex + 1]) && !IsShortOption(AppArgs[CurrentIndex + 1])) {
                arg = AppArgs[CurrentIndex + 1];

                if (MustStopParsing()) {
                    m_currentIndex = AppArgs.Length; // POSIX behaviour desired
                } else {
                    m_currentIndex += 2;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Parses a single short option.
        /// </summary>
        /// <param name="optArg">Out var; the argument required for the option.</param>
        /// <returns>The current option value.</returns>
        /// <exception cref="ParseException">If ignoring of errors is disabled (default) will throw a ParseException if an error occurs.</exception>
        protected int ParseShortOption(out string? optArg) {
            optArg = null;
            var curOpt = AppArgs[CurrentIndex][m_optPosition];

            bool incrementCurrentIndex = false;
            var argType = ShortOptRequiresArg(curOpt);
            if (argType is null) {
                ResetOptPosition();
                m_currentIndex++;
                return InvalidOptChar;
            } else if (argType is ArgumentType type) {
                switch (type) {
                    default:
                    case ArgumentType.None:
                        if (AppArgs[CurrentIndex].Length > AppArgs[CurrentIndex].IndexOf(curOpt) + 1) {
                            m_optPosition++;
                            return curOpt;
                        }
                        break;
                    case ArgumentType.Optional:
                        if (TryGetArgumentForShortOption(ref optArg, out incrementCurrentIndex)) {
                            ResetOptPosition();
                            if (incrementCurrentIndex) { m_currentIndex++; }
                            return curOpt;
                        }
                        break;
                    case ArgumentType.Required:
                        if (!TryGetArgumentForShortOption(ref optArg, out incrementCurrentIndex)) {
                            if (incrementCurrentIndex) { m_currentIndex++; }
                            if (IgnoreMissingArgument) { return MissingArgChar; }
                            else { throw new ParseException(curOpt.ToString(), "Missing argument for option!"); }
                        }
                        break;
                }
            }

            ResetOptPosition();
            m_currentIndex++;

            return curOpt;
        }

        /// <summary>
        /// Gets a value indicating whether or not a short option requires an argument.
        /// </summary>
        /// <param name="shortOpt">The opt to check for.</param>
        /// <returns><code >true</code> if the short opt requires an argument.</returns>
        /// <exception cref="ParseException">If ignoring errors is disabled (default) and an error occurs during parsing.</exception>
        protected ArgumentType? ShortOptRequiresArg(char shortOpt) {
            if (!string.IsNullOrEmpty(ShortOpts) && ShortOpts is not null) {
                var posInStr = ShortOpts.IndexOf(shortOpt);
                if (posInStr == -1) {
                    goto CheckLongOpt;
                }

                try {

                    char charToCheck;
                    if (posInStr < ShortOpts.Length - 1) {
                        charToCheck = ShortOpts[posInStr + 1];
                    } else {
                        charToCheck = ShortOpts[posInStr];
                    }

                    switch (charToCheck) {
                        case ':':
                            return ArgumentType.Required;
                        case ';':
                            return ArgumentType.Optional;
                        default:
                            return ArgumentType.None;
                    }
                } catch {
                    goto CheckLongOpt;
                }
            }

        CheckLongOpt:
            if (Options.Length == 0) {
                if (IgnoreInvalidOptions) {
                    return null;
                } else {
                    throw new ParseException(shortOpt.ToString(), "Invalid option list!");
                }
            }
            var nullableOpt = Options.FindOptionOrDefault(shortOpt);

            if (nullableOpt == null) {
                if (IgnoreInvalidOptions) {
                    shortOpt = InvalidOptChar;
                    return ArgumentType.None;
                } else { throw new ParseException(shortOpt.ToString(), "Encountered unknown option!"); }
            }

            var opt = (Option)nullableOpt;
            return opt.ArgumentType ?? ArgumentType.None;
        }

        /// <summary>
        /// Determines whether or not the current option contains its argument with the string or not.
        /// </summary>
        /// <param name="optName">Out var; the name of the option</param>
        /// <param name="argVal">Out var; the value of the argument</param>
        /// <returns><code >true</code> if the option contains its argument. <code >false</code> otherwise.</returns>
        protected bool HasArgumentInOption(out string optName, out string? argVal) {
            var curArg = AppArgs[CurrentIndex];
            var splitString = Array.Empty<string>();

            if (AllowWindowsConventions) {
                // if we're allowing Windows conventions, we have to replace
                // the first occurrence of ':' in the arg string with '='
                var indexOfSeparator = curArg.IndexOf(WinArgSeparator);
                if (indexOfSeparator != -1) {
                    curArg = $"{ curArg.Substring(0, indexOfSeparator) }{ GnuArgSeparator }{ curArg.Substring(indexOfSeparator + 1) }";
                }
            }

            splitString = ArgumentSplitter().Split(curArg);

            if (splitString.Length == 1) {
                optName = StripDashes(true); // we can set this to true, because this method will only ever be called for long opts
                argVal = null;
                return false;
            }

            optName = splitString[0];
#if NET6_0_OR_GREATER
            argVal = string.Join("", splitString[2..]);
#else
            argVal = string.Join("", splitString.Skip(2));
#endif
            return true;
        }

        /// <summary>
        /// Strips leading dashes from strings.
        /// </summary>
        /// <remarks >
        /// If <see cref="AllowWindowsConventions" /> is enabled, then this method will also strip leading slashes!
        /// </remarks>
        /// <param name="isLongOpt">Whether or not the current option is a long option.</param>
        /// <returns>The stripped string</returns>
        protected string StripDashes(bool isLongOpt) {
            var curArg = AppArgs[m_currentIndex];

            if (AllowWindowsConventions &&
                curArg.StartsWith(SingleSlash.ToString())) {
                return curArg.Substring(1);
            }

            if (!curArg.StartsWith(DoubleDash) &&
                !curArg.StartsWith(SingleDash.ToString())) {
                return curArg;
            }

            if (isLongOpt && curArg.StartsWith(DoubleDash)) {
                return curArg.Substring(2);
            } else if (curArg.StartsWith(SingleDash.ToString())) {
                return curArg.Substring(1);
            }

            return curArg;
        }

        /// <summary>
        /// Gets a value indicating whether or not the parser shall stop here.
        /// </summary>
        /// <param name="arg">The arg to check</param>
        /// <returns><code>true</code> if the parser shall stop parsing. <code >false</code> otherwise.</returns>
        protected bool ShallStopParsing(ref string arg) {
            return !string.IsNullOrEmpty(arg) &&
                   arg.Equals("--", StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Gets a value indicating whether or not the current string is a long option.
        /// </summary>
        /// <param name="arg">The string to check.</param>
        /// <returns><code >true</code> if the passed string is a long option.</returns>
        protected bool IsLongOption(string arg) {
            if (string.IsNullOrEmpty(arg)) { return false; }

            if (
                AllowWindowsConventions &&
                arg.Length > 1          &&
                arg[0] == SingleSlash   &&
                Options.Length != 0     &&
                Options.Any(o => o.Name == arg.Split(WinArgSeparator, GnuArgSeparator, ' ').First().Substring(1)) // We only need this option when parsing options following Windows' conventions
            ) { return true; }

            // Check for Powershell-style arguments.
            // Powershell arguments are weird and extra checks are needed.
            // Powershell-style arguments would theoretically interfere with short opts,
            // so a check to determine whether or not the option is found in Options is required.
            if (
                AllowPowershellConventions  &&
                arg.Length > 1              &&
                arg[0] == SingleDash        &&
                Options.Length != 0         &&
                Options.Any(o => o.Name == arg.Split(WinArgSeparator, GnuArgSeparator, ' ').First().Substring(1)) // We only need this when parsing options following Powershell's conventions
                // This parsing method is really similar to Windows option parsing...
            ) { return true; }

            return arg.Length > 2       &&
                   arg[0] == SingleDash &&
                   arg[1] == SingleDash;
        }

        /// <summary>
        /// Gets a value indicating whether or not the current string is/contains a (or more) short option
        /// </summary>
        /// <param name="arg">The string to check.</param>
        /// <returns><code >true</code> if the string contains one or more short options</returns>
        protected bool IsShortOption(string arg) {
            if (string.IsNullOrEmpty(arg)) { return false; }

            if (
                AllowWindowsConventions &&
                arg.Length > 1          &&
                arg[0] == SingleSlash   &&
                arg[1] != SingleSlash
            ) { return true; }

            return arg.Length > 1       &&
                   arg[0] == SingleDash &&
                   arg[1] != SingleDash;
        }
    }
}

