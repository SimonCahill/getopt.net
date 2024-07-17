using System;

namespace getopt.net.tests {

    [TestClass]
    public class GetOptTests_Other {

        [TestMethod]
        public void TestAllExceptionsEnabled() {
            var getopt = new GetOpt {
                IgnoreEmptyAppArgs = false,
                IgnoreEmptyOptions = false,
                IgnoreInvalidOptions = false,
                IgnoreMissingArgument = false
            };

            Assert.IsFalse(getopt.AllExceptionsDisabled);
        }

        [TestMethod]
        public void TestDisableSomeExceptions() {
            var getopt = new GetOpt {
                IgnoreEmptyAppArgs = true,
                IgnoreEmptyOptions = false,
                IgnoreInvalidOptions = false,
                IgnoreMissingArgument = false
            };

            Assert.IsFalse(getopt.AllExceptionsDisabled);
        }

        [TestMethod]
        public void TestDisableAllExceptions() {
            var getopt = new GetOpt { };

            Assert.IsFalse(getopt.AllExceptionsDisabled);
            Assert.IsTrue(getopt.IgnoreEmptyAppArgs);
            Assert.IsTrue(getopt.IgnoreEmptyOptions);
            Assert.IsFalse(getopt.IgnoreMissingArgument);
            Assert.IsTrue(getopt.IgnoreInvalidOptions);

            getopt.AllExceptionsDisabled = true;
            Assert.IsTrue(getopt.IgnoreEmptyAppArgs);
            Assert.IsTrue(getopt.IgnoreEmptyOptions);
            Assert.IsTrue(getopt.IgnoreMissingArgument);
            Assert.IsTrue(getopt.IgnoreInvalidOptions);
        }

        [TestMethod]
        public void TestEnableAllExceptions() {
            var getopt = new GetOpt { };

            Assert.IsFalse(getopt.AllExceptionsDisabled);
            Assert.IsTrue(getopt.IgnoreEmptyAppArgs);
            Assert.IsTrue(getopt.IgnoreEmptyOptions);
            Assert.IsFalse(getopt.IgnoreMissingArgument);
            Assert.IsTrue(getopt.IgnoreInvalidOptions);

            getopt.AllExceptionsDisabled = false;
            Assert.IsFalse(getopt.IgnoreEmptyAppArgs);
            Assert.IsFalse(getopt.IgnoreEmptyOptions);
            Assert.IsFalse(getopt.IgnoreMissingArgument);
            Assert.IsFalse(getopt.IgnoreInvalidOptions);
        }

        [TestMethod]
        public void TestHelpGeneration() {
            var getopt = new GetOpt {
                Options = new Option[] {
                    new Option("help", ArgumentType.None, 'h', "Displays this help text."),
                    new Option("version", ArgumentType.None, 'v', "Displays the version of this program.")
                }
            };

            var helpText = getopt.GenerateHelpText();
            Assert.IsNotNull(helpText);
            Assert.IsTrue(helpText.Contains("help"));
            Assert.IsTrue(helpText.Contains("version"));
        }

        [TestMethod]
        public void TestHelpGeneration_WithConfig() {
            var getopt = new GetOpt {
                Options = new Option[] {
                    new Option("help", ArgumentType.None, 'h', "Displays this help text."),
                    new Option("version", ArgumentType.None, 'v', "Displays the version of this program.")
                }
            };

            var helpText = getopt.GenerateHelpText(new HelpTextConfig {
                ApplicationName = "getopt.net reference",
                ApplicationVersion = "1.0.0",
                FooterText = "This is a footer text."
            });

            Assert.IsNotNull(helpText);
            Assert.IsTrue(helpText.Contains("help"));
            Assert.IsTrue(helpText.Contains("version"));
            Assert.IsTrue(helpText.Contains("getopt.net reference"));
            Assert.IsTrue(helpText.Contains("1.0.0"));
            Assert.IsTrue(helpText.Contains("This is a footer text."));
        }

        [TestMethod]
        public void TestHelpGeneration_WithCopyright() {
            var getopt = new GetOpt {
                Options = new Option[] {
                    new Option("help", ArgumentType.None, 'h', "Displays this help text."),
                    new Option("version", ArgumentType.None, 'v', "Displays the version of this program.")
                }
            };

            var helpText = getopt.GenerateHelpText(new HelpTextConfig {
                ApplicationName = "getopt.net reference",
                ApplicationVersion = "1.0.0",
                FooterText = "This is a footer text.",
                CopyrightDate = new DateTime(2021, 1, 1),
                CopyrightHolder = "Simon Cahill"
            });

            Assert.IsNotNull(helpText);
            Assert.IsTrue(helpText.Contains("help"));
            Assert.IsTrue(helpText.Contains("version"));
            Assert.IsTrue(helpText.Contains("getopt.net reference"));
            Assert.IsTrue(helpText.Contains("1.0.0"));
            Assert.IsTrue(helpText.Contains("This is a footer text."));
            Assert.IsTrue(helpText.Contains("©"));
            Assert.IsTrue(helpText.Contains("2021"));
            Assert.IsTrue(helpText.Contains("Simon Cahill"));
        }

        [TestMethod]
        public void TestHelpGeneration_WithConfig_WithCopyright_WithWindowsConvention() {
            var getopt = new GetOpt {
                Options = new Option[] {
                    new Option("help", ArgumentType.None, 'h', "Displays this help text."),
                    new Option("version", ArgumentType.None, 'v', "Displays the version of this program.")
                }
            };

            var helpText = getopt.GenerateHelpText(new HelpTextConfig {
                ApplicationName = "getopt.net reference",
                ApplicationVersion = "1.0.0",
                FooterText = "This is a footer text.",
                OptionConvention = OptionConvention.Windows,
                ShowSupportedConventions = true,
                CopyrightDate = new DateTime(2021, 1, 1),
                CopyrightHolder = "Simon Cahill"
            });

            Assert.IsNotNull(helpText);
            Assert.IsTrue(helpText.Contains("/help"));
            Assert.IsTrue(helpText.Contains("/version"));
            Assert.IsTrue(helpText.Contains("/h"));
            Assert.IsTrue(helpText.Contains("/v"));
            Assert.IsTrue(helpText.Contains("/h, /help"));
            Assert.IsTrue(helpText.Contains("/v, /version"));
            Assert.IsTrue(helpText.Contains("getopt.net reference"));
            Assert.IsTrue(helpText.Contains("1.0.0"));
            Assert.IsTrue(helpText.Contains("This is a footer text."));
            Assert.IsTrue(helpText.Contains("©"));
            Assert.IsTrue(helpText.Contains("2021"));
            Assert.IsTrue(helpText.Contains("Simon Cahill"));
        }

        [TestMethod]
        public void TestHelpGeneration_WithConfig_WithCopyright_WithPowershellConvention() {
            var getopt = new GetOpt {
                Options = new Option[] {
                    new Option("help", ArgumentType.None, 'h', "Displays this help text."),
                    new Option("version", ArgumentType.None, 'v', "Displays the version of this program.")
                }
            };

            var helpText = getopt.GenerateHelpText(new HelpTextConfig {
                ApplicationName = "getopt.net reference",
                ApplicationVersion = "1.0.0",
                FooterText = "This is a footer text.",
                OptionConvention = OptionConvention.Powershell,
                ShowSupportedConventions = true,
                CopyrightDate = new DateTime(2021, 1, 1),
                CopyrightHolder = "Simon Cahill"
            });

            Assert.IsNotNull(helpText);
            Assert.IsTrue(helpText.Contains("-help"));
            Assert.IsTrue(helpText.Contains("-version"));
            Assert.IsTrue(helpText.Contains("-h"));
            Assert.IsTrue(helpText.Contains("-v"));
            Assert.IsTrue(helpText.Contains("-h, -help"));
            Assert.IsTrue(helpText.Contains("-v, -version"));
            Assert.IsTrue(helpText.Contains("getopt.net reference"));
            Assert.IsTrue(helpText.Contains("1.0.0"));
            Assert.IsTrue(helpText.Contains("This is a footer text."));
            Assert.IsTrue(helpText.Contains("©"));
            Assert.IsTrue(helpText.Contains("2021"));
            Assert.IsTrue(helpText.Contains("Simon Cahill"));
        }

    }

}