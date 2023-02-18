using System;

namespace getopt.net {

	public class ParseException: Exception {
		public ParseException(string msg): this(null, msg) { }

		public ParseException(string? optString, string msg): base(msg) {
			Option = optString;
		}

		/// <summary>
		/// The option that caused the exception.
		/// </summary>
		public string? Option { get; set; }
	}
}

