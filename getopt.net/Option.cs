using System;

namespace getopt.net {

    /// <summary>
    /// Represents a single long option for getopt.
    /// </summary>
    public struct Option {

        /// <summary>
        /// Constructs a new, empty instance of this struct.
        /// </summary>
        public Option() { }

        /// <summary>
        /// Constructs a new instance of this struct with a <see cref="char"/> as the value type.
        /// </summary>
        /// <param name="name">The name of the argument.</param>
        /// <param name="argType">The argument type.</param>
        /// <param name="value">The value of the option.</param>
        /// <param name="description">The description of the option.</param>
        public Option(string name, ArgumentType argType, char value, string description = ""): this(name, argType, (int)value, description) {}

        /// <summary>
        /// Constructs a new instance of this struct with an <see cref="int"/> as the value type.
        /// </summary>
        /// <param name="name">The name of the argument.</param>
        /// <param name="argType">The argument type.</param>
        /// <param name="value">The value of the option.</param>
        /// <param name="description">The description of the option.</param>
        public Option(string name, ArgumentType argType, int value, string description = "") {
            Name = name;
            ArgumentType = argType;
            Value = value;
            Description = description;
        }

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
        /// The value (short opt) for the option.
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// The description of the option.
        /// </summary>
        public string? Description { get; set; }

    }
}

