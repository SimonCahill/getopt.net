namespace getopt.net.Typed;

/// <summary>
/// Convenience methods for parsing models declared with CLI attributes.
/// </summary>
public static class CommandLine {

    /// <summary>
    /// Creates a reusable parser from an attributed model.
    /// </summary>
    public static Parser<T> CreateParser<T>(ParserSettings? settings = null) where T: class, new() {
        return new ParserBuilder<T>(settings).UseAttributes().Build();
    }

    /// <summary>
    /// Parses arguments into a new attributed model or throws a TypedParseException for invalid input.
    /// </summary>
    public static T Parse<T>(string[] args, ParserSettings? settings = null) where T: class, new() {
        return CreateParser<T>(settings).Parse(args);
    }

    /// <summary>
    /// Parses arguments, returning false and the first input error on failure. Configuration and application failures still throw.
    /// </summary>
    public static bool TryParse<T>(string[] args, out T? options, out ParseError? error, ParserSettings? settings = null) where T: class, new() {
        return CreateParser<T>(settings).TryParse(args, out options, out error);
    }

}
