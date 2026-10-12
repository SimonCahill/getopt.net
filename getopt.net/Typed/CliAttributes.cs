namespace getopt.net.Typed;

/// <summary>
/// Declares a property bound to a command-line option.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class CliOptionAttribute: Attribute {

    private ArgumentType m_argumentType = ArgumentType.Required;

    /// <summary>
    /// Creates an option with an explicit long name, without a prefix.
    /// </summary>
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

    internal bool HasArgumentType { get; private set; }

}

/// <summary>
/// Declares a nullable property containing the options of a top-level command.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class CliCommandAttribute: Attribute {

    /// <summary>
    /// Creates a command with an explicit bare name.
    /// </summary>
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
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class CliArgumentsAttribute: Attribute {

}
