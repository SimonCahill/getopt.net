using System;

namespace getopt.net {

	/// <summary>
	/// Generic exception class that is thrown when the parser is not configured to ignore errors.
	/// 
	/// This exception may be thrown whenever one of the following options is disabled:
	///  - <see cref="GetOpt.IgnoreEmptyAppArgs" />
	///  - <see cref="GetOpt.IgnoreEmptyOptions" />
	///  - <see cref="GetOpt.IgnoreInvalidOptions" />
	///  - <see cref="GetOpt.IgnoreMissingArgument" />
	/// </summary>
    public class ParseException: Exception {

		/// <summary>
		/// Generic exception constructor; only accepts the error message.
		/// </summary>
		/// <param name="msg">A brief description of the error.</param>
        public ParseException(string msg): this(null, msg) { }

		/// <summary>
		/// Specialsied exception constructor; accepts the option causing the error and an accompanying message.
		/// </summary>
		/// <param name="optString">The option that caused the exception.</param>
		/// <param name="msg">A brief description of the error.</param>
        public ParseException(string? optString, string msg): base(msg) {
            Option = optString;
        }

		/// <inheritdoc />
		public override string ToString() {
			if (string.IsNullOrEmpty(Option)) {
				return base.ToString();
			}
			return $"Error occurred while parsing { Option }: { Message }";
		}

		/// <summary>
		/// The option that caused the exception.
		/// </summary>
		public string? Option { get; set; }
    }
}

