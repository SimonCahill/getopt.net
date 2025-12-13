using System;

namespace getopt.net.tests {

    [TestClass]
    public class GetOptTests_RealWorld {

        [TestMethod]
        public void TestGetNextOptLong() {
            var opt = new GetOpt();
            opt.AppArgs = new[] { "--help" };
            opt.Options = new[] { new Option { Name = "help", ArgumentType = ArgumentType.None, Value = 'h' } };

            string? optArg = "";
            char optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('h', optChar);
            Assert.AreEqual(null, optArg);
        }

        [TestMethod]
        public void TestGetNextOptLongWithRequiredArg_SeperatedByEquals() {
            var opt = new GetOpt();
            opt.AppArgs = new[] { "--test=test" };
            opt.Options = new[] { new Option { Name = "test", ArgumentType = ArgumentType.Required, Value = 't' } };

            string? optArg = "";
            char optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('t', optChar);
            Assert.AreEqual("test", optArg);
        }

        [TestMethod]
        public void TestGetNextOptLongWithRequiredArgs_SeparatedBySpace() {
            var opt = new GetOpt();
            opt.AppArgs = new[] { "--test test" };
            opt.Options = new[] { new Option { Name = "test", ArgumentType = ArgumentType.Required, Value = 't' } };

            string? optArg = "";
            char optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('t', optChar);
            Assert.AreEqual("test", optArg);
        }

        [TestMethod]
        public void TestGetNextOptLongWithRequiredArgs_SeparatedByArg() {
            var opt = new GetOpt();
            opt.AppArgs = new[] { "--test", "test" };
            opt.Options = new[] { new Option { Name = "test", ArgumentType = ArgumentType.Required, Value = 't' } };

            string? optArg = "";
            char optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('t', optChar);
            Assert.AreEqual("test", optArg);
        }

        [TestMethod]
        public void TestGetNextOptLongWithMultipleArgs() {
            var opt = new GetOpt();
            opt.AppArgs = new[] { "--test", "test", "--test2", "--test3=test3", "--test4 test4" };
            opt.Options = new[] {
                new Option { Name = "test",     ArgumentType = ArgumentType.Required,   Value = 't' },
                new Option { Name = "test2",    ArgumentType = ArgumentType.None,       Value = '1' },
                new Option { Name = "test3",    ArgumentType = ArgumentType.Optional,   Value = '2' },
                new Option { Name = "test4",    ArgumentType = ArgumentType.Required,   Value = '3' }
            };

            char optChar = (char)opt.GetNextOpt(out string? optArg);
            Assert.AreEqual('t', optChar);
            Assert.AreEqual("test", optArg);

            optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('1', optChar);
            Assert.AreEqual(null, optArg);

            optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('2', optChar);
            Assert.AreEqual("test3", optArg);

            optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('3', optChar);
            Assert.AreEqual("test4", optArg);
        }

        [TestMethod]
        public void TestGetNextOptShort_WithoutLongOpts() {
            var opt = new GetOpt();
            opt.ShortOpts = "hc:vf;";
            opt.AppArgs = new[] { "-h", "-ctest", "-v", "-f", "-ftest", "-f", "test2" };

            var optChar = (char)opt.GetNextOpt(out var optArg);
            Assert.AreEqual('h', optChar);
            Assert.AreEqual(null, optArg);

            optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('c', optChar);
            Assert.AreEqual("test", optArg);

            optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('v', optChar);
            Assert.AreEqual(null, optArg);

            optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('f', optChar);
            Assert.AreEqual(null, optArg);

            optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('f', optChar);
            Assert.AreEqual("test", optArg);

            optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('f', optChar);
            Assert.AreEqual("test2", optArg);
        }

        [TestMethod]
        public void TestGetNextOptShort_EmptyOptionalAtEnd() {
            var opt = new GetOpt();
            opt.ShortOpts = "hc:vf;F;g;";
            opt.AppArgs = new[] { "-h", "-ctest", "-v", "-ftest", "-F", "test2", "-g" };

            var optChar = (char)opt.GetNextOpt(out var optArg);
            Assert.AreEqual('h', optChar);
            Assert.AreEqual(null, optArg);

            optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('c', optChar);
            Assert.AreEqual("test", optArg);

            optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('v', optChar);
            Assert.AreEqual(null, optArg);

            optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('f', optChar);
            Assert.AreEqual("test", optArg);

            optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('F', optChar);
            Assert.AreEqual("test2", optArg);

            optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('g', optChar);
            Assert.AreEqual(null, optArg);
        }

        [TestMethod]
        public void TestGetNextOptShort_WithFallbackToLongOpt() {
            var opt = new GetOpt();
            opt.Options = new[] {
                new Option { Name = "help",     ArgumentType = ArgumentType.None,           Value = 'h' },
                new Option { Name = "config",   ArgumentType = ArgumentType.Required,       Value = 'c' },
                new Option { Name = "version",  ArgumentType = ArgumentType.None,           Value = 'v' }
            };
            opt.ShortOpts = "hv"; // intentionally leaving out config to test fallbar to long opts
            opt.AppArgs = new[] { "-hv", "-ctest" };

            var optChar = (char)opt.GetNextOpt(out var optArg);
            Assert.AreEqual('h', optChar);
            Assert.IsNull(optArg);

            optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('v', optChar);
            Assert.IsNull(optArg);

            optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('c', optChar);
            Assert.AreEqual("test", optArg);
        }

        [TestMethod]
        public void TestGetNextOptShort_AllOptsInSameString() {
            var opt = new GetOpt();
            opt.Options = new[] {
                new Option { Name = "help",     ArgumentType = ArgumentType.None,           Value = 'h' },
                new Option { Name = "config",   ArgumentType = ArgumentType.Required,       Value = 'c' },
                new Option { Name = "version",  ArgumentType = ArgumentType.None,           Value = 'v' }
            };
            opt.ShortOpts = "hvc:"; // intentionally leaving out config to test fallbar to long opts
            opt.AppArgs = new[] { "-hvctest" };

            var optChar = (char)opt.GetNextOpt(out var optArg);
            Assert.AreEqual('h', optChar);
            Assert.IsNull(optArg);

            optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('v', optChar);
            Assert.IsNull(optArg);

            optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('c', optChar);
            Assert.AreEqual("test", optArg);
        }

        [TestMethod]
        public void TestGetNextOptShort_AllOptsInSameString_WithErrors_NoException() {
            var opt = new GetOpt();
            opt.ShortOpts = "t";
            opt.Options = Array.Empty<Option>();
            opt.AppArgs = new[] { "-te" };
            opt.IgnoreInvalidOptions = true;

            var optChar = (char)opt.GetNextOpt(out var optArg);
            Assert.AreEqual('t', optChar);
            Assert.IsNull(optArg);

            optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual(GetOpt.InvalidOptChar, optChar);
            Assert.IsNull(optArg);
        }

        [TestMethod]
        public void TestGetNextOptShort_MultipleOptsAndArgs() {
            var opt = new GetOpt();
            opt.Options = new[] {
                new Option { Name = "config",           ArgumentType = ArgumentType.Required,   Value = 'c' },
                new Option { Name = "log-lvl",          ArgumentType = ArgumentType.Required,   Value = 'L' },
                new Option { Name = "help",             ArgumentType = ArgumentType.None,       Value = 'h' },
                new Option { Name = "version",          ArgumentType = ArgumentType.None,       Value = 'v' },
                new Option { Name = "check-updates",    ArgumentType = ArgumentType.None,       Value = 'U' },
                new Option { Name = "reset-cfg",        ArgumentType = ArgumentType.Optional,   Value = '%' }
                // add more as required
            };
            opt.ShortOpts = "c:L:hv%U";
            opt.DoubleDashStopsParsing = true;
            opt.AppArgs = new[] { "-ctest.json", "-Ltrace" };

            var optChar = (char)opt.GetNextOpt(out var optArg);
            Assert.AreEqual('c', optChar);
            Assert.IsNotNull(optArg);
            Assert.AreEqual("test.json", optArg);

            optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('L', optChar);
            Assert.IsNotNull(optArg);
            Assert.AreEqual("trace", optArg);
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void TestFilenameOnly_ExpectException() {
            var opt = new GetOpt();
            opt.ShortOpts = string.Empty;
            opt.Options = Array.Empty<Option>();
            opt.AppArgs = new[] { "filename.txt" };
            opt.IgnoreInvalidOptions = false;

            opt.GetNextOpt(out var _); // Something expressing the existence of filename.txt should happen here.
        }

        [TestMethod]
        public void TestFilenameOnly_IgnoreInvalidOpts() {
            var opt = new GetOpt();
            opt.ShortOpts = string.Empty;
            opt.Options = Array.Empty<Option>();
            opt.AppArgs = new[] { "filename.txt" };
            string? optArg;
            Assert.AreEqual(GetOpt.InvalidOptChar, (char)opt.GetNextOpt(out optArg));
        }

        [TestMethod]
        public void TestFilenameWithPreceedingDashes() {
            var opt = new GetOpt();
            opt.ShortOpts = string.Empty;
            opt.Options = Array.Empty<Option>();
            opt.DoubleDashStopsParsing = true;
            opt.AppArgs = new[] { "--", "--filename.txt" };
            Assert.AreEqual(1, opt.GetNextOpt(out var _));
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void TestOptionBeforeFilename_ExpectException() {
            var opt = new GetOpt();
            opt.ShortOpts = "t";
            opt.Options = Array.Empty<Option>();
            opt.AppArgs = new[] { "-t", "filename.txt" };
            opt.IgnoreInvalidOptions = false;

            var optChar = (char)opt.GetNextOpt(out var optArg);
            Assert.AreEqual('t', optChar);
            Assert.IsNull(optArg);
            opt.GetNextOpt(out var _); // Something expressing the existence of filename.txt should happen here.
        }

        [TestMethod]
        public void TestOptionBeforeFilename_IgnoreInvalidOpts() {
            var opt = new GetOpt();
            opt.ShortOpts = "t";
            opt.Options = Array.Empty<Option>();
            opt.AppArgs = new[] { "-t", "filename.txt" };
            opt.IgnoreInvalidOptions = true;

            var optChar = (char)opt.GetNextOpt(out var optArg);
            Assert.AreEqual('t', optChar);
            Assert.IsNull(optArg);
            optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual(GetOpt.InvalidOptChar, optChar);
            Assert.IsNotNull(optArg);
            Assert.AreEqual("filename.txt", optArg);
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void TestFilenameBeforeOptionGnuParsing_ExpectException() {
            var opt = new GetOpt();
            opt.ShortOpts = "t";
            opt.Options = Array.Empty<Option>();
            opt.AppArgs = new[] { "filename.txt", "-t" };
            opt.IgnoreInvalidOptions = false;

            var optChar = (char)opt.GetNextOpt(out var optArg);
            Assert.AreEqual('t', optChar);
            Assert.IsNull(optArg);
            opt.GetNextOpt(out var _); // Something expressing the existence of filename.txt should happen here.
        }

        [TestMethod]
        public void TestFilenameBeforeOptionGnuInOrderParsing() {
            var opt = new GetOpt();
            opt.ShortOpts = "-t";
            opt.Options = Array.Empty<Option>();
            opt.AppArgs = new[] { "filename.txt", "-t" };
            opt.IgnoreInvalidOptions = true;

            var optChar = (char)opt.GetNextOpt(out var optArg);
            Assert.AreEqual(GetOpt.NonOptChar, optChar);
            Assert.IsNotNull(optArg);
            Assert.AreEqual("filename.txt", optArg);

            optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('t', optChar);
            Assert.IsNull(optArg);
        }

        [TestMethod]
        public void TestFilenameBeforeOptionPosixParsing() {
            var opt = new GetOpt();
            opt.ShortOpts = "t";
            opt.Options = Array.Empty<Option>();
            opt.AppArgs = new[] { "filename.txt", "-t" };
            opt.IgnoreInvalidOptions = true;

            var optChar = (char)opt.GetNextOpt(out var optArg);
            Assert.AreEqual(GetOpt.InvalidOptChar, optChar);
            Assert.IsNotNull(optArg);
            Assert.AreEqual("filename.txt", optArg);

            optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('t', optChar);
            Assert.IsNull(optArg);
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void TestFilenameBeforeOptionPosixParsing_ExpectException() {
            var opt = new GetOpt();
            opt.ShortOpts = "t";
            opt.Options = Array.Empty<Option>();
            opt.AppArgs = new[] { "filename.txt", "-t" };
            opt.IgnoreInvalidOptions = false;

            var optChar = (char)opt.GetNextOpt(out var optArg);
            optChar = (char)opt.GetNextOpt(out optArg);
        }

        [TestMethod]
        public void TestDoubleDashStopsParsing_True() {
            var opt = new GetOpt {
                AppArgs = new[] { "-hc", "-v", "--", "--test", "-xzf" },
                DoubleDashStopsParsing = true, // this is true by default
                Options = new Option[] {
                    new Option("help", ArgumentType.None, 'h'),
                    new Option("config", ArgumentType.Optional, 'c'),
                    new Option("verbose", ArgumentType.None, 'v'),
                    new Option("test", ArgumentType.None, 't'),
                    new Option("extract", ArgumentType.None, 'x'),
                    new Option("zip", ArgumentType.None, 'z'),
                    new Option("find", ArgumentType.None, 'f')
                }
            };

            var optChar = (char)opt.GetNextOpt(out var optArg);
            Assert.AreEqual('h', optChar);
            Assert.IsNull(optArg);

            optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('c', optChar);
            Assert.IsNull(optArg);

            optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('v', optChar);
            Assert.IsNull(optArg);

            // At this point, "--" should be encountered.
            // This is ignored and the next option is returned via optArg.
            // optChar should == 1

            optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual(GetOpt.NonOptChar, optChar);
            Assert.IsNotNull(optArg);
            Assert.AreEqual("--test", optArg);

            optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual(GetOpt.NonOptChar, optChar);
            Assert.IsNotNull(optArg);
            Assert.AreEqual("-xzf", optArg);
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void TestDoubleDashStopsParsing_and_IgnoreInvalidOptions_False() {
            var opt = new GetOpt {
                AppArgs = new[] { "-hc", "-v", "--", "--test", "-xzf" },
                DoubleDashStopsParsing = false, // this is true by default
                IgnoreInvalidOptions = false, // this is default true
                Options = new Option[] {
                    new Option("help", ArgumentType.None, 'h'),
                    new Option("config", ArgumentType.Optional, 'c'),
                    new Option("verbose", ArgumentType.None, 'v'),
                    new Option("test", ArgumentType.None, 't'),
                    new Option("extract", ArgumentType.None, 'x'),
                    new Option("zip", ArgumentType.None, 'z'),
                    new Option("find", ArgumentType.None, 'f')
                }
            };

            var optChar = (char)opt.GetNextOpt(out var optArg);
            Assert.AreEqual('h', optChar);
            Assert.IsNull(optArg);

            optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('c', optChar);
            Assert.IsNull(optArg);

            optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('v', optChar);
            Assert.IsNull(optArg);

            // At this point, "--" should be encountered.
            // This is will throw an exception if "IgnoreInvalidOptions" is false
            optChar = (char)opt.GetNextOpt(out optArg);
        }

        [TestMethod]
        public void TestDoubleDashStopsParsing_False() {
            var opt = new GetOpt {
                AppArgs = new[] { "-hc", "-v", "--", "--test", "-xzf" },
                DoubleDashStopsParsing = false, // this is true by default
                Options = new Option[] {
                    new Option("help", ArgumentType.None, 'h'),
                    new Option("config", ArgumentType.Optional, 'c'),
                    new Option("verbose", ArgumentType.None, 'v'),
                    new Option("test", ArgumentType.None, 't'),
                    new Option("extract", ArgumentType.None, 'x'),
                    new Option("zip", ArgumentType.None, 'z'),
                    new Option("find", ArgumentType.None, 'f')
                }
            };

            var optChar = (char)opt.GetNextOpt(out var optArg);
            Assert.AreEqual('h', optChar);
            Assert.IsNull(optArg);

            optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('c', optChar);
            Assert.IsNull(optArg);

            optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('v', optChar);
            Assert.IsNull(optArg);

            // At this point, "--" should be encountered.
            // This is an invalid option and GetNextOpt should
            // return '?'

            optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual(GetOpt.InvalidOptChar, optChar);
            Assert.IsNotNull(optArg);
            Assert.AreEqual("--", optArg);

            optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('t', optChar);
            Assert.IsNull(optArg);

            optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('x', optChar);
            Assert.IsNull(optArg);

            optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('z', optChar);
            Assert.IsNull(optArg);

            optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('f', optChar);
            Assert.IsNull(optArg);
        }

        [TestMethod]
        public void TestPowershellConvention_LongOptions() {
            var getopt = new GetOpt {
                Options = new[] {
                    new Option("test", ArgumentType.None, '1'),
                    new Option("test2", ArgumentType.Optional, '2'),
                    new Option("test3", ArgumentType.Required, '3')
                },
                AllowPowershellConventions = true,
                AppArgs = new[] { "-test", "-1", "-test2", "-2", "-test2:arg", "-test2=arg", "-test2 arg", "-test2", "arg", "-test3:arg", "-test3=arg", "-test3 arg", "-test3", "arg", "-12", "-3", "arg" }
            };

            const string testArg = "arg";

            var optChar = (char)getopt.GetNextOpt(out var optArg);
            Assert.AreEqual('1', optChar);
            Assert.IsNull(optArg);

            optChar = (char)getopt.GetNextOpt(out optArg);
            Assert.AreEqual('1', optChar);
            Assert.IsNull(optArg);

            optChar = (char)getopt.GetNextOpt(out optArg);
            Assert.AreEqual('2', optChar);
            Assert.IsNull(optArg);

            optChar = (char)getopt.GetNextOpt(out optArg);
            Assert.AreEqual('2', optChar);
            Assert.IsNull(optArg);

            getopt.AllowWindowsConventions = true;
            optChar = (char)getopt.GetNextOpt(out optArg);
            Assert.AreEqual('2', optChar);
            Assert.IsNotNull(optArg);
            Assert.AreEqual(testArg, optArg);


            optChar = (char)getopt.GetNextOpt(out optArg);
            Assert.AreEqual('2', optChar);
            Assert.IsNotNull(optArg);
            Assert.AreEqual(testArg, optArg);

            optChar = (char)getopt.GetNextOpt(out optArg);
            Assert.AreEqual('2', optChar);
            Assert.IsNotNull(optArg);
            Assert.AreEqual(testArg, optArg);

            optChar = (char)getopt.GetNextOpt(out optArg);
            Assert.AreEqual('2', optChar);
            Assert.IsNotNull(optArg);
            Assert.AreEqual(testArg, optArg);

            optChar = (char)getopt.GetNextOpt(out optArg);
            Assert.AreEqual('3', optChar);
            Assert.IsNotNull(optArg);
            Assert.AreEqual(testArg, optArg);

            optChar = (char)getopt.GetNextOpt(out optArg);
            Assert.AreEqual('3', optChar);
            Assert.IsNotNull(optArg);
            Assert.AreEqual(testArg, optArg);

            optChar = (char)getopt.GetNextOpt(out optArg);
            Assert.AreEqual('3', optChar);
            Assert.IsNotNull(optArg);
            Assert.AreEqual(testArg, optArg);

            optChar = (char)getopt.GetNextOpt(out optArg);
            Assert.AreEqual('3', optChar);
            Assert.IsNotNull(optArg);
            Assert.AreEqual(testArg, optArg);

            optChar = (char)getopt.GetNextOpt(out optArg);
            Assert.AreEqual('1', optChar);
            Assert.IsNull(optArg);

            optChar = (char)getopt.GetNextOpt(out optArg);
            Assert.AreEqual('2', optChar);
            Assert.IsNull(optArg);

            optChar = (char)getopt.GetNextOpt(out optArg);
            Assert.AreEqual('3', optChar);
            Assert.IsNotNull(optArg);
            Assert.AreEqual(testArg, optArg);
        }

        [TestMethod]
        public void TestOptionsWithLongerIntValues() {
            var getopt = new GetOpt {
                AppArgs = new[] { "--long-one", "--long-two" },
                Options = new[] {
                    new Option("long-one", ArgumentType.None, 0xbada55),
                    new Option("long-two", ArgumentType.None, 0xdeada55)
                }
            };

            var optChar = getopt.GetNextOpt(out var optArg);
            Assert.AreEqual(0xbada55, optChar);
            Assert.IsNull(optArg);

            optChar = getopt.GetNextOpt(out optArg);
            Assert.AreEqual(0xdeada55, optChar);
            Assert.IsNull(optArg);
        }

        [TestMethod]
        public void TestConvertLongOptsToShortOpts() {
            var longOpts = new Option[] {
                new Option("version", ArgumentType.None, 'v'),
                new Option("help", ArgumentType.None, 'h'),
                new Option("config", ArgumentType.Required, 'c'),
                new Option("working-dir", ArgumentType.Optional, 'w'),
                new Option("console", ArgumentType.None, 'C')
            };

            var shortOpts = longOpts.ToShortOptString();

            Assert.IsNotNull(shortOpts);
            Assert.IsFalse(shortOpts?.Length == 0);
            Assert.IsTrue(shortOpts?.Length == 7);
            Assert.AreEqual(shortOpts, "vhc:w;C");
        }

        [TestMethod]
        public void TestConvertLongOptsToShortOptsWithFailure() {
            var emptyLongOpts = new Option[] {};
            var nullLongOpts = default(Option[]);

            Assert.IsNotNull(Extensions.ToShortOptString(emptyLongOpts));
            Assert.IsTrue(string.IsNullOrEmpty(Extensions.ToShortOptString(emptyLongOpts)));
            Assert.IsNotNull(Extensions.ToShortOptString(nullLongOpts!));
            Assert.IsTrue(string.IsNullOrEmpty(Extensions.ToShortOptString(emptyLongOpts)));
        }

        [TestMethod]
        public void TestSupportForNonOptions() {
            var getopt = new GetOpt {
                AppArgs = new[] { "filename.txt", "--long-one", "--long-two" },
                Options = new[] {
                    new Option("long-one", ArgumentType.None, '1'),
                    new Option("long-two", ArgumentType.None, '2')
                },
                ShortOpts = "-12"
            };

            var optChar = getopt.GetNextOpt(out var optArg);
            Assert.AreEqual(GetOpt.NonOptChar, optChar);
            Assert.AreEqual("filename.txt", optArg);

            optChar = getopt.GetNextOpt(out optArg);
            Assert.AreEqual('1', optChar);
            Assert.IsNull(optArg);

            optChar = getopt.GetNextOpt(out optArg);
            Assert.AreEqual('2', optChar);
            Assert.IsNull(optArg);
        }

        [TestMethod]
        public void TestParseAllOptions_OnlyValidOptionsAndHandlers() {
            var getopt = new GetOpt {
                AppArgs = new[] { "--long-one", "--long-two" },
                Options = new[] {
                    new Option("long-one", ArgumentType.None, '1') {
                        Handler = (e) => {
                            Assert.AreEqual('1', e.Option.Value);
                            Assert.IsNull(e.Argument);
                        }
                    },
                    new Option("long-two", ArgumentType.None, '2') {
                        Handler = (e) => {
                            Assert.AreEqual('2', e.Option.Value);
                            Assert.IsNull(e.Argument);
                        }
                    }
                }
            };
        }

        [TestMethod]
        public void TestParseAllOptions_WithNonOptionHandler_DoNotIgnoreInvalidOpts() {
            var getopt = new GetOpt {
                AppArgs = new[] { "filename.txt", "--long-one", "--long-two" },
                Options = new[] {
                    new Option("long-one", ArgumentType.None, '1') {
                        Handler = (e) => {
                            Assert.AreEqual('1', e.Option.Value);
                            Assert.IsNull(e.Argument);
                        }
                    },
                    new Option("long-two", ArgumentType.None, '2') {
                        Handler = (e) => {
                            Assert.AreEqual('2', e.Option.Value);
                            Assert.IsNull(e.Argument);
                        }
                    }
                },
                NonOptionHandler = (e) => {
                    Assert.AreEqual(GetOpt.InvalidOptChar, e.Option.Value);
                    Assert.AreEqual("filename.txt", e.Argument);
                }
            };

            var allParsed = getopt.ParseAllOptions();
            Assert.IsTrue(allParsed);
        }

        [TestMethod]
        public void TestParseAllOptions_WithNonOptionHandler_IgnoreInvalidOpts() {
            var getopt = new GetOpt {
                IgnoreInvalidOptions = true,
                AppArgs = new[] { "filename.txt", "--long-one", "--long-two" },
                Options = new[] {
                    new Option("long-one", ArgumentType.None, '1') {
                        Handler = (e) => {
                            Assert.AreEqual('1', e.Option.Value);
                            Assert.IsNull(e.Argument);
                        }
                    },
                    new Option("long-two", ArgumentType.None, '2') {
                        Handler = (e) => {
                            Assert.AreEqual('2', e.Option.Value);
                            Assert.IsNull(e.Argument);
                        }
                    }
                },
                NonOptionHandler = (e) => {
                    Assert.AreEqual(GetOpt.NonOptChar, e.Option.Value);
                    Assert.AreEqual("filename.txt", e.Argument);
                },
                ShortOpts = "-12"
            };

            var allParsed = getopt.ParseAllOptions();
            Assert.IsTrue(allParsed);
        }

        [TestMethod]
        public void ParseAllOptions_AllValidOptionsAndHandlers() {
            var calls = new Dictionary<int, string?>();

            var options = new[] {
                new Option("alpha",  ArgumentType.None,     'a') { Handler = e => { calls.Add(e.Option.Value, e.Argument); } },
                new Option("beta",   ArgumentType.Required, 'b') { Handler = e => { calls.Add(e.Option.Value, e.Argument); } },
                new Option("gamma",  ArgumentType.Optional, 'g') { Handler = e => { calls.Add(e.Option.Value, e.Argument); } }
            };

            var opt = new GetOpt(new[] { "-a", "-b", "val", "-gopt" }, options.ToShortOptString(), options);

            var result = opt.ParseAllOptions();

            Assert.IsTrue(result);
            Assert.IsTrue(calls.ContainsKey('a'));
            Assert.IsNull(calls['a']);
            Assert.IsTrue(calls.ContainsKey('b'));
            Assert.IsNotNull(calls['b']);
            Assert.AreEqual("val", calls['b']);
            Assert.IsNotNull(calls['g']);
            Assert.AreEqual("opt", calls['g']);

            Assert.AreEqual(3, calls.Count);
        }

        [TestMethod]
        public void ParseAllOptions_InvokesNonOptionHandler_ForNonOption() {
            var optionCalls = new Dictionary<int, string?>();
            var nonOptionCalls = new Dictionary<int, string?>();

            var options = new[] {
                new Option("alpha", ArgumentType.None, 'a') { Handler = e => optionCalls.Add(e.Option.Value, e.Argument) }
            };

            var opt = new GetOpt(new[] { "file.txt", "-a" }, options) {
                NonOptionHandler = e => nonOptionCalls.Add(e.Option.Value, e.Argument)
            };

            var result = opt.ParseAllOptions();

            Assert.IsTrue(result);
            Assert.AreEqual(1, optionCalls.Count);
            Assert.IsNull(optionCalls[0]);

            Assert.AreEqual(1, nonOptionCalls.Count);
            // First item should be a non-option: when MustReturnChar1() is false, InvalidOptChar is used
            Assert.AreEqual(GetOpt.InvalidOptChar, nonOptionCalls.Keys.ElementAt(0));
            Assert.AreEqual("file.txt", nonOptionCalls[0]);
        }

        [TestMethod]
        public void ParseAllOptions_NonOptionHandler_StopsEarly() {
            var optionCalls = new Dictionary<int, string?>();
            var nonOptionCalls = new Dictionary<int, string?>();

            var options = new[] {
                new Option("alpha", ArgumentType.None, 'a') { Handler = e => optionCalls.Add(e.Option.Value, e.Argument) }
            };

            var opt = new GetOpt(new[] { "file.txt", "-a" }, options) {
                NonOptionHandler = e => {
                    nonOptionCalls.Add(e.Option.Value, e.Argument);
                    e.ContinueParsing = false;
                }
            };

            var result = opt.ParseAllOptions();

            Assert.IsFalse(result); // stopped before -1 reached
            Assert.AreEqual(0, optionCalls.Count); // 'a' not processed
            Assert.AreEqual(1, nonOptionCalls.Count);
            Assert.AreEqual("file.txt", nonOptionCalls[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void ParseAllOptions_InvalidOption_NoNonOptionHandler_Throws_WhenNotIgnoring() {
            var options = new[] {
                new Option("alpha", ArgumentType.None, 'a') { Handler = _ => { } }
            };

            var opt = new GetOpt(new[] { "-x" }, options) {
                IgnoreInvalidOptions = false
            };

            opt.ParseAllOptions();
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void ParseAllOptions_MissingHandler_Throws() {
            var options = new[] {
                new Option("alpha", ArgumentType.None, 'a') // no handler assigned
            };

            var opt = new GetOpt(new[] { "-a" }, options);

            opt.ParseAllOptions();
        }

        [TestMethod]
        public void ParseAllOptions_NoNonOptionHandler_IgnoreInvalidOptionsTrue_SkipsNonOptions() {
            var optionCalls = new Dictionary<int, string?>();

            var options = new[] {
                new Option("alpha", ArgumentType.None, 'a') { Handler = e => optionCalls.Add(e.Option.Value, e.Argument) }
            };

            var opt = new GetOpt(new[] { "file.txt", "-a" }, options) {
                IgnoreInvalidOptions = true,
                NonOptionHandler = null
            };

            var result = opt.ParseAllOptions();

            Assert.IsTrue(result);
            Assert.AreEqual(1, optionCalls.Count);
            Assert.IsNull(optionCalls[0]);
        }

        [TestMethod]
        public void ParseAllOptions_DoubleDashStopsParsing_NonOptionHandlerReceivesRest() {
            var optionCalls = new Dictionary<int, string?>();
            var nonOptionCalls = new Dictionary<int, string?>();

            var options = new[] {
                new Option("alpha", ArgumentType.None, 'a') { Handler = e => optionCalls.Add(e.Option.Value, e.Argument) }
            };

            var opt = new GetOpt(new[] { "-a", "--", "file.txt", "-xzf" }, options) {
                DoubleDashStopsParsing = true,
                NonOptionHandler = e => nonOptionCalls.Add(e.Option.Value, e.Argument)
            };

            var result = opt.ParseAllOptions();

            Assert.IsTrue(result);
            Assert.AreEqual(1, optionCalls.Count);
            Assert.IsNull(optionCalls[0]);

            Assert.AreEqual(2, nonOptionCalls.Count);
            Assert.AreEqual(GetOpt.NonOptChar, nonOptionCalls.Keys.ElementAt(0));
            Assert.AreEqual("file.txt", nonOptionCalls[0]);
            Assert.AreEqual(GetOpt.NonOptChar, nonOptionCalls.Keys.ElementAt(1));
            Assert.AreEqual("-xzf", nonOptionCalls[1]);
        }

        [TestMethod]
        public void ParseAllOptions_RequiredAndOptionalArgs_PassedToHandlers() {
            var calls = new Dictionary<int, string?>();

            var options = new[] {
                new Option("required", ArgumentType.Required, 'r') { Handler = e => calls.Add(e.Option.Value, e.Argument) },
                new Option("optional", ArgumentType.Optional, 'o') { Handler = e => calls.Add(e.Option.Value, e.Argument) }
            };

            var opt = new GetOpt(new[] { "-r", "data", "-oextra" }, options);

            var result = opt.ParseAllOptions();

            Assert.IsTrue(result);
            CollectionAssert.Contains(calls, new KeyValuePair<int, string?>('r', "data"));
            CollectionAssert.Contains(calls, new KeyValuePair<int, string?>('o', "extra"));
            Assert.AreEqual(2, calls.Count);
        }

    }
}
