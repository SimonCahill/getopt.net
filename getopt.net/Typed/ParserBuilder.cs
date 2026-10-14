using System.Linq.Expressions;
using System.Reflection;

namespace getopt.net.Typed;

/// <summary>
/// Declares typed options and a single level of commands using property expressions.
/// </summary>
/// <typeparam name="T">The options model.</typeparam>
/// <remarks>
/// Models must be concrete classes with public parameterless constructors and public readable, writable properties.
/// Declarations are mutable until <see cref="Build"/> creates a snapshot. Attributes are read only by <see cref="UseAttributes"/>.
/// </remarks>
public sealed class ParserBuilder<T> where T: class, new() {

    /// <summary>
    /// Accumulates declarations until they are validated and copied into a parser schema.
    /// </summary>
    private readonly ModelDefinition m_definition = new(typeof(T));

    /// <summary>
    /// Stores the construction-time copy of convention settings used by this builder and its commands.
    /// </summary>
    private readonly ParserSettings m_settings;

    /// <summary>
    /// Creates a builder. Settings are copied when the builder is created.
    /// </summary>
    /// <param name="settings">Accepted command-line conventions, or null to use <see cref="ParserSettings"/> defaults.</param>
    public ParserBuilder(ParserSettings? settings = null) {
        m_settings = (settings ?? new ParserSettings()).Copy();
    }

    /// <summary>
    /// Binds a boolean flag which becomes true when present.
    /// </summary>
    /// <param name="property">A direct property selector, such as <c>options =&gt; options.Verbose</c>.</param>
    /// <param name="name">The explicit long name, without an option prefix.</param>
    /// <param name="shortName">An optional ASCII letter or digit used as the short alias.</param>
    /// <param name="description">The description displayed in generated help.</param>
    /// <param name="required">Whether this option must appear in its active scope.</param>
    /// <returns>This builder for further declarations.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="property"/> is null.</exception>
    /// <exception cref="ArgumentException">The expression does not directly select a model property.</exception>
    public ParserBuilder<T> AddFlag(Expression<Func<T, bool>> property, string name, char? shortName = null, string description = "", bool required = false) {
        return AddOption(property, name, shortName, description, ArgumentType.None, required);
    }

    /// <summary>
    /// Binds an option. Boolean values default to flags; all other values require an argument.
    /// </summary>
    /// <typeparam name="TValue">The property type, including nullable values, arrays or lists of supported scalar values.</typeparam>
    /// <param name="property">A direct selector for the property receiving converted values.</param>
    /// <param name="name">The explicit long name, without an option prefix.</param>
    /// <param name="shortName">An optional ASCII letter or digit used as the short alias.</param>
    /// <param name="description">The description displayed in generated help.</param>
    /// <param name="argumentType">An explicit argument mode, or null to infer it from the scalar or collection element type.</param>
    /// <param name="required">Whether the option itself must appear; independent of whether its argument is required.</param>
    /// <returns>This builder for further declarations.</returns>
    /// <remarks>
    /// Absent options and omitted optional arguments preserve initializers. Repeated scalar values overwrite earlier values;
    /// arrays and lists append occurrences to their initialized contents. Conversion uses invariant culture.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="property"/> is null.</exception>
    /// <exception cref="ArgumentException">The expression does not directly select a model property.</exception>
    public ParserBuilder<T> AddOption<TValue>(Expression<Func<T, TValue>> property, string name, char? shortName = null,
                                            string description = "", ArgumentType? argumentType = null, bool required = false) {
        m_definition.Options.Add(new OptionDefinition(GetProperty(property), name, shortName, description, argumentType, required));

        return this;
    }

    /// <summary>
    /// Binds a top-level command to a property that must initially be null.
    /// </summary>
    /// <typeparam name="TCommand">The concrete command model with a public parameterless constructor.</typeparam>
    /// <param name="property">The property populated only when this command is selected.</param>
    /// <param name="name">The bare command name.</param>
    /// <param name="configure">Configures the command's options and optional operand capture.</param>
    /// <param name="description">The description displayed beside the command in generated help.</param>
    /// <returns>This builder for further declarations.</returns>
    /// <remarks>Commands are optional. Only the selected command's options are accepted after selection; nested commands are rejected.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="property"/> or <paramref name="configure"/> is null.</exception>
    /// <exception cref="ArgumentException">The expression does not directly select a model property.</exception>
    public ParserBuilder<T> AddCommand<TCommand>(Expression<Func<T, TCommand?>> property, string name, Action<ParserBuilder<TCommand>> configure,
                                                string description = "") where TCommand: class, new() {
        if (configure is null) { throw new ArgumentNullException(nameof(configure)); }

        var builder = new ParserBuilder<TCommand>(m_settings);
        configure(builder);
        m_definition.Commands.Add(new CommandDefinition(GetProperty(property), name, description, builder.m_definition));

        return this;
    }

    /// <summary>
    /// Binds non-option arguments to a string array on this model.
    /// </summary>
    /// <param name="property">The single property receiving operands in encounter order, after its initialized contents.</param>
    /// <returns>This builder for further declarations.</returns>
    /// <remarks>After command selection, operands belong to the command model. Unbound operands produce an input error.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="property"/> is null.</exception>
    /// <exception cref="ArgumentException">The expression does not directly select a model property.</exception>
    public ParserBuilder<T> AddArguments(Expression<Func<T, string[]>> property) {
        m_definition.Arguments.Add(GetProperty(property));

        return this;
    }

    /// <summary>
    /// Adds attributed declarations, including attributed command models. Duplicates are rejected by Build.
    /// </summary>
    /// <returns>This builder with attributed declarations appended to the existing declarations.</returns>
    /// <exception cref="ArgumentException">An attributed command contains another command.</exception>
    public ParserBuilder<T> UseAttributes() {
        m_definition.AddAttributes();

        return this;
    }

    /// <summary>
    /// Validates declarations and creates an independent, reusable parser.
    /// </summary>
    /// <returns>A parser whose declarations and settings are unaffected by later builder changes.</returns>
    /// <exception cref="ArgumentException">A model, property, name, alias, conversion or binding is unsupported or conflicts within its scope.</exception>
    public Parser<T> Build() {
        return new Parser<T>(m_definition.Compile(m_settings), m_settings.Copy());
    }

    /// <summary>
    /// Extracts a property without compiling or executing the selector expression.
    /// </summary>
    /// <typeparam name="TValue">The selected property's type.</typeparam>
    /// <param name="expression">A direct model property access; nested access, fields and computed expressions are rejected.</param>
    /// <returns>The selected property's reflection metadata.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="expression"/> is null.</exception>
    /// <exception cref="ArgumentException">The expression does not directly select a model property.</exception>
    private static PropertyInfo GetProperty<TValue>(Expression<Func<T, TValue>> expression) {
        if (expression is null) { throw new ArgumentNullException(nameof(expression)); }
        if (expression.Body is not MemberExpression member || member.Member is not PropertyInfo property || member.Expression != expression.Parameters[0]) {
            throw new ArgumentException("Select a property directly on the options model.", nameof(expression));
        }

        return property;
    }

}
