namespace getopt.net.tests { 

    [TestClass]
    public class GetOptTests: GetOpt {

        public static string ShortOpts1 = "hv";
        public static string ShortOpts2 = "hc:v";
        public static string ShortOpts3 = "hc:vL:";
        public static string ShortOpts4 = "h c:vL:";

        [TestMethod]
        public void TestIsShortOption() {
            var validArg1 = "-c";
            var validArg2 = "-c:";
            var validArg3 = "-c:L:v";

            var invalidArg1 = "--c";
            var invalidArg2 = "c:";
            var invalidArg3 = "--hvc:";

            Assert.IsTrue(IsShortOption(ref validArg1));
            Assert.IsTrue(IsShortOption(ref validArg2));
            Assert.IsTrue(IsShortOption(ref validArg3));

            Assert.IsFalse(IsShortOption(ref invalidArg1));
            Assert.IsFalse(IsShortOption(ref invalidArg2));
            Assert.IsFalse(IsShortOption(ref invalidArg3));
        }

        [TestMethod]
        public void TestIsLongOption() {
            var validArg1 = "--config";
            var validArg2 = "--config /path/to/config";
            var validArg3 = "--config=/path/to/config";

            var invalidArg1 = "-config";
            var invalidArg2 = "-c";
            var invalidArg3 = "config=test";
            var invalidArg4 = "config --test";

            Assert.IsTrue(IsLongOption(ref validArg1));
            Assert.IsTrue(IsLongOption(ref validArg2));
            Assert.IsTrue(IsLongOption(ref validArg3));

            Assert.IsFalse(IsLongOption(ref invalidArg1));
            Assert.IsFalse(IsLongOption(ref invalidArg2));
            Assert.IsFalse(IsLongOption(ref invalidArg3));
            Assert.IsFalse(IsLongOption(ref invalidArg4));
        }

        [TestMethod]
        public void TestStripDashes() {
            AppArgs = new[] { "nodashes" };
            Assert.AreEqual("nodashes", StripDashes(true));

            AppArgs = new[] { "-single-dash" };
            Assert.AreEqual("single-dash", StripDashes(true));

            AppArgs = new[] { "--double-dash" };
            Assert.AreEqual("double-dash", StripDashes(true));
        }

        [TestMethod]
        public void TestGetNextOptLong() {
            AppArgs = new[] { "--help" };
            Options = new[] { new Option { Name = "help", ArgumentType = ArgumentType.None, Flag = IntPtr.Zero, Value = 'h' } };

            string? optArg = "";
            char optChar = (char)GetNextOpt(out optArg);
            Assert.AreEqual('h', optChar);
            Assert.AreEqual(null, optArg);
        }

        [TestMethod]
        public void TestGetNextOptLongWithRequiredArg_SeperatedByEquals() {
            AppArgs = new[] { "--test=test" };
            Options = new[] { new Option { Name = "test", ArgumentType = ArgumentType.Required, Flag = IntPtr.Zero, Value = 't' } };

            string? optArg = "";
            char optChar = (char)GetNextOpt(out optArg);
            Assert.AreEqual('t', optChar);
            Assert.AreEqual("test", optArg);
        }

        [TestMethod]
        public void TestGetNextOptLongWithRequiredArgs_SeparatedBySpace() {
            AppArgs = new[] { "--test test" };
            Options = new[] { new Option { Name = "test", ArgumentType = ArgumentType.Required, Flag = IntPtr.Zero, Value = 't' } };

            string? optArg = "";
            char optChar = (char)GetNextOpt(out optArg);
            Assert.AreEqual('t', optChar);
            Assert.AreEqual("test", optArg);
        }

        [TestMethod]
        public void TestGetNextOptLongWithRequiredArgs_SeparatedByArg() {
            AppArgs = new[] { "--test", "test" };
            Options = new[] { new Option { Name = "test", ArgumentType = ArgumentType.Required, Flag = IntPtr.Zero, Value = 't' } };

            string? optArg = "";
            char optChar = (char)GetNextOpt(out optArg);
            Assert.AreEqual('t', optChar);
            Assert.AreEqual("test", optArg);
        }

        [TestMethod]
        public void TestGetNextOptLongWithMultipleArgs() {
            AppArgs = new[] { "--test", "test", "--test2", "--test3=test3", "--test4 test4" };
            Options = new[] {
                new Option { Name = "test",		ArgumentType = ArgumentType.Required,	Flag = IntPtr.Zero, Value = 't' },
                new Option { Name = "test2",	ArgumentType = ArgumentType.None,		Flag = IntPtr.Zero, Value = '1' },
                new Option { Name = "test3",	ArgumentType = ArgumentType.Optional,	Flag = IntPtr.Zero, Value = '2' },
                new Option { Name = "test4",	ArgumentType = ArgumentType.Required,	Flag = IntPtr.Zero, Value = '3' }
            };

            char optChar = (char)GetNextOpt(out string? optArg);
            Assert.AreEqual('t', optChar);
            Assert.AreEqual("test", optArg);

            optChar = (char)GetNextOpt(out optArg);
            Assert.AreEqual('1', optChar);
            Assert.AreEqual(null, optArg);

            optChar = (char)GetNextOpt(out optArg);
            Assert.AreEqual('2', optChar);
            Assert.AreEqual("test3", optArg);

            optChar = (char)GetNextOpt(out optArg);
            Assert.AreEqual('3', optChar);
            Assert.AreEqual("test4", optArg);
        }

        [TestMethod]
        public void TestGetNextOptShort_WithoutLongOpts() {
            ShortOpts = "hc:v";
            AppArgs = new[] { "-h", "-ctest", "-v" };

            var optChar = (char)GetNextOpt(out var optArg);
            Assert.AreEqual('h', optChar);
            Assert.AreEqual(null, optArg);

            optChar = (char)GetNextOpt(out optArg);
            Assert.AreEqual('c', optChar);
            Assert.AreEqual("test", optArg);

            optChar = (char)GetNextOpt(out optArg);
            Assert.AreEqual('v', optChar);
            Assert.AreEqual(null, optArg);
        }

        [TestMethod]
        public void TestGetNextOptShort_WithFallbackToLongOpt() {
            Options = new[] {
                new Option { Name = "help",     ArgumentType = ArgumentType.None,       Flag = IntPtr.Zero,     Value = 'h' },
                new Option { Name = "config",   ArgumentType = ArgumentType.Required,   Flag = IntPtr.Zero,     Value = 'c' },
                new Option { Name = "version",  ArgumentType = ArgumentType.None,       Flag = IntPtr.Zero,     Value = 'v' }
            };
            ShortOpts = "hv"; // intentionally leaving out config to test fallbar to long opts
            AppArgs = new[] { "-hv", "-ctest" };

            var optChar = (char)GetNextOpt(out var optArg);
            Assert.AreEqual('h', optChar);
            Assert.IsNull(optArg);

            optChar = (char)GetNextOpt(out optArg);
            Assert.AreEqual('v', optChar);
            Assert.IsNull(optArg);

            optChar = (char)GetNextOpt(out optArg);
            Assert.AreEqual('c', optChar);
            Assert.AreEqual("test", optArg);
        }

        [TestMethod]
        public void TestGetNextOptShort_AllOptsInSameString() {
            Options = new[] {
                new Option { Name = "help",     ArgumentType = ArgumentType.None,       Flag = IntPtr.Zero,     Value = 'h' },
                new Option { Name = "config",   ArgumentType = ArgumentType.Required,   Flag = IntPtr.Zero,     Value = 'c' },
                new Option { Name = "version",  ArgumentType = ArgumentType.None,       Flag = IntPtr.Zero,     Value = 'v' }
            };
            ShortOpts = "hvc:"; // intentionally leaving out config to test fallbar to long opts
            AppArgs = new[] { "-hvctest" };

            var optChar = (char)GetNextOpt(out var optArg);
            Assert.AreEqual('h', optChar);
            Assert.IsNull(optArg);

            optChar = (char)GetNextOpt(out optArg);
            Assert.AreEqual('v', optChar);
            Assert.IsNull(optArg);

            optChar = (char)GetNextOpt(out optArg);
            Assert.AreEqual('c', optChar);
            Assert.AreEqual("test", optArg);
        }

        [TestMethod]
        public void TestGetNextOptShort_MultipleOptsAndArgs() {
            Options = new[] {
                new Option { Name = "config",           ArgumentType = ArgumentType.Required,   Flag = IntPtr.Zero, Value = 'c' },
                new Option { Name = "log-lvl",          ArgumentType = ArgumentType.Required,   Flag = IntPtr.Zero, Value = 'L' },
                new Option { Name = "help",             ArgumentType = ArgumentType.None,       Flag = IntPtr.Zero, Value = 'h' },
                new Option { Name = "version",          ArgumentType = ArgumentType.None,       Flag = IntPtr.Zero, Value = 'v' },
                new Option { Name = "check-updates",    ArgumentType = ArgumentType.None,       Flag = IntPtr.Zero, Value = 'U' },
                new Option { Name = "reset-cfg",        ArgumentType = ArgumentType.Optional,   Flag = IntPtr.Zero, Value = '%' }
                // add more as required
            };
            ShortOpts = "c:L:hv%U";
            DoubleDashStopsParsing = true;

            AppArgs = new[] { "-ctest.json", "-Ltrace" };

            var optChar = (char)GetNextOpt(out var optArg);
            Assert.AreEqual('c', optChar);
            Assert.IsNotNull(optArg);
            Assert.AreEqual("test.json", optArg);

            optChar = (char)GetNextOpt(out optArg);
            Assert.AreEqual('L', optChar);
            Assert.IsNotNull(optArg);
            Assert.AreEqual("trace", optArg);
        }

		[TestMethod]
        [ExpectedException(typeof(ParseException))]
		public void TestFilenameOnly_ExpectException() {
			ShortOpts = string.Empty;
			Options = Array.Empty<Option>();
			AppArgs = new[] { "filename.txt" };
			GetNextOpt(out var _); // Something expressing the existence of filename.txt should happen here.
		}

		[TestMethod]
		public void TestFilenameOnly_IgnoreInvalidOpts() {
			ShortOpts = string.Empty;
			Options = Array.Empty<Option>();
			AppArgs = new[] { "filename.txt" };
            IgnoreInvalidOptions = true;
            string optArg;
			Assert.AreEqual('!', (char)GetNextOpt(out optArg));
		}

		[TestMethod]
		public void TestFilenameWithPreceedingDashes() {
			ShortOpts = string.Empty;
			Options = Array.Empty<Option>();
			DoubleDashStopsParsing = true;
			AppArgs = new[] { "--", "--filename.txt" };
			GetNextOpt(out var _); // Something expressing the existence of --filename.txt should happen here.
		}

		[TestMethod]
        [ExpectedException(typeof(ParseException))]
		public void TestOptionBeforeFilename_ExpectException() {
			ShortOpts = "t";
			Options = Array.Empty<Option>();
			AppArgs = new[] { "-t", "filename.txt" };
			var optChar = (char)GetNextOpt(out var optArg);
			Assert.AreEqual('t', optChar);
			Assert.IsNull(optArg);
			GetNextOpt(out var _); // Something expressing the existence of filename.txt should happen here.
		}

		[TestMethod]
		public void TestOptionBeforeFilename_IgnoreInvalidOpts() {
			ShortOpts = "t";
			Options = Array.Empty<Option>();
			AppArgs = new[] { "-t", "filename.txt" };
            IgnoreInvalidOptions = true;

			var optChar = (char)GetNextOpt(out var optArg);
			Assert.AreEqual('t', optChar);
			Assert.IsNull(optArg);
			GetNextOpt(out var _); // Something expressing the existence of filename.txt should happen here.
		}

		[TestMethod]
		public void TestFilenameBeforeOptionGnuParsing() {
			ShortOpts = "t";
			Options = Array.Empty<Option>();
			AppArgs = new[] { "filename.txt", "-t" };
			var optChar = (char)GetNextOpt(out var optArg);
			Assert.AreEqual('t', optChar);
			Assert.IsNull(optArg);
			GetNextOpt(out var _); // Something expressing the existence of filename.txt should happen here.
		}

		[TestMethod]
		public void TestFilenameBeforeOptionGnuInOrderParsing() {
			ShortOpts = "-t";
			Options = Array.Empty<Option>();
			AppArgs = new[] { "filename.txt", "-t" };
			var optChar = (char)GetNextOpt(out var optArg);
			Assert.AreEqual('\x01', optChar);
			Assert.IsNull("filename.txt");
			optChar = (char)GetNextOpt(out optArg);
			Assert.AreEqual('t', optChar);
			Assert.IsNull(optArg);
		}

		[TestMethod]
		public void TestFilenameBeforeOptionPosixParsing() {
			ShortOpts = "t";
			Options = Array.Empty<Option>();
			AppArgs = new[] { "filename.txt", "-t" };
			GetNextOpt(out var _); // Something expressing the existence of filename.txt should happen here.
			GetNextOpt(out var _); // Something expressing the existence of -t, as if it were a filename, should happen here.
		}

	}

}