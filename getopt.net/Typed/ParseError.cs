namespace getopt.net.Typed;

using System.Collections.Generic;

/// <summary>
/// Identifies an invalid command-line input.
/// </summary>
public enum ParseErrorKind {

    /// <summary>
    /// An option is not declared in the active scope.
    /// </summary>
    UnknownOption,

    /// <summary>
    /// A command is not declared.
    /// </summary>
    UnknownCommand,

    /// <summary>
    /// An option requires an argument that was not supplied.
    /// </summary>
    MissingArgument,

    /// <summary>
    /// An option marked as required did not appear.
    /// </summary>
    MissingOption,

    /// <summary>
    /// An argument could not be converted to the property's type.
    /// </summary>
    InvalidValue,

    /// <summary>
    /// A non-option argument has no capture property.
    /// </summary>
    UnexpectedArgument

}

/// <summary>
/// Describes the first invalid input encountered during a typed parse.
/// </summary>
public sealed class ParseError {

    /// <summary>
    /// Captures an input error and takes a read-only copy of command suggestions.
    /// </summary>
    /// <param name="kind">The input-error category.</param>
    /// <param name="message">A human-readable explanation of the failure.</param>
    /// <param name="name">The relevant option or command name, when known.</param>
    /// <param name="value">The supplied value, when available.</param>
    /// <param name="possibleCommands">Suggested command names in declaration order, or null for no suggestions.</param>
    internal ParseError(ParseErrorKind kind, string message, string? name = null, string? value = null, IEnumerable<string>? possibleCommands = null) {
        Kind = kind;
        Message = message;
        Name = name;
        Value = value;
        PossibleCommands = Array.AsReadOnly((possibleCommands ?? Array.Empty<string>()).ToArray());
    }

    /// <summary>
    /// Gets the error category.
    /// </summary>
    public ParseErrorKind Kind { get; }

    /// <summary>
    /// Gets a human-readable explanation.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the relevant option or command name, when available.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Gets the supplied value, when available.
    /// </summary>
    public string? Value { get; }

    /// <summary>
    /// Gets suggestions for an unknown command.
    /// </summary>
    public IReadOnlyList<string> PossibleCommands { get; }

}

/// <summary>
/// Reports invalid input from a typed parser's Parse method.
/// </summary>
public sealed class TypedParseException: ParseException {

    /// <summary>
    /// Creates an exception containing a structured input error.
    /// </summary>
    /// <param name="error">The input error reported by the typed parser.</param>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is null.</exception>
    public TypedParseException(ParseError error): base(error?.Name, error?.Message ?? throw new ArgumentNullException(nameof(error))) {
        Error = error;
    }

    /// <summary>
    /// Gets the structured input error.
    /// </summary>
    public ParseError Error { get; }

}
