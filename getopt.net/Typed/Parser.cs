namespace getopt.net.Typed;

using System.Collections.Generic;

/// <summary>
/// Parses arguments into fresh models using an immutable declaration snapshot.
/// </summary>
/// <typeparam name="T">The root options model.</typeparam>
/// <remarks>
/// Each call copies the arguments and creates new model and engine instances, allowing concurrent parser reuse.
/// Constructors and setters supplied by applications remain responsible for their own external side effects.
/// </remarks>
public sealed class Parser<T> where T: class, new() {

    /// <summary>
    /// Contains validated bindings shared by all invocations without retaining parsed values.
    /// </summary>
    private readonly ModelSchema m_schema;

    /// <summary>
    /// Contains the parser's private copy of command-line convention settings.
    /// </summary>
    private readonly ParserSettings m_settings;

    /// <summary>
    /// Creates a parser from a validated schema and settings snapshot.
    /// </summary>
    /// <param name="schema">Bindings produced by the builder's compilation step.</param>
    /// <param name="settings">A settings copy owned exclusively by this parser.</param>
    internal Parser(ModelSchema schema, ParserSettings settings) {
        m_schema = schema;
        m_settings = settings;
    }

    /// <summary>
    /// Returns a new options model or throws a TypedParseException for the first input error.
    /// </summary>
    /// <param name="args">Command-line arguments; the caller's array is not modified.</param>
    /// <returns>A populated root model, with only the selected command property populated.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="args"/> is null.</exception>
    /// <exception cref="TypedParseException">An option, command, operand, required value or conversion is invalid.</exception>
    /// <exception cref="ArgumentException">A newly constructed model has an already populated command property.</exception>
    /// <remarks>Model-construction, property-access and environmental failures propagate to the caller.</remarks>
    public T Parse(string[] args) {
        if (!TryParse(args, out var options, out var error)) { throw new TypedParseException(error!); }

        return options!;
    }

    /// <summary>
    /// Returns false and the first input error on failure. Configuration, model and I/O failures are not suppressed.
    /// </summary>
    /// <param name="args">Command-line arguments; the caller's array is not modified.</param>
    /// <param name="options">The completed root model on success; null for an input error.</param>
    /// <param name="error">The first input error on failure; null on success.</param>
    /// <returns>True when parsing and required-presence validation succeed; false for invalid input.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="args"/> is null.</exception>
    /// <exception cref="ArgumentException">A newly constructed model has an already populated command property.</exception>
    /// <remarks>
    /// Partial models are not returned. Required options are checked for the root and selected command only.
    /// Constructor, getter, setter and file-access failures are not converted into <see cref="ParseError"/> values.
    /// </remarks>
    public bool TryParse(string[] args, out T? options, out ParseError? error) {
        if (args is null) { throw new ArgumentNullException(nameof(args)); }

        options = null;
        error = null;
        var root = (T)m_schema.CreateModel();
        var model = (object)root;
        var schema = m_schema;
        var parser = CreateEngine((string[])args.Clone());
        var seen = new HashSet<OptionBinding>();
        CommandBinding? selectedCommand = null;

        while (true) {
            int id;
            string? argument;
            try {
                id = parser.GetNextOpt(out argument);
            } catch (ParseException exception) {
                error = exception switch {
                    CommandMisspeltException misspelt => new ParseError(ParseErrorKind.UnknownCommand, exception.Message,
                                                                      misspelt.Command, misspelt.Command, misspelt.PossibleCommands),
                    CommandNotFoundException unknown => new ParseError(ParseErrorKind.UnknownCommand, exception.Message,
                                                                      unknown.Command, unknown.Command, unknown.PossibleCommands),
                    _ => new ParseError(exception.IsMissingArgument ? ParseErrorKind.MissingArgument : ParseErrorKind.UnknownOption,
                                        exception.Message, exception.Option, exception.IsMissingArgument ? null : exception.Option)
                };

                return false;
            }

            // A bare command can be consumed in the same engine call that returns its first option or end of input.
            if (selectedCommand is null && parser.SelectedCommand is not null) {
                selectedCommand = m_schema.Commands.Single(command => command.Name == parser.SelectedCommand.Name);
                schema = selectedCommand.Model;
                model = schema.CreateModel();
                selectedCommand.Property.SetValue(root, model);
            }

            if (id == -1) { break; }
            if (id == GetOpt.NonOptChar) {
                if (schema.Arguments is null) {
                    error = new ParseError(ParseErrorKind.UnexpectedArgument, $"Unexpected argument '{argument}'.", value: argument);

                    return false;
                }

                var previous = (string[]?)schema.Arguments.GetValue(model) ?? Array.Empty<string>();
                schema.Arguments.SetValue(model, previous.Concat(new[] { argument! }).ToArray());
                continue;
            }

            var binding = schema.Options.Single(option => option.Id == id);
            seen.Add(binding);
            if (argument is null && binding.ArgumentType == ArgumentType.Optional) { continue; }

            object? value;
            try {
                value = binding.ConvertValue(argument);
            } catch (Exception exception) when (IsConversionError(exception)) {
                error = new ParseError(ParseErrorKind.InvalidValue, $"Invalid value '{argument}' for option '--{binding.Name}': {exception.Message}", binding.Name, argument);

                return false;
            }

            // Keep application setter failures outside the input-conversion exception filter.
            binding.Assign(model, value);
        }

        var required = m_schema.Options.Concat(selectedCommand?.Model.Options ?? Array.Empty<OptionBinding>()).FirstOrDefault(option => option.Required && !seen.Contains(option));
        if (required is not null) {
            error = new ParseError(ParseErrorKind.MissingOption, $"Required option '--{required.Name}' was not supplied.", required.Name);

            return false;
        }

        options = root;

        return true;
    }

    /// <summary>
    /// Generates help for global options and commands without parsing or constructing a model.
    /// </summary>
    /// <param name="config">Formatting and application metadata, or null for the existing help defaults.</param>
    /// <returns>Generated help text, or an empty string when no options or commands are declared.</returns>
    public string GenerateHelpText(HelpTextConfig? config = null) {
        return Extensions.GenerateHelpText(CreateEngine(Array.Empty<string>()), config, option => option.Value <= char.MaxValue ? (char?)option.Value : null);
    }

    /// <summary>
    /// Creates an isolated legacy parser with strict input handling and this schema's active scopes.
    /// </summary>
    /// <param name="args">An array owned by the invocation; the engine may modify and expand it.</param>
    /// <returns>A fresh engine with no selected command or consumed arguments.</returns>
    private GetOpt CreateEngine(string[] args) {
        return new GetOpt {
            AppArgs = args,
            Options = m_schema.CreateOptions(),
            ShortOpts = m_schema.CreateShortOptions(),
            Commands = m_schema.Commands.Select(command => new MainCommand(
                command.Name, command.Description, command.Model.CreateShortOptions(), command.Model.CreateOptions()
            )).ToArray(),
            IgnoreInvalidOptions = false,
            IgnoreMissingArgument = false,
            AllowWindowsConventions = m_settings.AllowWindowsConventions,
            AllowPowershellConventions = m_settings.AllowPowershellConventions,
            AllowParamFiles = m_settings.AllowParamFiles,
            CaseInsensitiveMatching = m_settings.CaseInsensitiveMatching,
            DoubleDashStopsParsing = m_settings.DoubleDashStopsParsing
        };
    }

    /// <summary>
    /// Recognizes conversion failures, including errors wrapped by a type converter.
    /// </summary>
    /// <param name="exception">The failure raised while converting an option value.</param>
    /// <returns>True for supported input-conversion error types or an inner exception of one of those types.</returns>
    private static bool IsConversionError(Exception exception) {
        return exception is FormatException || exception is OverflowException || exception is ArgumentException || exception is NotSupportedException ||
               exception.InnerException is not null && IsConversionError(exception.InnerException);
    }

}
