using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using getopt.net.Typed;

namespace getopt.net.tests;

/// <summary>
/// Verifies equivalent fluent and attributed binding, isolation, validation and legacy-engine compatibility.
/// </summary>
[TestClass]
public class TypedParserTests {

    /// <summary>
    /// Creates the fluent equivalent of the attributed fixture so both declaration paths can be compared.
    /// </summary>
    /// <param name="settings">Optional convention settings shared with the attributed parser.</param>
    /// <returns>A new compiled parser with the same declarations as the attributed fixture.</returns>
    private static Parser<AppOptions> FluentParser(ParserSettings? settings = null) {
        return new ParserBuilder<AppOptions>(settings)
            .AddFlag(options => options.Verbose, "verbose", 'v', "Verbose output.")
            .AddOption(options => options.Output, "output", 'o', "Output file.")
            .AddOption(options => options.Retries, "retries", 'r')
            .AddOption(options => options.Format, "format", 'f', argumentType: ArgumentType.Optional)
            .AddOption(options => options.Tags, "tag", 't')
            .AddOption(options => options.Numbers, "number")
            .AddOption(options => options.Enabled, "enabled", argumentType: ArgumentType.Required)
            .AddOption(options => options.Count, "count")
            .AddOption(options => options.Mode, "mode")
            .AddArguments(options => options.Arguments)
            .AddCommand(options => options.Config, "config", command => command
                .AddFlag(options => options.Create, "create", 'c', "Create configuration.")
                .AddOption(options => options.Output, "output", 'o')
                .AddArguments(options => options.Arguments), "Manage configuration.")
            .Build();
    }

    /// <summary>
    /// Compares both declaration paths using grouped options, repeated values, command bindings and identical help metadata.
    /// </summary>
    [TestMethod]
    public void FluentAndAttributesProduceEquivalentModelsAndHelp() {
        var arguments = new[] { "-voresult.txt", "--retries=7", "--tag=red", "-tblue", "--number=4", "--number=5", "config", "-cochild.txt", "operand" };
        var fluent = FluentParser();
        var attributed = CommandLine.CreateParser<AppOptions>();
        foreach (var parser in new[] { fluent, attributed }) {
            var result = parser.Parse(arguments);
            Assert.IsTrue(result.Verbose);
            Assert.AreEqual("result.txt", result.Output);
            Assert.AreEqual(7, result.Retries);
            CollectionAssert.AreEqual(new[] { "default", "red", "blue" }, result.Tags);
            CollectionAssert.AreEqual(new[] { 1, 4, 5 }, result.Numbers);
            Assert.IsTrue(result.Config!.Create);
            Assert.AreEqual("child.txt", result.Config.Output);
            CollectionAssert.AreEqual(new[] { "operand" }, result.Config.Arguments);
        }

        var config = new HelpTextConfig { ApplicationName = "example" };
        Assert.AreEqual(fluent.GenerateHelpText(config), attributed.GenerateHelpText(config));
    }

    /// <summary>
    /// Checks initialized defaults, omitted optional values, empty nullable values and last-occurrence scalar assignment.
    /// </summary>
    [TestMethod]
    public void DefaultsOptionalValuesAndLastScalarArePreserved() {
        foreach (var parser in new[] { FluentParser(), CommandLine.CreateParser<AppOptions>() }) {
            var empty = parser.Parse(Array.Empty<string>());
            Assert.AreEqual("output.txt", empty.Output);
            Assert.AreEqual(3, empty.Retries);
            Assert.IsNull(empty.Config);
            var result = parser.Parse(new[] { "--output=first", "-osecond", "--format", "--verbose", "--count=", "--enabled=false", "--mode=FAST" });
            Assert.AreEqual("second", result.Output);
            Assert.AreEqual("json", result.Format);
            Assert.IsNull(result.Count);
            Assert.IsFalse(result.Enabled);
            Assert.AreEqual(OutputMode.Fast, result.Mode);
        }
    }

    /// <summary>
    /// Checks that fluent and attributed parsers expose the same structured errors and throwing behavior.
    /// </summary>
    /// <param name="input">The invalid command-line token.</param>
    /// <param name="expected">The expected structured error category.</param>
    [DataTestMethod]
    [DataRow("--unknown", ParseErrorKind.UnknownOption)]
    [DataRow("-z", ParseErrorKind.UnknownOption)]
    [DataRow("--retries", ParseErrorKind.MissingArgument)]
    [DataRow("-r", ParseErrorKind.MissingArgument)]
    [DataRow("--retries=invalid", ParseErrorKind.InvalidValue)]
    [DataRow("--retries=99999999999999999", ParseErrorKind.InvalidValue)]
    [DataRow("confg", ParseErrorKind.UnknownCommand)]
    [DataRow("unrelated", ParseErrorKind.UnknownCommand)]
    public void BothStylesReportEquivalentErrors(string input, ParseErrorKind expected) {
        ParseError? previous = null;
        foreach (var parser in new[] { FluentParser(), CommandLine.CreateParser<AppOptions>() }) {
            Assert.IsFalse(parser.TryParse(new[] { input }, out var result, out var error));
            Assert.IsNull(result);
            Assert.AreEqual(expected, error!.Kind);
            var exception = Assert.ThrowsException<TypedParseException>(() => parser.Parse(new[] { input }));
            Assert.AreEqual(error.Message, exception.Error.Message);
            if (previous is not null) {
                Assert.AreEqual(previous.Name, error.Name);
                Assert.AreEqual(previous.Value, error.Value);
                Assert.AreEqual(previous.Message, error.Message);
                CollectionAssert.AreEqual(previous.PossibleCommands.ToArray(), error.PossibleCommands.ToArray());
            }

            previous = error;
        }
    }

    /// <summary>
    /// Checks global-only input, command-only input, scope isolation and misspelling suggestions.
    /// </summary>
    [TestMethod]
    public void CommandsRemainOptionalAndUseTheirOwnScope() {
        var parser = FluentParser();
        Assert.IsNull(parser.Parse(new[] { "--verbose" }).Config);
        Assert.IsNotNull(parser.Parse(new[] { "config" }).Config);
        Assert.IsFalse(parser.TryParse(new[] { "config", "--verbose" }, out _, out var error));
        Assert.AreEqual(ParseErrorKind.UnknownOption, error!.Kind);
        Assert.IsFalse(parser.TryParse(new[] { "confg" }, out _, out error));
        CollectionAssert.AreEqual(new[] { "config" }, error!.PossibleCommands.ToArray());
    }

    /// <summary>
    /// Checks presence requirements independently of value requirements and inactive command scopes.
    /// </summary>
    [TestMethod]
    public void RequiredPresenceIsSeparateFromArgumentModeAndCommandSelection() {
        var parser = new ParserBuilder<AppOptions>()
            .AddOption(options => options.Format, "format", argumentType: ArgumentType.Optional, required: true)
            .AddCommand(options => options.Config, "config", command => command.AddFlag(options => options.Create, "create", required: true))
            .Build();
        Assert.IsFalse(parser.TryParse(Array.Empty<string>(), out _, out var error));
        Assert.AreEqual(ParseErrorKind.MissingOption, error!.Kind);
        Assert.AreEqual("format", error.Name);
        Assert.AreEqual("json", parser.Parse(new[] { "--format" }).Format);
        Assert.IsFalse(parser.TryParse(new[] { "--format=json", "config" }, out _, out error));
        Assert.AreEqual(ParseErrorKind.MissingOption, error!.Kind);
        Assert.AreEqual("create", error.Name);
        Assert.IsTrue(parser.Parse(new[] { "--format=json", "config", "--create" }).Config!.Create);
        Assert.IsFalse(CommandLine.TryParse<RequiredOptions>(Array.Empty<string>(), out _, out error));
        Assert.AreEqual(ParseErrorKind.MissingOption, error!.Kind);
    }

    /// <summary>
    /// Checks the same option spelling in global and command scopes for both declaration styles.
    /// </summary>
    /// <param name="argument">The convention-specific option token.</param>
    /// <param name="windows">Whether Windows conventions are enabled.</param>
    /// <param name="powershell">Whether PowerShell conventions are enabled.</param>
    [DataTestMethod]
    [DataRow("--output=value", false, false)]
    [DataRow("-ovalue", false, false)]
    [DataRow("/output:value", true, false)]
    [DataRow("/ovalue", true, false)]
    [DataRow("-output=value", false, true)]
    public void ExistingConventionsAreAvailableInBothStyles(string argument, bool windows, bool powershell) {
        var settings = new ParserSettings { AllowWindowsConventions = windows, AllowPowershellConventions = powershell };
        foreach (var parser in new[] { FluentParser(settings), CommandLine.CreateParser<AppOptions>(settings) }) {
            Assert.AreEqual("value", parser.Parse(new[] { argument }).Output);
            Assert.AreEqual("value", parser.Parse(new[] { "config", argument }).Config!.Output);
        }
    }

    /// <summary>
    /// Checks case-insensitive names while retaining case-sensitive short aliases.
    /// </summary>
    [TestMethod]
    public void CasingSettingsApplyToNamesButNotShortAliases() {
        var parser = FluentParser(new ParserSettings { CaseInsensitiveMatching = true });
        Assert.IsTrue(parser.Parse(new[] { "--VERBOSE", "CONFIG", "--CREATE" }).Config!.Create);
        Assert.IsFalse(parser.TryParse(new[] { "-V" }, out _, out _));
        Assert.IsFalse(FluentParser().TryParse(new[] { "--VERBOSE" }, out _, out _));
    }

    /// <summary>
    /// Checks operand capture, rejection of unbound operands and suppression of command selection after double dash.
    /// </summary>
    [TestMethod]
    public void ArgumentsAreCapturedAndDoubleDashPreventsCommandSelection() {
        var parser = FluentParser();
        var result = parser.Parse(new[] { "--", "config", "--verbose" });
        Assert.IsNull(result.Config);
        Assert.IsFalse(result.Verbose);
        CollectionAssert.AreEqual(new[] { "config", "--verbose" }, result.Arguments);
        var noCommands = new ParserBuilder<AppOptions>().AddArguments(options => options.Arguments).Build();
        CollectionAssert.AreEqual(new[] { "one", "two" }, noCommands.Parse(new[] { "one", "two" }).Arguments);
        var noArguments = new ParserBuilder<AppOptions>().Build();
        Assert.IsFalse(noArguments.TryParse(new[] { "one" }, out _, out var error));
        Assert.AreEqual(ParseErrorKind.UnexpectedArgument, error!.Kind);
    }

    /// <summary>
    /// Guards the engine's append ordering when file values follow original command-line values.
    /// </summary>
    [TestMethod]
    public void ParameterFilesRetainEngineOrderingAndCommandBehavior() {
        var path = Path.GetTempFileName();
        try {
            File.WriteAllLines(path, new[] { "--output=file", "config", "--create" });
            var parser = FluentParser(new ParserSettings { AllowParamFiles = true });
            var result = parser.Parse(new[] { "@" + path, "--output=cli" });
            Assert.AreEqual("file", result.Output);
            Assert.IsTrue(result.Config!.Create);
        } finally {
            File.Delete(path);
        }
    }

    /// <summary>
    /// Checks caller-array preservation and separate root, command and collection state on repeated parses.
    /// </summary>
    [TestMethod]
    public void ParsingDoesNotMutateInputOrReuseModels() {
        var parser = FluentParser();
        var args = new[] { "--output=first", "--tag=extra", "config", "--create" };
        var original = (string[])args.Clone();
        var first = parser.Parse(args);
        var second = parser.Parse(args);
        CollectionAssert.AreEqual(original, args);
        Assert.AreNotSame(first, second);
        Assert.AreNotSame(first.Config, second.Config);
        CollectionAssert.AreEqual(first.Tags, second.Tags);
        Assert.IsNull(parser.Parse(Array.Empty<string>()).Config);
    }

    /// <summary>
    /// Checks invocation isolation when one compiled parser is used by multiple workers.
    /// </summary>
    [TestMethod]
    public void ParserCanBeUsedConcurrently() {
        var parser = FluentParser();
        Parallel.For(0, 40, index => {
            var result = parser.Parse(new[] { "--retries=" + index, "config", "--output=" + index });
            Assert.AreEqual(index, result.Retries);
            Assert.AreEqual(index.ToString(), result.Config!.Output);
        });
    }

    /// <summary>
    /// Checks construction-time settings copies, build-time schema copies and explicit attribute discovery.
    /// </summary>
    [TestMethod]
    public void BuilderAndSettingsAreSnapshotsAndAttributesAreOptIn() {
        var settings = new ParserSettings();
        var builder = new ParserBuilder<AppOptions>(settings).AddFlag(options => options.Verbose, "verbose");
        var parser = builder.Build();
        settings.CaseInsensitiveMatching = true;
        builder.AddOption(options => options.Output, "output");
        Assert.IsFalse(parser.TryParse(new[] { "--output=value" }, out _, out _));
        Assert.IsFalse(parser.TryParse(new[] { "--VERBOSE" }, out _, out _));
        Assert.AreEqual("value", builder.Build().Parse(new[] { "--output=value" }).Output);
        var mixed = new ParserBuilder<RequiredOptions>().UseAttributes().AddFlag(options => options.Verbose, "verbose").Build();
        Assert.IsTrue(mixed.Parse(new[] { "--name=x", "--verbose" }).Verbose);
    }

    /// <summary>
    /// Checks long-only labels, selected prefix conventions and wrapping of descriptions without spaces.
    /// </summary>
    [TestMethod]
    public void LongOnlyHelpHasNoSyntheticAliasesAndRespectsConventions() {
        var parser = new ParserBuilder<AppOptions>()
            .AddOption(options => options.Output, "output", description: "Output file.")
            .AddCommand(options => options.Config, "config", command => command.AddFlag(options => options.Create, "create"))
            .Build();
        var help = parser.GenerateHelpText(new HelpTextConfig { ApplicationName = "example", OptionConvention = OptionConvention.Windows });
        StringAssert.Contains(help, "/output");
        StringAssert.Contains(help, "        /create");
        Assert.IsFalse(help.Contains('\0'));
        Assert.IsFalse(help.Contains(", /"));
        var wrapping = new ParserBuilder<AppOptions>().AddOption(options => options.Output, "output", description: new string('x', 100)).Build();
        Assert.IsTrue(wrapping.GenerateHelpText(new HelpTextConfig { MaxWidth = 40 }).Split('\n').All(line => line.TrimEnd('\r').Length <= 40));
    }

    /// <summary>
    /// Checks schema validation before invalid declarations can reach token parsing.
    /// </summary>
    [TestMethod]
    public void DuplicateDeclarationsInvalidExpressionsAndUnsupportedTypesAreRejected() {
        Assert.ThrowsException<ArgumentException>(() => new ParserBuilder<AppOptions>().UseAttributes().UseAttributes().Build());
        Assert.ThrowsException<ArgumentException>(() => new ParserBuilder<AppOptions>().AddFlag(options => options.Verbose, "same")
            .AddOption(options => options.Output, "same").Build());
        Assert.ThrowsException<ArgumentException>(() => new ParserBuilder<AppOptions>().AddFlag(options => options.Verbose, "verbose", 'v')
            .AddOption(options => options.Output, "output", 'v').Build());
        Assert.ThrowsException<ArgumentException>(() => new ParserBuilder<AppOptions>().AddOption(options => options.Output, "output", '!').Build());
        Assert.ThrowsException<ArgumentException>(() => new ParserBuilder<AppOptions>().AddOption(options => options.Output, "--output").Build());
        Assert.ThrowsException<ArgumentException>(() => new ParserBuilder<AppOptions>().AddOption(options => options.Config!.Output, "output"));
        Assert.ThrowsException<ArgumentException>(() => new ParserBuilder<InvalidOptions>().AddOption(options => options.Data, "data").Build());
        Assert.ThrowsException<ArgumentException>(() => CommandLine.CreateParser<ReadOnlyOptions>());
        Assert.ThrowsException<ArgumentException>(() => CommandLine.CreateParser<NestedCommands>());
        Assert.ThrowsException<ArgumentException>(() => new ParserBuilder<AppOptions>(new ParserSettings { CaseInsensitiveMatching = true })
            .AddFlag(options => options.Verbose, "NAME").AddOption(options => options.Output, "name").Build());
    }

    /// <summary>
    /// Checks that TryParse does not absorb invalid model initialization, application failures or null API arguments.
    /// </summary>
    [TestMethod]
    public void ConfigurationAndApplicationFailuresAreNotInputErrors() {
        Assert.ThrowsException<ArgumentException>(() => CommandLine.TryParse<InitializedCommand>(Array.Empty<string>(), out _, out _));
        Assert.ThrowsException<TargetInvocationException>(() => CommandLine.TryParse<ThrowingSetter>(new[] { "--value=x" }, out _, out _));
        Assert.ThrowsException<TargetInvocationException>(() => CommandLine.TryParse<ThrowingConstructor>(Array.Empty<string>(), out _, out _));
        Assert.ThrowsException<ArgumentNullException>(() => FluentParser().TryParse(null!, out _, out _));
    }

    /// <summary>
    /// Checks that attributed properties must support public instance reads and ordinary writes.
    /// </summary>
    [TestMethod]
    public void NonPublicStaticAndInitOnlyAttributedPropertiesAreRejected() {
        Assert.ThrowsException<ArgumentException>(() => CommandLine.CreateParser<PrivateOptions>());
        Assert.ThrowsException<ArgumentException>(() => CommandLine.CreateParser<StaticOptions>());
        Assert.ThrowsException<ArgumentException>(() => CommandLine.CreateParser<InitOnlyOptions>());
    }

    /// <summary>
    /// Checks inherited required declarations and conversion of nullable and enum collection elements.
    /// </summary>
    [TestMethod]
    public void InheritedAttributesAndNullableCollectionElementsAreSupported() {
        var result = CommandLine.Parse<DerivedOptions>(new[] { "--name=inherited", "--values=2", "--values=", "--modes=fast", "--modes=normal" });
        Assert.AreEqual("inherited", result.Name);
        CollectionAssert.AreEqual(new int?[] { 2, null }, result.Values);
        CollectionAssert.AreEqual(new[] { OutputMode.Fast, OutputMode.Normal }, result.Modes);
    }

    /// <summary>
    /// Checks that appending occurrences leaves a list shared by model initializers unchanged.
    /// </summary>
    [TestMethod]
    public void CollectionParsingDoesNotModifySharedDefaults() {
        var defaults = SharedDefaults.Values;
        var parser = CommandLine.CreateParser<SharedDefaults>();
        CollectionAssert.AreEqual(new[] { 1, 2 }, parser.Parse(new[] { "--number=2" }).Numbers);
        CollectionAssert.AreEqual(new[] { 1 }, defaults);
        CollectionAssert.AreEqual(new[] { 1, 3 }, parser.Parse(new[] { "--number=3" }).Numbers);
    }

    /// <summary>
    /// Checks strict treatment of a disabled marker and optional explicit boolean values.
    /// </summary>
    [TestMethod]
    public void DisablingDoubleDashAndOptionalBooleanValuesRetainDeclaredBehavior() {
        var parser = new ParserBuilder<AppOptions>(new ParserSettings { DoubleDashStopsParsing = false })
            .AddFlag(options => options.Verbose, "verbose")
            .AddOption(options => options.Enabled, "enabled", argumentType: ArgumentType.Optional)
            .AddArguments(options => options.Arguments)
            .Build();
        Assert.IsFalse(parser.TryParse(new[] { "--", "--verbose" }, out _, out var error));
        Assert.AreEqual(ParseErrorKind.UnknownOption, error!.Kind);
        var result = parser.Parse(new[] { "--verbose", "--enabled" });
        Assert.IsTrue(result.Verbose);
        Assert.IsTrue(result.Enabled);
        Assert.IsFalse(parser.Parse(new[] { "--enabled=false" }).Enabled);
    }

    /// <summary>
    /// Checks scalar and custom conversions while the current culture uses different numeric conventions.
    /// </summary>
    [TestMethod]
    public void BuiltInTypesAndCustomConvertersUseInvariantCulture() {
        var previousCulture = CultureInfo.CurrentCulture;
        try {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            var result = CommandLine.Parse<ConversionOptions>(new[] {
                "--amount=12.5", "--ratio=1.25", "--id=35a4c48e-1c58-440c-8035-c6a367acfe3e", "--date=2026-10-12",
                "--instant=2026-10-12T12:00:00+02:00", "--duration=01:30:00", "--custom=12"
            });
            Assert.AreEqual(12.5m, result.Amount);
            Assert.AreEqual(1.25, result.Ratio);
            Assert.AreEqual(Guid.Parse("35a4c48e-1c58-440c-8035-c6a367acfe3e"), result.Id);
            Assert.AreEqual(new DateTime(2026, 10, 12), result.Date);
            Assert.AreEqual(TimeSpan.FromHours(2), result.Instant.Offset);
            Assert.AreEqual(TimeSpan.FromMinutes(90), result.Duration);
            Assert.AreEqual(12, result.Custom!.Value);
        } finally {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    /// <summary>
    /// Compares typed and legacy consumption with POSIXLY_CORRECT set, restoring the process environment afterwards.
    /// </summary>
    [TestMethod]
    [DoNotParallelize]
    public void PosixEnvironmentRetainsExistingEngineConsumption() {
        var previous = Environment.GetEnvironmentVariable("POSIXLY_CORRECT");
        try {
            Environment.SetEnvironmentVariable("POSIXLY_CORRECT", "1");
            var engine = new GetOpt(new[] { "-ovalue", "--output", "second", "--verbose" }, "-o:v",
                new Option("output", ArgumentType.Required, 'o'), new Option("verbose", ArgumentType.None, 'v'));
            string? expected = null;
            var verbose = false;
            int option;
            while ((option = engine.GetNextOpt(out var value)) != -1) {
                if (option == 'o') { expected = value; }
                if (option == 'v') { verbose = true; }
            }

            var result = FluentParser().Parse(new[] { "-ovalue", "--output", "second", "--verbose" });
            Assert.AreEqual(expected, result.Output);
            Assert.AreEqual(verbose, result.Verbose);
        } finally {
            Environment.SetEnvironmentVariable("POSIXLY_CORRECT", previous);
        }
    }

    /// <summary>
    /// Provides equivalent attributed and fluent bindings for global options, operands and a configuration command.
    /// </summary>
    public class AppOptions {

        /// <summary>
        /// Gets or sets the flag used to verify boolean inference and grouped short options.
        /// </summary>
        [CliOption("verbose", ShortName = 'v', Description = "Verbose output.")]
        public bool Verbose { get; set; }

        /// <summary>
        /// Gets or sets a string with a nonempty initializer used to check overwrite and default behavior.
        /// </summary>
        [CliOption("output", ShortName = 'o', Description = "Output file.")]
        public string Output { get; set; } = "output.txt";

        /// <summary>
        /// Gets or sets an initialized integer used for conversion and overflow scenarios.
        /// </summary>
        [CliOption("retries", ShortName = 'r')]
        public int Retries { get; set; } = 3;

        /// <summary>
        /// Gets or sets an optional value whose initializer survives an omitted argument.
        /// </summary>
        [CliOption("format", ShortName = 'f', ArgumentType = ArgumentType.Optional)]
        public string Format { get; set; } = "json";

        /// <summary>
        /// Gets or sets array values appended after an initialized default element.
        /// </summary>
        [CliOption("tag", ShortName = 't')]
        public string[] Tags { get; set; } = new[] { "default" };

        /// <summary>
        /// Gets or sets list values appended after an initialized default element.
        /// </summary>
        [CliOption("number")]
        public List<int> Numbers { get; set; } = new() { 1 };

        /// <summary>
        /// Gets or sets a boolean requiring an explicit value despite boolean flag inference.
        /// </summary>
        [CliOption("enabled", ArgumentType = ArgumentType.Required)]
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a nullable integer used to distinguish empty input from an absent option.
        /// </summary>
        [CliOption("count")]
        public int? Count { get; set; } = 9;

        /// <summary>
        /// Gets or sets an enum used to check case-insensitive value conversion.
        /// </summary>
        [CliOption("mode")]
        public OutputMode Mode { get; set; }

        /// <summary>
        /// Gets or sets operands captured in the global scope.
        /// </summary>
        [CliArguments]
        public string[] Arguments { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the command model, initially null to represent no selection.
        /// </summary>
        [CliCommand("config", Description = "Manage configuration.")]
        public ConfigOptions? Config { get; set; }

    }

    /// <summary>
    /// Provides command-owned flags, values and operand capture.
    /// </summary>
    public class ConfigOptions {

        /// <summary>
        /// Gets or sets the command flag used to verify scope selection.
        /// </summary>
        [CliOption("create", ShortName = 'c', Description = "Create configuration.")]
        public bool Create { get; set; }

        /// <summary>
        /// Gets or sets a command-local value whose alias is also valid in the global scope.
        /// </summary>
        [CliOption("output", ShortName = 'o')]
        public string Output { get; set; } = "child.txt";

        /// <summary>
        /// Gets or sets operands captured after command selection.
        /// </summary>
        [CliArguments]
        public string[] Arguments { get; set; } = Array.Empty<string>();

    }

    /// <summary>
    /// Provides named enum values for scalar and repeated-value conversion tests.
    /// </summary>
    public enum OutputMode {

        /// <summary>
        /// The default enum value used before an option is supplied.
        /// </summary>
        Normal,

        /// <summary>
        /// An alternate value used to verify case-insensitive parsing.
        /// </summary>
        Fast

    }

    /// <summary>
    /// Provides an inherited required option and an unannotated property for mixed configuration tests.
    /// </summary>
    public class RequiredOptions {

        /// <summary>
        /// Gets or sets the option whose presence is required even when a default exists.
        /// </summary>
        [CliOption("name", Required = true)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a flag bound only by fluent configuration.
        /// </summary>
        public bool Verbose { get; set; }

    }

    /// <summary>
    /// Provides a property type with no supported string conversion.
    /// </summary>
    public class InvalidOptions {

        /// <summary>
        /// Gets or sets the unsupported object-valued binding.
        /// </summary>
        public object Data { get; set; } = new();

    }

    /// <summary>
    /// Provides an attributed property without a setter to test schema rejection.
    /// </summary>
    public class ReadOnlyOptions {

        /// <summary>
        /// Gets a value that cannot receive parsed arguments.
        /// </summary>
        [CliOption("value")]
        public int Value { get; }

    }

    /// <summary>
    /// Provides a nonpublic attributed property that must not be silently ignored.
    /// </summary>
    public class PrivateOptions {

        /// <summary>
        /// Gets or sets an inaccessible binding used to test visibility validation.
        /// </summary>
        [CliOption("value")]
        private int Value { get; set; }

    }

    /// <summary>
    /// Provides a static attributed property to test rejection of shared model state.
    /// </summary>
    public class StaticOptions {

        /// <summary>
        /// Gets or sets the invalid static binding.
        /// </summary>
        [CliOption("value")]
        public static int Value { get; set; }

    }

    /// <summary>
    /// Provides an init-only property that cannot be bound after construction.
    /// </summary>
    public class InitOnlyOptions {

        /// <summary>
        /// Gets or initializes the invalid init-only binding.
        /// </summary>
        [CliOption("value")]
        public int Value { get; init; }

    }

    /// <summary>
    /// Combines inherited attributes with nullable and enum collection bindings.
    /// </summary>
    public class DerivedOptions: RequiredOptions {

        /// <summary>
        /// Gets or sets nullable integer occurrences, including explicitly empty values.
        /// </summary>
        [CliOption("values")]
        public int?[] Values { get; set; } = Array.Empty<int?>();

        /// <summary>
        /// Gets or sets repeated case-insensitive enum values.
        /// </summary>
        [CliOption("modes")]
        public List<OutputMode> Modes { get; set; } = new();

    }

    /// <summary>
    /// Provides separate model instances whose initialized lists refer to the same object.
    /// </summary>
    public class SharedDefaults {

        /// <summary>
        /// Gets the shared initializer list which parsing must leave unchanged.
        /// </summary>
        public static List<int> Values { get; } = new() { 1 };

        /// <summary>
        /// Gets or sets the collection whose appended values must be isolated from shared defaults.
        /// </summary>
        [CliOption("number")]
        public List<int> Numbers { get; set; } = Values;

    }

    /// <summary>
    /// Provides a recursive command declaration to test the single-level command restriction.
    /// </summary>
    public class NestedCommands {

        /// <summary>
        /// Gets or sets the invalid nested command declaration.
        /// </summary>
        [CliCommand("nested")]
        public NestedCommands? Nested { get; set; }

    }

    /// <summary>
    /// Provides an already populated command property to test runtime model validation.
    /// </summary>
    public class InitializedCommand {

        /// <summary>
        /// Gets or sets the invalid non-null command initializer.
        /// </summary>
        [CliCommand("config")]
        public ConfigOptions Config { get; set; } = new();

    }

    /// <summary>
    /// Provides an application setter failure that must escape TryParse.
    /// </summary>
    public class ThrowingSetter {

        /// <summary>
        /// Gets an empty value and throws whenever binding attempts to assign it.
        /// </summary>
        [CliOption("value")]
        public string Value {
            get => string.Empty;
            set => throw new InvalidOperationException("Application failure.");
        }

    }

    /// <summary>
    /// Provides a construction failure that must escape TryParse.
    /// </summary>
    public class ThrowingConstructor {

        /// <summary>
        /// Throws an application error to distinguish model failures from invalid CLI input.
        /// </summary>
        public ThrowingConstructor() {
            throw new InvalidOperationException("Application failure.");
        }

    }

    /// <summary>
    /// Provides built-in scalar types and a custom converter for culture-independent conversion tests.
    /// </summary>
    public class ConversionOptions {

        /// <summary>
        /// Gets or sets a decimal value parsed with invariant punctuation.
        /// </summary>
        [CliOption("amount")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets a floating-point value parsed with invariant punctuation.
        /// </summary>
        [CliOption("ratio")]
        public double Ratio { get; set; }

        /// <summary>
        /// Gets or sets a GUID parsed from its textual representation.
        /// </summary>
        [CliOption("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets an invariantly parsed calendar date.
        /// </summary>
        [CliOption("date")]
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets an instant with an explicitly supplied UTC offset.
        /// </summary>
        [CliOption("instant")]
        public DateTimeOffset Instant { get; set; }

        /// <summary>
        /// Gets or sets a duration parsed from hours, minutes and seconds.
        /// </summary>
        [CliOption("duration")]
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Gets or sets the value produced by the custom type converter.
        /// </summary>
        [CliOption("custom")]
        public CustomValue? Custom { get; set; }

    }

    /// <summary>
    /// Wraps an integer to exercise attribute-discovered custom string conversion.
    /// </summary>
    [TypeConverter(typeof(CustomValueConverter))]
    public class CustomValue {

        /// <summary>
        /// Gets or sets the converted integer payload.
        /// </summary>
        public int Value { get; set; }

    }

    /// <summary>
    /// Converts string input into the custom fixture type using the supplied culture.
    /// </summary>
    public class CustomValueConverter: TypeConverter {

        /// <summary>
        /// Reports support only for string input.
        /// </summary>
        /// <param name="context">Unused component conversion context.</param>
        /// <param name="sourceType">The proposed input type.</param>
        /// <returns>True only for string sources.</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) {
            return sourceType == typeof(string);
        }

        /// <summary>
        /// Parses the custom integer payload using the culture passed by the binder.
        /// </summary>
        /// <param name="context">Unused component conversion context.</param>
        /// <param name="culture">The culture supplied by the parser.</param>
        /// <param name="value">The string containing the integer payload.</param>
        /// <returns>A new custom value containing the parsed integer.</returns>
        public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) {
            return new CustomValue { Value = int.Parse((string)value, culture) };
        }

    }

}
