namespace getopt.net.Typed;

/// <summary>
/// Convenience methods for parsing models declared with CLI attributes.
/// </summary>
public static class CommandLine {

    /// <summary>
    /// Creates a reusable parser from an attributed model.
    /// </summary>
    /// <typeparam name="T">A concrete attributed model with a public parameterless constructor.</typeparam>
    /// <param name="settings">Accepted conventions, or null for the defaults.</param>
    /// <returns>A validated parser that can be reused across invocations.</returns>
    /// <exception cref="ArgumentException">The attributed declarations contain unsupported or conflicting bindings.</exception>
    public static Parser<T> CreateParser<T>(ParserSettings? settings = null) where T: class, new() {
        return new ParserBuilder<T>(settings).UseAttributes().Build();
    }

    /// <summary>
    /// Parses arguments into a new attributed model or throws a TypedParseException for invalid input.
    /// </summary>
    /// <typeparam name="T">A concrete attributed model with a public parameterless constructor.</typeparam>
    /// <param name="args">Command-line arguments; the caller's array is not modified.</param>
    /// <param name="settings">Accepted conventions, or null for the defaults.</param>
    /// <returns>The completed options model.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="args"/> is null.</exception>
    /// <exception cref="ArgumentException">The model or its declarations are invalid.</exception>
    /// <exception cref="TypedParseException">An input error prevents parsing.</exception>
    /// <remarks>Creates a parser for each call. Use <see cref="CreateParser{T}"/> when parsing repeatedly.</remarks>
    public static T Parse<T>(string[] args, ParserSettings? settings = null) where T: class, new() {
        return CreateParser<T>(settings).Parse(args);
    }

    /// <summary>
    /// Parses arguments, returning false and the first input error on failure. Configuration and application failures still throw.
    /// </summary>
    /// <typeparam name="T">A concrete attributed model with a public parameterless constructor.</typeparam>
    /// <param name="args">Command-line arguments; the caller's array is not modified.</param>
    /// <param name="options">The completed model on success; null for an input error.</param>
    /// <param name="error">The first input error on failure; null on success.</param>
    /// <param name="settings">Accepted conventions, or null for the defaults.</param>
    /// <returns>True on success; false for invalid command-line input.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="args"/> is null.</exception>
    /// <exception cref="ArgumentException">The model or its declarations are invalid.</exception>
    /// <remarks>Constructor, property-access and file-access failures propagate just as they do from <see cref="Parser{T}.TryParse"/>.</remarks>
    public static bool TryParse<T>(string[] args, out T? options, out ParseError? error, ParserSettings? settings = null) where T: class, new() {
        return CreateParser<T>(settings).TryParse(args, out options, out error);
    }

}
