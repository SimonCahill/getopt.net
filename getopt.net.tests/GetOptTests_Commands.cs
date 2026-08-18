using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace getopt.net.tests {

    [TestClass]
    public class GetOptTests_Commands {

        private static MainCommand ConfigCommand => new(
            "config",
            "Manage application configuration.",
            "cp:f;",
            new Option("create", ArgumentType.None, 'c', "Create configuration."),
            new Option("purge", ArgumentType.None, 'p', "Purge configuration."),
            new Option("file", ArgumentType.Required, 'f', "Use a configuration file.")
        );

        [TestMethod]
        public void SelectsCommandAndParsesLongSubcommand() {
            var getopt = new GetOpt {
                AppArgs = new[] { "config", "--create" },
                Commands = new[] { ConfigCommand }
            };

            Assert.AreEqual('c', (char)getopt.GetNextOpt(out var argument));
            Assert.IsNull(argument);
            Assert.AreEqual("config", getopt.SelectedCommand?.Name);
            Assert.AreEqual(-1, getopt.GetNextOpt(out _));
        }

        [TestMethod]
        public void ParsesShortSubcommandAndRequiredArgument() {
            var getopt = new GetOpt {
                AppArgs = new[] { "config", "-f", "settings.json" },
                Commands = new[] { ConfigCommand }
            };

            Assert.AreEqual('f', (char)getopt.GetNextOpt(out var argument));
            Assert.AreEqual("settings.json", argument);
        }

        [TestMethod]
        public void ParsesOptionalSubcommandArgument() {
            var getopt = new GetOpt {
                AppArgs = new[] { "config", "--format=json" },
                Commands = new[] {
                    new MainCommand(
                        "config",
                        options: new[] { new Option("format", ArgumentType.Optional, 'F') }
                    )
                }
            };

            Assert.AreEqual('F', (char)getopt.GetNextOpt(out var argument));
            Assert.AreEqual("json", argument);
        }

        [TestMethod]
        public void ParsesGlobalOptionsOnlyBeforeCommand() {
            var getopt = new GetOpt {
                AppArgs = new[] { "--verbose", "config", "--create", "--verbose" },
                Options = new[] { new Option("verbose", ArgumentType.None, 'v') },
                Commands = new[] { ConfigCommand }
            };

            Assert.AreEqual('v', (char)getopt.GetNextOpt(out _));
            Assert.AreEqual('c', (char)getopt.GetNextOpt(out _));
            Assert.AreEqual(GetOpt.InvalidOptChar, (char)getopt.GetNextOpt(out var invalidOption));
            Assert.AreEqual("verbose", invalidOption);
        }

        [TestMethod]
        public void CommandWithoutOptionsIsSelectedAndReturnsEndOfInput() {
            var getopt = new GetOpt {
                AppArgs = new[] { "status" },
                Commands = new[] { new MainCommand("status", "Show status.") }
            };

            Assert.AreEqual(-1, getopt.GetNextOpt(out var argument));
            Assert.IsNull(argument);
            Assert.AreEqual("status", getopt.SelectedCommand?.Name);
        }

        [TestMethod]
        public void ConfiguredCommandsDoNotRequireACommand() {
            var getopt = new GetOpt {
                AppArgs = Array.Empty<string>(),
                Commands = new[] { ConfigCommand }
            };

            Assert.AreEqual(-1, getopt.GetNextOpt(out _));
            Assert.IsNull(getopt.SelectedCommand);
        }

        [TestMethod]
        public void UnknownCommandReturnsInvalidSentinelWhenIgnored() {
            var getopt = new GetOpt {
                AppArgs = new[] { "unknown" },
                Commands = new[] { ConfigCommand },
                IgnoreInvalidOptions = true
            };

            Assert.AreEqual(GetOpt.InvalidOptChar, (char)getopt.GetNextOpt(out var argument));
            Assert.AreEqual("unknown", argument);
        }

        [TestMethod]
        public void MisspeltCommandThrowsWithNearestSuggestions() {
            var getopt = new GetOpt {
                AppArgs = new[] { "cot" },
                Commands = new[] { new MainCommand("cat"), new MainCommand("cut"), new MainCommand("status") },
                IgnoreInvalidOptions = false
            };

            var exception = Assert.ThrowsException<CommandMisspeltException>(() => getopt.GetNextOpt(out _));
            CollectionAssert.AreEqual(new[] { "cat", "cut" }, exception.PossibleCommands.ToArray());
            Assert.AreEqual("cot", exception.Command);
        }

        [TestMethod]
        public void TransposedCommandIsReportedAsMisspelt() {
            var getopt = new GetOpt {
                AppArgs = new[] { "conifg" },
                Commands = new[] { ConfigCommand },
                IgnoreInvalidOptions = false
            };

            var exception = Assert.ThrowsException<CommandMisspeltException>(() => getopt.GetNextOpt(out _));
            CollectionAssert.AreEqual(new[] { "config" }, exception.PossibleCommands.ToArray());
        }

        [TestMethod]
        public void UnrelatedCommandThrowsNotFound() {
            var getopt = new GetOpt {
                AppArgs = new[] { "xyz" },
                Commands = new[] { ConfigCommand },
                IgnoreInvalidOptions = false
            };

            var exception = Assert.ThrowsException<CommandNotFoundException>(() => getopt.GetNextOpt(out _));
            Assert.AreEqual(0, exception.PossibleCommands.Count);
            Assert.AreEqual("xyz", exception.Command);
        }

        [TestMethod]
        public void CaseInsensitiveMatchingAppliesToCommandsAndLongOptions() {
            var getopt = new GetOpt {
                AppArgs = new[] { "--VERBOSE", "CONFIG", "--CREATE" },
                Options = new[] { new Option("verbose", ArgumentType.None, 'v') },
                Commands = new[] { ConfigCommand },
                CaseInsensitiveMatching = true
            };

            Assert.AreEqual('v', (char)getopt.GetNextOpt(out _));
            Assert.AreEqual('c', (char)getopt.GetNextOpt(out _));
            Assert.AreEqual("config", getopt.SelectedCommand?.Name);
        }

        [TestMethod]
        public void DefaultMatchingRemainsCaseSensitive() {
            var getopt = new GetOpt {
                AppArgs = new[] { "CONFIG" },
                Commands = new[] { ConfigCommand },
                IgnoreInvalidOptions = false
            };

            var exception = Assert.ThrowsException<CommandMisspeltException>(() => getopt.GetNextOpt(out _));
            CollectionAssert.AreEqual(new[] { "config" }, exception.PossibleCommands.ToArray());
        }

        [TestMethod]
        public void CaseInsensitiveMatchingIsCultureIndependent() {
            var previousCulture = CultureInfo.CurrentCulture;
            try {
                CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
                var getopt = new GetOpt {
                    AppArgs = new[] { "INSTALL", "--INPUT" },
                    Commands = new[] {
                        new MainCommand("install", options: new Option("input", ArgumentType.None, 'i'))
                    },
                    CaseInsensitiveMatching = true
                };

                Assert.AreEqual('i', (char)getopt.GetNextOpt(out _));
            } finally {
                CultureInfo.CurrentCulture = previousCulture;
            }
        }

        [TestMethod]
        public void ShortOptionsRemainCaseSensitive() {
            var getopt = new GetOpt {
                AppArgs = new[] { "config", "-C" },
                Commands = new[] { ConfigCommand },
                CaseInsensitiveMatching = true
            };

            Assert.AreEqual(GetOpt.InvalidOptChar, (char)getopt.GetNextOpt(out _));
        }

        [TestMethod]
        public void ParsesCommandOptionsUsingWindowsAndPowershellConventions() {
            var windows = new GetOpt {
                AppArgs = new[] { "config", "/create" },
                Commands = new[] { ConfigCommand },
                AllowWindowsConventions = true
            };
            var powershell = new GetOpt {
                AppArgs = new[] { "config", "-create" },
                Commands = new[] { ConfigCommand },
                AllowPowershellConventions = true
            };

            Assert.AreEqual('c', (char)windows.GetNextOpt(out _));
            Assert.AreEqual('c', (char)powershell.GetNextOpt(out _));
        }

        [TestMethod]
        public void ParsesCommandFromParameterFile() {
            var path = Path.GetTempFileName();
            try {
                File.WriteAllLines(path, new[] { "config", "--purge" });
                var getopt = new GetOpt {
                    AppArgs = new[] { $"@{path}" },
                    Commands = new[] { ConfigCommand },
                    AllowParamFiles = true
                };

                Assert.AreEqual('p', (char)getopt.GetNextOpt(out _));
                Assert.AreEqual("config", getopt.SelectedCommand?.Name);
            } finally {
                File.Delete(path);
            }
        }

        [TestMethod]
        public void HelpListsCommandsAndIndentedSubcommands() {
            var getopt = new GetOpt {
                Commands = new[] { ConfigCommand }
            };

            var help = getopt.GenerateHelpText(new HelpTextConfig {
                ApplicationName = "myapp",
                OptionConvention = OptionConvention.GnuPosix
            });

            StringAssert.Contains(help, "myapp [options] [command] [command options]");
            StringAssert.Contains(help, "Commands:");
            StringAssert.Contains(help, "    config Manage application configuration.");
            StringAssert.Contains(help, "        -c, --create");
            StringAssert.Contains(help, "        -p, --purge");
        }

        [TestMethod]
        public void HelpUsesConfiguredConventionForSubcommands() {
            var getopt = new GetOpt { Commands = new[] { ConfigCommand } };

            var help = getopt.GenerateHelpText(new HelpTextConfig {
                ApplicationName = "myapp",
                OptionConvention = OptionConvention.Windows
            });

            StringAssert.Contains(help, "        /c, /create");
        }

        [TestMethod]
        public void CommandHelpWrapsDescriptionsWithinConfiguredWidth() {
            var getopt = new GetOpt {
                Commands = new[] {
                    new MainCommand(
                        "config",
                        options: new[] {
                            new Option("create", ArgumentType.None, 'c', "Create a new application configuration with default values.")
                        }
                    )
                }
            };

            var help = getopt.GenerateHelpText(new HelpTextConfig { ApplicationName = "app", MaxWidth = 50 });
            Assert.IsTrue(help.Split('\n').All(line => line.TrimEnd('\r').Length <= 50));
        }

    }
}
