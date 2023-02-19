using System;

namespace getopt.net {

	/// <summary>
	/// Represents a single long option for getopt.
	/// </summary>
	public struct Option {

		/// <summary>
		/// The name of the (long) option
		/// </summary>
		/// <remarks >
		/// Do not prefix this with dashes.
		/// </remarks>
		/// <example >
		/// help
		/// </example>
		public string? Name { get; set; }

		/// <summary>
		/// The type of argument this option requires.
		/// </summary>
		public ArgumentType? ArgumentType { get; set; }

		/// <summary>
		/// An (optional) pointer to a flag variable to set.
		/// </summary>
		/// <remarks >
		/// If set, Flag must point to a char type!
		/// </remarks>
		public IntPtr Flag { get; set; }

		/// <summary>
		/// The value (short opt) for the option.
		/// </summary>
		public char Value { get; set; }

	}
}

