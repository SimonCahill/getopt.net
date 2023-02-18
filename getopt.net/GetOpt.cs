using System;

namespace getopt.net {

	using System.Text.RegularExpressions;

	/// <summary>
	/// GetOpt-like class for handling getopt-like command-line arguments in .net.
	/// </summary>
	public partial class GetOpt {

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
		/// If this is set to <code >false</code> and an invalid argument is found, then '!' will be returned.
		/// </remarks>
		public bool IgnoreInvalidArguments { get; set; } = false;

		/// <summary>
		/// Gets the current index of the app arguments being parsed.
		/// </summary>
		public int CurrentIndex => m_currentIndex;

		///// <summary>
		///// The argument for the current option.
		///// </summary>
		//public string? OptArg { get; private set; } = string.Empty;

		public GetOpt() { }

		public GetOpt(string[] appArgs, string shortOpts, params Option[] options) {
			AppArgs = appArgs;
			ShortOpts = shortOpts;
			Options = options;
		}

		private bool m_skipNextOpt = false;
		private int m_currentIndex = 0;
		private int m_optPosition = 1; // this applies to short opts only, where [0] == '-'

		[GeneratedRegex(@"([\s]|[=])", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
		private static partial Regex ArgumentSplitter();

		public int GetNextOpt(out string? outOptArg) {
			outOptArg = null; // pre-set this here so we don't have to set it during every condition

			if (string.IsNullOrEmpty(AppArgs[m_currentIndex])) {
				if (!IgnoreEmptyOptions) { throw new ParseException("Encountered null or empty argument!"); }
				else { return 0; }
			}

			if (IsLongOption(ref AppArgs[m_currentIndex])) {
				if (Options.Length == 0) { throw new ParseException("Cannot parse long option! No option list provided!"); }
				return ParseLongOption(out outOptArg);
			} else if (IsShortOption(ref AppArgs[CurrentIndex])) {
				// check if both arg lists are empty
				if (string.IsNullOrEmpty(ShortOpts) && Options.Length == 0) { throw new ParseException("Cannot parse short option! No option list provided!"); }
				return ParseShortOption(out outOptArg);
			}

			if (IgnoreInvalidArguments) {
				return '!';
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
		private int ParseLongOption(out string? optArg) {
			if (HasArgumentInOption(out var optName, out optArg)) {
				AppArgs[m_currentIndex] = optName;
			}

			var nullableOpt = Options.FindOptionOrDefault(AppArgs[m_currentIndex]);
			if (nullableOpt == null) {
				if (IgnoreInvalidArguments) {
					++m_currentIndex;
					throw new ParseException(AppArgs[m_currentIndex], "Invalid option found!");
				}
				++m_currentIndex;
				return '!';
			}

			
			var opt = (Option)nullableOpt;
			switch (opt.ArgumentType) {
				case ArgumentType.Reguired:
					if (optArg == null && (IsLongOption(ref AppArgs[CurrentIndex + 1]) || IsShortOption(ref AppArgs[CurrentIndex + 1]))) {
						if (IgnoreMissingArgument) {
							++m_currentIndex;
							return '?';
						}
						else {
							++m_currentIndex;
							throw new ParseException(AppArgs[CurrentIndex], "Missing required argument!");
						}
					}

					optArg = AppArgs[CurrentIndex + 1];
					break;
				case ArgumentType.None:
				default: // this case will handle cases where developers carelessly cast integers to the enum type
					optArg = null;
					break;
				case ArgumentType.Optional:
					// DRY this off at some point
					if (optArg == null && !IsLongOption(ref AppArgs[CurrentIndex + 1]) && !IsShortOption(ref AppArgs[CurrentIndex + 1])) {
						optArg = AppArgs[CurrentIndex + 1];
					}
					break;
			}

			
			++m_currentIndex;
			return opt.Value;
		}

		private int ParseShortOption(out string? optArg) {
			var curOpt = AppArgs[CurrentIndex][m_optPosition];


			throw new NotImplementedException();
		}

		private bool ShortOptRequiresArg(ref char shortOpt) {
			if (!string.IsNullOrEmpty(ShortOpts)) {
				var posInStr = ShortOpts.IndexOf(shortOpt);
				if (posInStr == -1 || posInStr + 1 > ShortOpts.Length) { goto CheckLongOpt; }

				return ShortOpts[posInStr + 1] == ':';
			}

			CheckLongOpt:
			if (Options.Length == 0) { throw new ParseException(shortOpt.ToString(), "Invalid option list!"); }

		}

		/// <summary>
		/// Determines whether or not the current option contains its argument with the string or not.
		/// </summary>
		/// <param name="optName">Out var; the name of the option</param>
		/// <param name="argVal">Out var; the value of the argument</param>
		/// <returns><code >true</code> if the option contains its argument. <code >false</code> otherwise.</returns>
		private bool HasArgumentInOption(out string optName, out string? argVal) {
			var curArg = AppArgs[m_currentIndex];
			var splitString = Array.Empty<string>();

			splitString = ArgumentSplitter().Split(AppArgs[m_currentIndex]);

			if (splitString.Length == 0) {
				optName = StripDashes(true); // we can set this to true, because this method will only ever be called for long opts
				argVal = null;
				return false;
			}

			optName = splitString[0];
			argVal = string.Join("", splitString[1..]);
			return true;
		}

		private string StripDashes(bool isLongOpt) {
			var curArg = AppArgs[m_currentIndex];

			if (isLongOpt) {
				return curArg.Substring(2);
			}
			return curArg.Substring(1);
		}

		/// <summary>
		/// Gets a value indicating whether or not the parser shall stop here.
		/// </summary>
		/// <param name="arg">The arg to check</param>
		/// <returns><code>true</code> if the parser shall stop parsing. <code >false</code> otherwise.</returns>
		private bool ShallStopParsing(ref string arg) {
			return !string.IsNullOrEmpty(arg) &&
				   arg.Equals("--", StringComparison.CurrentCultureIgnoreCase);
		}

		/// <summary>
		/// Gets a value indicating whether or not the current string is a long option.
		/// </summary>
		/// <param name="arg">The string to check.</param>
		/// <returns><code >true</code> if the passed string is a long option.</returns>
		private bool IsLongOption(ref string arg) {
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
		private bool IsShortOption(ref string arg) {
			return !string.IsNullOrEmpty(arg) &&
				   arg[0] == '-' &&
				   arg.Length > 1;
		}
	}
}

