using System.Collections.Generic;
using System.Reflection;

namespace getopt.net.Typed;

/// <summary>
/// Collects fluent and attributed declarations before validation and schema compilation.
/// </summary>
internal sealed class ModelDefinition {

    /// <summary>
    /// Starts a mutable declaration set for one root or command model.
    /// </summary>
    /// <param name="modelType">The model type whose properties will be bound.</param>
    internal ModelDefinition(Type modelType) {
        ModelType = modelType;
    }

    /// <summary>
    /// Gets the root or command model type to validate and instantiate.
    /// </summary>
    internal Type ModelType { get; }

    /// <summary>
    /// Gets option declarations in registration order, including duplicates awaiting validation.
    /// </summary>
    internal List<OptionDefinition> Options { get; } = new();

    /// <summary>
    /// Gets top-level command declarations and their mutable child definitions.
    /// </summary>
    internal List<CommandDefinition> Commands { get; } = new();

    /// <summary>
    /// Gets operand capture declarations; compilation rejects more than one per model.
    /// </summary>
    internal List<PropertyInfo> Arguments { get; } = new();

    /// <summary>
    /// Appends attributed properties and their command definitions without silently discarding invalid property shapes.
    /// </summary>
    /// <param name="isCommand">Whether this model already belongs to a command, so further commands must be rejected.</param>
    /// <remarks>
    /// Nonpublic and static properties are inspected so their attributes lead to validation errors rather than being ignored.
    /// Repeated calls append declarations; duplicate detection occurs during compilation.
    /// </remarks>
    /// <exception cref="ArgumentException">An attributed command contains a nested command.</exception>
    internal void AddAttributes(bool isCommand = false) {
        foreach (var property in ModelType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)) {
            var option = (CliOptionAttribute?)Attribute.GetCustomAttribute(property, typeof(CliOptionAttribute), true);
            var command = (CliCommandAttribute?)Attribute.GetCustomAttribute(property, typeof(CliCommandAttribute), true);
            var arguments = Attribute.IsDefined(property, typeof(CliArgumentsAttribute), true);
            if (option is not null) {
                Options.Add(new OptionDefinition(property, option.Name, option.ShortName == '\0' ? null : option.ShortName,
                                                option.Description, option.HasArgumentType ? option.ArgumentType : null, option.Required));
            }

            if (arguments) { Arguments.Add(property); }
            if (command is not null) {
                if (isCommand) { throw new ArgumentException("Only one level of commands is supported."); }

                var definition = new ModelDefinition(property.PropertyType);
                definition.AddAttributes(true);
                Commands.Add(new CommandDefinition(property, command.Name, command.Description, definition));
            }
        }
    }

    /// <summary>
    /// Validates model construction, property access, names and aliases, then creates an independent schema snapshot.
    /// </summary>
    /// <param name="settings">Name-comparison rules shared by the root and its commands.</param>
    /// <param name="isCommand">Whether commands are forbidden in this scope.</param>
    /// <returns>Validated bindings in declaration order, with invocation-independent option identifiers.</returns>
    /// <remarks>
    /// Long-only identifiers start above the character range to avoid short aliases and legacy sentinel values.
    /// Option-name and short-alias uniqueness are enforced separately within each scope.
    /// </remarks>
    /// <exception cref="ArgumentException">A declaration is unsupported, duplicated or inconsistent with the model shape.</exception>
    internal ModelSchema Compile(ParserSettings settings, bool isCommand = false) {
        if (!ModelType.IsClass || ModelType.IsAbstract || ModelType.GetConstructor(Type.EmptyTypes) is null) {
            throw new ArgumentException($"Model '{ModelType.Name}' must be a concrete class with a public parameterless constructor.");
        }

        if (isCommand && Commands.Count > 0) { throw new ArgumentException("Only one level of commands is supported."); }
        if (Arguments.Count > 1) { throw new ArgumentException("Only one arguments property is allowed per model."); }

        var properties = new HashSet<PropertyInfo>();
        foreach (var property in Options.Select(option => option.Property).Concat(Commands.Select(command => command.Property)).Concat(Arguments)) {
            if (property.GetIndexParameters().Length != 0 || property.GetMethod is null || !property.GetMethod.IsPublic || property.GetMethod.IsStatic ||
                property.SetMethod is null || !property.SetMethod.IsPublic || property.SetMethod.IsStatic ||
                property.SetMethod.ReturnParameter.GetRequiredCustomModifiers().Any(type => type.FullName == "System.Runtime.CompilerServices.IsExternalInit")) {
                throw new ArgumentException($"Property '{property.Name}' must be a public readable and writable instance property.");
            }

            if (!properties.Add(property)) { throw new ArgumentException($"Property '{property.Name}' is bound more than once."); }
        }

        if (Arguments.Count == 1 && Arguments[0].PropertyType != typeof(string[])) {
            throw new ArgumentException("The arguments property must be a string array.");
        }

        var comparer = settings.CaseInsensitiveMatching ? StringComparer.InvariantCultureIgnoreCase : StringComparer.InvariantCulture;
        var names = new HashSet<string>(comparer);
        var aliases = new HashSet<char>();
        var bindings = new List<OptionBinding>();
        var nextId = char.MaxValue + 1;
        foreach (var option in Options) {
            ValidateName(option.Name);
            if (!names.Add(option.Name)) { throw new ArgumentException($"Duplicate option name '{option.Name}'."); }
            if (option.ShortName is char alias && (!IsAlias(alias) || !aliases.Add(alias))) {
                throw new ArgumentException($"Short alias '{alias}' must be unique within its scope and an ASCII letter or digit.");
            }

            bindings.Add(new OptionBinding(option, option.ShortName is char shortName ? shortName : nextId++));
        }

        names.Clear();
        var commands = new List<CommandBinding>();
        foreach (var command in Commands) {
            ValidateName(command.Name);
            if (!names.Add(command.Name)) { throw new ArgumentException($"Duplicate command name '{command.Name}'."); }

            commands.Add(new CommandBinding(command, command.Model.Compile(settings, true)));
        }

        return new ModelSchema(ModelType, bindings.ToArray(), commands.ToArray(), Arguments.SingleOrDefault());
    }

    /// <summary>
    /// Checks whether a short alias belongs to the supported ASCII letter or digit set.
    /// </summary>
    /// <param name="value">The proposed short alias.</param>
    /// <returns>True for ASCII letters or digits; false otherwise.</returns>
    private static bool IsAlias(char value) {
        return value >= 'a' && value <= 'z' || value >= 'A' && value <= 'Z' || value >= '0' && value <= '9';
    }

    /// <summary>
    /// Rejects names that the legacy engine could interpret as prefixes, separators or whitespace.
    /// </summary>
    /// <param name="name">The proposed explicit option or command name.</param>
    /// <exception cref="ArgumentException">The name is empty or contains reserved syntax.</exception>
    private static void ValidateName(string name) {
        if (string.IsNullOrWhiteSpace(name) || name[0] == '-' || name[0] == '/' || name[0] == '@' ||
            name.Any(character => char.IsWhiteSpace(character) || char.IsControl(character) || character == '=' || character == ':')) {
            throw new ArgumentException("Option and command names must be nonempty, without prefixes, whitespace, control characters or argument separators.");
        }
    }

}

/// <summary>
/// Stores an unvalidated property-to-option declaration shared by fluent and attribute configuration.
/// </summary>
internal sealed class OptionDefinition {

    /// <summary>
    /// Captures one option declaration without performing schema-wide validation.
    /// </summary>
    /// <param name="property">The destination property.</param>
    /// <param name="name">The explicit long option name.</param>
    /// <param name="shortName">The optional short alias.</param>
    /// <param name="description">The help description.</param>
    /// <param name="argumentType">An explicit argument mode, or null for type inference.</param>
    /// <param name="required">Whether the option must appear in its active scope.</param>
    internal OptionDefinition(PropertyInfo property, string name, char? shortName, string description, ArgumentType? argumentType, bool required) {
        Property = property;
        Name = name;
        ShortName = shortName;
        Description = description;
        ArgumentType = argumentType;
        Required = required;
    }

    /// <summary>
    /// Gets the property receiving converted values.
    /// </summary>
    internal PropertyInfo Property { get; }

    /// <summary>
    /// Gets the explicit long name before validation.
    /// </summary>
    internal string Name { get; }

    /// <summary>
    /// Gets the optional short alias before validation.
    /// </summary>
    internal char? ShortName { get; }

    /// <summary>
    /// Gets the help description.
    /// </summary>
    internal string Description { get; }

    /// <summary>
    /// Gets the declared argument mode, or null when the scalar type should determine it.
    /// </summary>
    internal ArgumentType? ArgumentType { get; }

    /// <summary>
    /// Gets whether option presence is required, independently of argument mode.
    /// </summary>
    internal bool Required { get; }

}

/// <summary>
/// Associates a command property with its name, help text and mutable child declarations.
/// </summary>
internal sealed class CommandDefinition {

    /// <summary>
    /// Captures a command declaration for later validation.
    /// </summary>
    /// <param name="property">The root property that will receive the selected command model.</param>
    /// <param name="name">The bare command name.</param>
    /// <param name="description">The help description.</param>
    /// <param name="model">The command model declarations.</param>
    internal CommandDefinition(PropertyInfo property, string name, string description, ModelDefinition model) {
        Property = property;
        Name = name;
        Description = description;
        Model = model;
    }

    /// <summary>
    /// Gets the root property populated when this command is selected.
    /// </summary>
    internal PropertyInfo Property { get; }

    /// <summary>
    /// Gets the bare command name before validation.
    /// </summary>
    internal string Name { get; }

    /// <summary>
    /// Gets the command help description.
    /// </summary>
    internal string Description { get; }

    /// <summary>
    /// Gets the mutable declarations for this command model.
    /// </summary>
    internal ModelDefinition Model { get; }

}

/// <summary>
/// Holds a compiled scope shared by parser invocations without storing invocation-specific values.
/// </summary>
/// <remarks>
/// The binding arrays are created during compilation and treated as immutable by parser code.
/// </remarks>
internal sealed class ModelSchema {

    /// <summary>
    /// Stores the validated bindings for a root or command model.
    /// </summary>
    /// <param name="modelType">The concrete type to instantiate for each invocation.</param>
    /// <param name="options">Validated option bindings in declaration order.</param>
    /// <param name="commands">Validated commands, empty for a command scope.</param>
    /// <param name="arguments">The optional string-array property used for operands.</param>
    internal ModelSchema(Type modelType, OptionBinding[] options, CommandBinding[] commands, PropertyInfo? arguments) {
        ModelType = modelType;
        Options = options;
        Commands = commands;
        Arguments = arguments;
    }

    /// <summary>
    /// Gets the concrete model type created for this scope.
    /// </summary>
    internal Type ModelType { get; }

    /// <summary>
    /// Gets the validated option bindings in declaration order.
    /// </summary>
    internal OptionBinding[] Options { get; }

    /// <summary>
    /// Gets the validated top-level command bindings.
    /// </summary>
    internal CommandBinding[] Commands { get; }

    /// <summary>
    /// Gets the operand capture property, or null when operands are rejected.
    /// </summary>
    internal PropertyInfo? Arguments { get; }

    /// <summary>
    /// Constructs a fresh model and verifies that command properties can represent an unselected state.
    /// </summary>
    /// <returns>A model containing its constructor and property-initializer defaults.</returns>
    /// <remarks>
    /// Constructor and property-getter failures propagate through reflection; they are not command-line input errors.
    /// </remarks>
    /// <exception cref="ArgumentException">A command property is already populated by the model initializer.</exception>
    internal object CreateModel() {
        var model = Activator.CreateInstance(ModelType)!;
        if (Commands.Any(command => command.Property.GetValue(model) is not null)) {
            throw new ArgumentException("Command properties must initially be null.");
        }

        return model;
    }

    /// <summary>
    /// Materializes fresh legacy option definitions for a parser invocation.
    /// </summary>
    /// <returns>An option array containing binding identifiers, argument modes and help descriptions.</returns>
    internal Option[] CreateOptions() {
        return Options.Select(option => new Option(option.Name, option.ArgumentType, option.Id, option.Description)).ToArray();
    }

    /// <summary>
    /// Encodes declared short aliases and their argument modes for the legacy engine.
    /// </summary>
    /// <returns>A short-option string beginning with the operand-reporting prefix, followed only by declared aliases.</returns>
    /// <remarks>
    /// The leading minus sign distinguishes operands from unknown-option sentinels.
    /// Long-only identifiers are deliberately excluded from the short-option string.
    /// </remarks>
    internal string CreateShortOptions() {
        // The prefix makes operands distinguishable from invalid-option sentinels.
        return "-" + string.Concat(Options.Where(option => option.ShortName.HasValue).Select(option =>
            option.ShortName + (option.ArgumentType == ArgumentType.Required ? ":" : option.ArgumentType == ArgumentType.Optional ? ";" : string.Empty)));
    }

}

/// <summary>
/// Connects a validated command declaration to its compiled command-model schema.
/// </summary>
internal sealed class CommandBinding {

    /// <summary>
    /// Copies command metadata and retains its compiled child scope.
    /// </summary>
    /// <param name="definition">The validated command declaration.</param>
    /// <param name="model">The compiled command scope.</param>
    internal CommandBinding(CommandDefinition definition, ModelSchema model) {
        Property = definition.Property;
        Name = definition.Name;
        Description = definition.Description;
        Model = model;
    }

    /// <summary>
    /// Gets the root model property assigned when the command is selected.
    /// </summary>
    internal PropertyInfo Property { get; }

    /// <summary>
    /// Gets the validated bare command name.
    /// </summary>
    internal string Name { get; }

    /// <summary>
    /// Gets the command help description.
    /// </summary>
    internal string Description { get; }

    /// <summary>
    /// Gets the validated bindings for the command model.
    /// </summary>
    internal ModelSchema Model { get; }

}
