namespace getopt.net.Typed;

/// <summary>
/// Configures the command-line conventions accepted by a typed parser.
/// </summary>
public sealed class ParserSettings {

    /// <summary>
    /// Gets or sets whether Windows option prefixes and separators are accepted.
    /// </summary>
    /// <remarks>Defaults to false. When enabled, slash prefixes and colon separators are recognized by the legacy engine.</remarks>
    public bool AllowWindowsConventions { get; set; }

    /// <summary>
    /// Gets or sets whether PowerShell-style long options are accepted.
    /// </summary>
    /// <remarks>Defaults to false. When enabled, a single dash can introduce a declared long option.</remarks>
    public bool AllowPowershellConventions { get; set; }

    /// <summary>
    /// Gets or sets whether parameter files are expanded by the getopt parser.
    /// </summary>
    /// <remarks>Defaults to false. Existing files introduced by an at sign are read using the engine's append-to-input ordering.</remarks>
    public bool AllowParamFiles { get; set; }

    /// <summary>
    /// Gets or sets whether long option and command names ignore case. Short aliases remain case-sensitive.
    /// </summary>
    /// <remarks>Defaults to false. Matching uses the invariant culture rather than the process's current culture.</remarks>
    public bool CaseInsensitiveMatching { get; set; }

    /// <summary>
    /// Gets or sets whether a double dash ends option parsing. Defaults to true.
    /// </summary>
    public bool DoubleDashStopsParsing { get; set; } = true;

    /// <summary>
    /// Copies the value-only settings so later caller mutations cannot affect a builder or parser.
    /// </summary>
    /// <returns>An independent settings instance.</returns>
    internal ParserSettings Copy() {
        return (ParserSettings)MemberwiseClone();
    }

}
