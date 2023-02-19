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
            Assert.AreEqual(StripDashes(true), "nodashes");

            AppArgs = new[] { "-single-dash" };
            Assert.AreEqual(StripDashes(true), "single-dash");

            AppArgs = new[] { "--double-dash" };
            Assert.AreEqual(StripDashes(true), "double-dash");
        }

        [TestMethod]
        public void TestGetNextOptLong() {
            AppArgs = new[] { "--help" };
            Options = new[] { new Option { Name = "help", ArgumentType = ArgumentType.None, Flag = IntPtr.Zero, Value = 'h' } };

            string? optArg = "";
            char optChar = (char)GetNextOpt(out optArg);
            Assert.AreEqual(optChar, 'h');
            Assert.AreEqual(optArg, null);
        }

        [TestMethod]
        public void TestGetNextOptLongWithRequiredArg_SeperatedByEquals() {
            AppArgs = new[] { "--test=test" };
            Options = new[] { new Option { Name = "test", ArgumentType = ArgumentType.Required, Flag = IntPtr.Zero, Value = 't' } };

            string? optArg = "";
            char optChar = (char)GetNextOpt(out optArg);
            Assert.AreEqual(optChar, 't');
            Assert.AreEqual(optArg, "test");
        }

        [TestMethod]
        public void TestGetNextOptLongWithRequiredArgs_SeparatedBySpace() {
            AppArgs = new[] { "--test test" };
            Options = new[] { new Option { Name = "test", ArgumentType = ArgumentType.Required, Flag = IntPtr.Zero, Value = 't' } };

            string? optArg = "";
            char optChar = (char)GetNextOpt(out optArg);
            Assert.AreEqual(optChar, 't');
            Assert.AreEqual(optArg, "test");
        }

        [TestMethod]
        public void TestGetNextOptLongWithRequiredArgs_SeparatedByArg() {
            AppArgs = new[] { "--test", "test" };
            Options = new[] { new Option { Name = "test", ArgumentType = ArgumentType.Required, Flag = IntPtr.Zero, Value = 't' } };

            string? optArg = "";
            char optChar = (char)GetNextOpt(out optArg);
            Assert.AreEqual(optChar, 't');
            Assert.AreEqual(optArg, "test");
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
            Assert.AreEqual(optChar, 't');
            Assert.AreEqual(optArg, "test");

            optChar = (char)GetNextOpt(out optArg);
            Assert.AreEqual(optChar, '1');
            Assert.AreEqual(optArg, null);

            optChar = (char)GetNextOpt(out optArg);
            Assert.AreEqual(optChar, '2');
            Assert.AreEqual(optArg, "test3");

            optChar = (char)GetNextOpt(out optArg);
            Assert.AreEqual(optChar, '3');
            Assert.AreEqual(optArg, "test4");
        }

        [TestMethod]
        public void TestGetNextOptShort_WithoutLongOpts() {
            ShortOpts = "hc:v";
            AppArgs = new[] { "-h", "-ctest", "-v" };

            var optChar = (char)GetNextOpt(out var optArg);
            Assert.AreEqual(optChar, 'h');
            Assert.AreEqual(optArg, null);

            optChar = (char)GetNextOpt(out optArg);
            Assert.AreEqual(optChar, 'c');
            Assert.AreEqual(optArg, "test");

            optChar = (char)GetNextOpt(out optArg);
            Assert.AreEqual(optChar, 'v');
            Assert.AreEqual(optArg, null);
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
            Assert.AreEqual(optChar, 'h');
            Assert.IsNull(optArg);

            optChar = (char)GetNextOpt(out optArg);
            Assert.AreEqual(optChar, 'v');
            Assert.IsNull(optArg);

            optChar = (char)GetNextOpt(out optArg);
            Assert.AreEqual(optChar, 'c');
            Assert.AreEqual(optArg, "test");
        }


    }

}