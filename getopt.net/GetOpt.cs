using System;

namespace getopt.net {

    using System.Text.RegularExpressions;

    /// <summary>
    /// GetOpt-like class for handling getopt-like command-line arguments in .net.
    /// </summary>
    public partial class GetOpt {

        public const char MissingArgChar = '?';
        public const char InvalidOptChar = '!';
        public const char NonOptChar     = (char)1;
        public const string DoubleDash   = "--";

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
        /// </summary>
        public bool DoubleDashStopsParsing { get; set; } = false;

        /// <summary>
        /// Gets or sets the arguments to parse.
        /// </summary>
        public string[] AppArgs { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets a value indicating whether or not to only parse short options.
        /// </summary>
        public bool OnlyShortOpts { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether or not to ignore empty values.
        /// </summary>
        public bool IgnoreEmptyOptions { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not to ignore missing arguments.
        /// </summary>
        /// <remarks >
        /// If this is set to <code >true</code> and a required argument is missing, '?' will be returned.
        /// </remarks>
        public bool IgnoreMissingArgument { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether or not invalid arguments should be ignored or not.
        /// </summary>
        /// <remarks >
        /// If this is set to <code >true</code> and an invalid argument is found, then '!' will be returned.
        /// </remarks>
        public bool IgnoreInvalidOptions { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not empty <see cref="AppArgs"/> are ignored or throw an exception.
        /// </summary>
        /// <remarks >
        /// Default: <code >true</code>
        /// </remarks>
        public bool IgnoreEmptyAppArgs { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not option parsing shall stop or not.
        /// </summary>
        /// <remarks >
        /// When this is set to <code >true</code>, all remaining arguments in AppArgs will be returned without being parsed.
        /// </remarks>
        public bool StopParsingOptions { get; set; } = false;

        /// <summary>
        /// Gets the current index of the app arguments being parsed.
        /// </summary>
        public int CurrentIndex => m_currentIndex;

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

        protected int m_currentIndex = 0;
        protected int m_optPosition = 1; // this applies to short opts only, where [0] == '-'

        /// <summary>
        /// Compiled Regex.
        /// </summary>
        /// <returns>A pre-compiled and optimised regular expression object.</returns>
#if NET7_0_OR_GREATER
        [GeneratedRegex(@"([\s]|[=])", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        protected static partial Regex ArgumentSplitter();
#else
        protected static Regex ArgumentSplitter() => new(@"([\s]|[=])", RegexOptions.IgnoreCase | RegexOptions.Compiled);
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
                if (!IgnoreEmptyOptions) { throw new ParseException("Encountered null or empty argument!"); }
                else { return 0; }
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

            if (IsLongOption(ref AppArgs[m_currentIndex])) {
                if (Options.Length == 0) { throw new ParseException("Cannot parse long option! No option list provided!"); }
                return ParseLongOption(out outOptArg);
            } else if (IsShortOption(ref AppArgs[CurrentIndex])) {
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
            if (nullableOpt == null) {
                if (!IgnoreInvalidOptions) {
                    ++m_currentIndex;
                    throw new ParseException(AppArgs[m_currentIndex], "Invalid option found!");
                }
                ++m_currentIndex;
                return InvalidOptChar;
            }

            
            var opt = (Option)nullableOpt;
            switch (opt.ArgumentType) {
                case ArgumentType.Required:
                    if (optArg == null && (IsLongOption(ref AppArgs[CurrentIndex + 1]) || IsShortOption(ref AppArgs[CurrentIndex + 1]))) {
                        if (IgnoreMissingArgument) {
                            m_currentIndex += 1;
                            return MissingArgChar;
                        }
                        else {
                            ++m_currentIndex;
                            throw new ParseException(AppArgs[CurrentIndex], "Missing required argument!");
                        }
                    } else if (optArg != null) {
                        break;
                    }

                    optArg = AppArgs[CurrentIndex + 1];
                    if (MustStopParsing()) { // POSIX behaviour desired
                        m_currentIndex = AppArgs.Length;
                        break;
                    }

                    m_currentIndex += 1;
                    break;
                case ArgumentType.None:
                default: // this case will handle cases where developers carelessly cast integers to the enum type
                    optArg = null;
                    break;
                case ArgumentType.Optional:
                    // DRY this off at some point
                    if (optArg == null && !IsLongOption(ref AppArgs[CurrentIndex + 1]) && !IsShortOption(ref AppArgs[CurrentIndex + 1])) {
                        optArg = AppArgs[CurrentIndex + 1];
                        ++m_currentIndex;
                    }
                    break;
            }

            
            ++m_currentIndex;
            return opt.Value;
        }

        /// <summary>
        /// Parses a single short option.
        /// </summary>
        /// <param name="optArg">Out var; the argument required for the option.</param>
        /// <returns>The current option value.</returns>
        /// <exception cref="ParseException">If ignoring of errors is disabled (default) will throw a ParseException if an error occurs.</exception>
        protected int ParseShortOption(out string? optArg) {
            var curOpt = AppArgs[CurrentIndex][m_optPosition];
            if (ShortOptRequiresArg(ref curOpt)) {
                if (m_optPosition + 1 >= AppArgs[CurrentIndex].Length) {
                    if (!IsLongOption(ref AppArgs[CurrentIndex + 1]) && !IsShortOption(ref AppArgs[CurrentIndex + 1])) {
                        optArg = AppArgs[CurrentIndex + 1];
                        ResetOptPosition();

                        if (!MustStopParsing()) {
                            m_currentIndex = AppArgs.Length; // POSIX behaviour desired
                        } else {
                            m_currentIndex += 2;
                        }

                        return curOpt;
                    }

                    if (IgnoreMissingArgument) {
                        optArg = null;
                        return MissingArgChar;
                    } else { throw new ParseException(curOpt.ToString(), "Missing argument for option!"); }
                } else {
                    optArg = AppArgs[CurrentIndex].Substring(m_optPosition + 1);
                    m_currentIndex++;
                    ResetOptPosition();
                    return curOpt;
                }
            }

            optArg = null;
            if (AppArgs[CurrentIndex].Length > AppArgs[CurrentIndex].IndexOf(curOpt) + 1) {
                m_optPosition++;
            } else {
                m_currentIndex++;
                ResetOptPosition();
            }
            return curOpt;
        }

        /// <summary>
        /// Gets a value indicating whether or not a short option requires an argument.
        /// </summary>
        /// <param name="shortOpt">The opt to check for.</param>
        /// <returns><code >true</code> if the short opt requires an argument.</returns>
        /// <exception cref="ParseException">If ignoring errors is disabled (default) and an error occurs during parsing.</exception>
        protected bool ShortOptRequiresArg(ref char shortOpt) {
            if (!string.IsNullOrEmpty(ShortOpts)) {
                var posInStr = ShortOpts.IndexOf(shortOpt);
                if (posInStr == -1) { goto CheckLongOpt; }
                else if (posInStr + 1 >= ShortOpts.Length) { return false; }

                return ShortOpts[posInStr + 1] == ':';
            }

            CheckLongOpt:
            if (Options.Length == 0) {
                if (IgnoreInvalidOptions) {
                    shortOpt = InvalidOptChar;
                    return false;
                } else {
                    throw new ParseException(shortOpt.ToString(), "Invalid option list!");
                }
            }
            var nullableOpt = Options.FindOptionOrDefault(shortOpt);

            if (nullableOpt == null) {
                if (IgnoreInvalidOptions) {
                    shortOpt = InvalidOptChar;
                    return false;
                } else { throw new ParseException(shortOpt.ToString(), "Encountered unknown option!"); }
            }

            var opt = (Option)nullableOpt;
            return opt.ArgumentType == ArgumentType.Required;
        }

        /// <summary>
        /// Determines whether or not the current option contains its argument with the string or not.
        /// </summary>
        /// <param name="optName">Out var; the name of the option</param>
        /// <param name="argVal">Out var; the value of the argument</param>
        /// <returns><code >true</code> if the option contains its argument. <code >false</code> otherwise.</returns>
        protected bool HasArgumentInOption(out string optName, out string? argVal) {
            var curArg = AppArgs[m_currentIndex];
            var splitString = Array.Empty<string>();

            splitString = ArgumentSplitter().Split(AppArgs[m_currentIndex]);

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
        /// <param name="isLongOpt">Whether or not the current option is a long option.</param>
        /// <returns>The stripped string</returns>
        protected string StripDashes(bool isLongOpt) {
            var curArg = AppArgs[m_currentIndex];

            if (!curArg.StartsWith("--") && !curArg.StartsWith("-")) {
                return curArg;
            }

            if (isLongOpt && curArg.StartsWith("--")) {
                return curArg.Substring(2);
            } else if (curArg.StartsWith("-")) {
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
        protected bool IsLongOption(ref string arg) {
            return !string.IsNullOrEmpty(arg) && // string must not be null or empty
                   arg[0] == '-' && // first and 
                   arg[1] == '-' && // second char must be '-'
                   arg.Length > 2;                 // if string is "--" then parsing should stop
        }

        /// <summary>
        /// Gets a value indicating whether or not the current string is/contains a (or more) short option
        /// </summary>
        /// <param name="arg">The string to check.</param>
        /// <returns><code >true</code> if the string contains one or more short options</returns>
        protected bool IsShortOption(ref string arg) {
            return !string.IsNullOrEmpty(arg) &&
                   arg[0] == '-'  &&
                   arg.Length > 1 &&
                   arg[1] != '-';
        }
    }
}

