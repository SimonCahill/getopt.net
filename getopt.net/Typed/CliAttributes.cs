namespace getopt.net.Typed;

/// <summary>
/// Declares a property bound to a command-line option.
/// </summary>
/// <remarks>
/// Applied to a public readable, writable property. A boolean scalar or collection element is a flag unless an argument mode is supplied.
/// Arrays and lists append repeated values; other properties retain the last supplied value.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class CliOptionAttribute: Attribute {

    /// <summary>
    /// Stores an explicit argument mode; inference is controlled separately by <see cref="HasArgumentType"/>.
    /// </summary>
    private ArgumentType m_argumentType = ArgumentType.Required;

    /// <summary>
    /// Creates an option with an explicit long name, without a prefix.
    /// </summary>
    /// <param name="name">The long option name, validated when the parser is constructed.</param>
    public CliOptionAttribute(string name) {
        Name = name;
    }

    /// <summary>
    /// Gets the long option name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets the optional short alias. The default null character means no alias.
    /// </summary>
    public char ShortName { get; set; }

    /// <summary>
    /// Gets or sets the description used in generated help.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the option must appear, independently of its argument mode.
    /// </summary>
    public bool Required { get; set; }

    /// <summary>
    /// Gets or sets the argument mode. When omitted, booleans are flags and other types require a value.
    /// </summary>
    public ArgumentType ArgumentType {
        get => m_argumentType;
        set {
            m_argumentType = value;
            HasArgumentType = true;
        }
    }

    /// <summary>
    /// Gets whether attribute initialization explicitly set the argument mode rather than leaving it to type inference.
    /// </summary>
    internal bool HasArgumentType { get; private set; }

}

/// <summary>
/// Declares a nullable property containing the options of a top-level command.
/// </summary>
/// <remarks>
/// The property must initially be null and expose a concrete model with a public parameterless constructor.
/// Only the selected command is constructed. Its properties may declare options and operands, but no nested commands.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class CliCommandAttribute: Attribute {

    /// <summary>
    /// Creates a command with an explicit bare name.
    /// </summary>
    /// <param name="name">The command name, without an option prefix.</param>
    public CliCommandAttribute(string name) {
        Name = name;
    }

    /// <summary>
    /// Gets the command name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets the description used in generated help.
    /// </summary>
    public string Description { get; set; } = string.Empty;

}

/// <summary>
/// Captures non-option arguments in a string array on the active options model.
/// </summary>
/// <remarks>
/// At most one capture property is allowed per model. Values are appended to initialized contents in encounter order.
/// After command selection, operands are captured by the command model rather than the root model.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class CliArgumentsAttribute: Attribute {

}
