using System;

namespace getopt.net.tests {

    [TestClass]
    public class GetOptTests_RealWorld {

        [TestMethod]
        public void TestGetNextOptLong() {
            var opt = new GetOpt();
            opt.AppArgs = new[] { "--help" };
            opt.Options = new[] { new Option { Name = "help", ArgumentType = ArgumentType.None, Flag = IntPtr.Zero, Value = 'h' } };

            string? optArg = "";
            char optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('h', optChar);
            Assert.AreEqual(null, optArg);
        }

        [TestMethod]
        public void TestGetNextOptLongWithRequiredArg_SeperatedByEquals() {
            var opt = new GetOpt();
            opt.AppArgs = new[] { "--test=test" };
            opt.Options = new[] { new Option { Name = "test", ArgumentType = ArgumentType.Required, Flag = IntPtr.Zero, Value = 't' } };

            string? optArg = "";
            char optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('t', optChar);
            Assert.AreEqual("test", optArg);
        }

        [TestMethod]
        public void TestGetNextOptLongWithRequiredArgs_SeparatedBySpace() {
            var opt = new GetOpt();
            opt.AppArgs = new[] { "--test test" };
            opt.Options = new[] { new Option { Name = "test", ArgumentType = ArgumentType.Required, Flag = IntPtr.Zero, Value = 't' } };

            string? optArg = "";
            char optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('t', optChar);
            Assert.AreEqual("test", optArg);
        }

        [TestMethod]
        public void TestGetNextOptLongWithRequiredArgs_SeparatedByArg() {
            var opt = new GetOpt();
            opt.AppArgs = new[] { "--test", "test" };
            opt.Options = new[] { new Option { Name = "test", ArgumentType = ArgumentType.Required, Flag = IntPtr.Zero, Value = 't' } };

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
                new Option { Name = "test",     ArgumentType = ArgumentType.Required,   Flag = IntPtr.Zero, Value = 't' },
                new Option { Name = "test2",    ArgumentType = ArgumentType.None,       Flag = IntPtr.Zero, Value = '1' },
                new Option { Name = "test3",    ArgumentType = ArgumentType.Optional,   Flag = IntPtr.Zero, Value = '2' },
                new Option { Name = "test4",    ArgumentType = ArgumentType.Required,   Flag = IntPtr.Zero, Value = '3' }
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
            opt.ShortOpts = "hc:v";
            opt.AppArgs = new[] { "-h", "-ctest", "-v" };

            var optChar = (char)opt.GetNextOpt(out var optArg);
            Assert.AreEqual('h', optChar);
            Assert.AreEqual(null, optArg);

            optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('c', optChar);
            Assert.AreEqual("test", optArg);

            optChar = (char)opt.GetNextOpt(out optArg);
            Assert.AreEqual('v', optChar);
            Assert.AreEqual(null, optArg);
        }

        [TestMethod]
        public void TestGetNextOptShort_WithFallbackToLongOpt() {
            var opt = new GetOpt();
            opt.Options = new[] {
                new Option { Name = "help",     ArgumentType = ArgumentType.None,       Flag = IntPtr.Zero,     Value = 'h' },
                new Option { Name = "config",   ArgumentType = ArgumentType.Required,   Flag = IntPtr.Zero,     Value = 'c' },
                new Option { Name = "version",  ArgumentType = ArgumentType.None,       Flag = IntPtr.Zero,     Value = 'v' }
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
                new Option { Name = "help",     ArgumentType = ArgumentType.None,       Flag = IntPtr.Zero,     Value = 'h' },
                new Option { Name = "config",   ArgumentType = ArgumentType.Required,   Flag = IntPtr.Zero,     Value = 'c' },
                new Option { Name = "version",  ArgumentType = ArgumentType.None,       Flag = IntPtr.Zero,     Value = 'v' }
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
                new Option { Name = "config",           ArgumentType = ArgumentType.Required,   Flag = IntPtr.Zero, Value = 'c' },
                new Option { Name = "log-lvl",          ArgumentType = ArgumentType.Required,   Flag = IntPtr.Zero, Value = 'L' },
                new Option { Name = "help",             ArgumentType = ArgumentType.None,       Flag = IntPtr.Zero, Value = 'h' },
                new Option { Name = "version",          ArgumentType = ArgumentType.None,       Flag = IntPtr.Zero, Value = 'v' },
                new Option { Name = "check-updates",    ArgumentType = ArgumentType.None,       Flag = IntPtr.Zero, Value = 'U' },
                new Option { Name = "reset-cfg",        ArgumentType = ArgumentType.Optional,   Flag = IntPtr.Zero, Value = '%' }
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
                AppArgs = new[] { "-hcv", "--", "--test", "-xzf" },
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
                AppArgs = new[] { "-hcv", "--", "--test", "-xzf" },
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
                AppArgs = new[] { "-hcv", "--", "--test", "-xzf" },
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

    }
}
