namespace getopt.net.Typed;

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

/// <summary>
/// Validates an option value type and implements invariant conversion and property assignment.
/// </summary>
internal sealed class OptionBinding {

    /// <summary>
    /// Stores the scalar property type or collection element type, retaining nullable information.
    /// </summary>
    private readonly Type m_valueType;
    /// <summary>
    /// Records whether occurrences append to a one-dimensional array or a generic list.
    /// </summary>
    private readonly bool m_isCollection;

    /// <summary>
    /// Validates the property value type and resolves an explicit or inferred argument mode.
    /// </summary>
    /// <param name="definition">The option declaration whose names and property shape were validated by the model compiler.</param>
    /// <param name="id">The short alias code or synthetic long-only identifier returned by the engine.</param>
    /// <exception cref="ArgumentException">The collection shape, argument mode or string conversion is unsupported.</exception>
    internal OptionBinding(OptionDefinition definition, int id) {
        Property = definition.Property;
        Name = definition.Name;
        ShortName = definition.ShortName;
        Description = definition.Description;
        Required = definition.Required;
        Id = id;
        var propertyType = Property.PropertyType;
        m_isCollection = propertyType.IsArray || propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>);
        if (propertyType.IsArray && propertyType.GetArrayRank() != 1) { throw new ArgumentException("Only one-dimensional option arrays are supported."); }

        m_valueType = propertyType.IsArray ? propertyType.GetElementType()! : m_isCollection ? propertyType.GetGenericArguments()[0] : propertyType;
        var valueType = Nullable.GetUnderlyingType(m_valueType) ?? m_valueType;
        ArgumentType = definition.ArgumentType ?? (valueType == typeof(bool) ? ArgumentType.None : ArgumentType.Required);
        if (!Enum.IsDefined(typeof(ArgumentType), ArgumentType) || ArgumentType == ArgumentType.None && valueType != typeof(bool)) {
            throw new ArgumentException($"Option '{Name}' has an invalid argument mode for '{Property.PropertyType.Name}'.");
        }

        if (valueType != typeof(string) && !valueType.IsEnum && !TypeDescriptor.GetConverter(valueType).CanConvertFrom(typeof(string))) {
            throw new ArgumentException($"Option '{Name}' has unsupported value type '{valueType.Name}'.");
        }
    }

    /// <summary>
    /// Gets the property used to read defaults and assign converted values.
    /// </summary>
    internal PropertyInfo Property { get; }

    /// <summary>
    /// Gets the validated long option name.
    /// </summary>
    internal string Name { get; }

    /// <summary>
    /// Gets the optional declared short alias.
    /// </summary>
    internal char? ShortName { get; }

    /// <summary>
    /// Gets the option help description.
    /// </summary>
    internal string Description { get; }

    /// <summary>
    /// Gets whether this option must occur when its scope is active.
    /// </summary>
    internal bool Required { get; }

    /// <summary>
    /// Gets the engine return value used to locate this binding within its scope.
    /// </summary>
    internal int Id { get; }

    /// <summary>
    /// Gets the resolved argument mode after boolean inference and validation.
    /// </summary>
    internal ArgumentType ArgumentType { get; }

    /// <summary>
    /// Converts one raw argument without accessing or mutating the destination model.
    /// </summary>
    /// <param name="value">The argument returned by the engine, or null for an argument-less occurrence.</param>
    /// <returns>True for flags, the raw string for string properties, or the converted scalar value; empty nullable values become null.</returns>
    /// <remarks>
    /// Enum names ignore case. Other conversions use invariant culture through TypeDescriptor.
    /// Cached converters are locked while executing conversion so shared state is not accessed concurrently by this binder.
    /// </remarks>
    /// <exception cref="FormatException">The supplied text does not match the target format.</exception>
    /// <exception cref="OverflowException">The value exceeds the target type range.</exception>
    /// <exception cref="ArgumentException">The converter rejects the value, possibly wrapping a format or overflow error.</exception>
    /// <exception cref="NotSupportedException">The converter cannot perform the requested conversion.</exception>
    internal object? ConvertValue(string? value) {
        if (ArgumentType == ArgumentType.None) { return true; }
        if (value is null) { return null; }

        var valueType = Nullable.GetUnderlyingType(m_valueType) ?? m_valueType;
        if (valueType == typeof(string)) { return value; }
        if (Nullable.GetUnderlyingType(m_valueType) is not null && value.Length == 0) { return null; }
        if (valueType.IsEnum) { return Enum.Parse(valueType, value, true); }

        var converter = TypeDescriptor.GetConverter(valueType);
        // TypeDescriptor may cache converters, including stateful user implementations.
        lock (converter) {
            return converter.ConvertFrom(null, CultureInfo.InvariantCulture, value);
        }
    }

    /// <summary>
    /// Writes a scalar or appends a converted occurrence without mutating a shared collection initializer.
    /// </summary>
    /// <param name="model">The current root or command model.</param>
    /// <param name="value">The scalar value produced by conversion.</param>
    /// <remarks>
    /// Arrays and lists are copied before appending. Reflection and application setter failures propagate to the caller.
    /// </remarks>
    internal void Assign(object model, object? value) {
        if (!m_isCollection) {
            Property.SetValue(model, value);

            return;
        }

        var previous = (IList?)Property.GetValue(model);
        if (Property.PropertyType.IsArray) {
            var values = Array.CreateInstance(m_valueType, (previous?.Count ?? 0) + 1);
            previous?.CopyTo(values, 0);
            values.SetValue(value, values.Length - 1);
            Property.SetValue(model, values);
        } else {
            // Copy defaults instead of mutating a potentially shared initializer list.
            var values = (IList)Activator.CreateInstance(Property.PropertyType)!;
            if (previous is not null) {
                foreach (var item in previous) { values.Add(item); }
            }

            values.Add(value);
            Property.SetValue(model, values);
        }
    }

}
