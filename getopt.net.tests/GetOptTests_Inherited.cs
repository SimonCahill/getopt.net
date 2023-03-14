﻿namespace getopt.net.tests { 

    [TestClass]
    public class GetOptTests_Inherited: GetOpt {

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

            Assert.IsTrue(IsShortOption(validArg1));
            Assert.IsTrue(IsShortOption(validArg2));
            Assert.IsTrue(IsShortOption(validArg3));

            Assert.IsFalse(IsShortOption(invalidArg1));
            Assert.IsFalse(IsShortOption(invalidArg2));
            Assert.IsFalse(IsShortOption(invalidArg3));
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

            Assert.IsTrue(IsLongOption(validArg1));
            Assert.IsTrue(IsLongOption(validArg2));
            Assert.IsTrue(IsLongOption(validArg3));

            Assert.IsFalse(IsLongOption(invalidArg1));
            Assert.IsFalse(IsLongOption(invalidArg2));
            Assert.IsFalse(IsLongOption(invalidArg3));
            Assert.IsFalse(IsLongOption(invalidArg4));
        }

        [TestMethod]
        public void TestStripDashes() {
            AppArgs = new[] { "nodashes" };
            Assert.AreEqual("nodashes", StripDashes(true));

            AppArgs = new[] { "-single-dash" };
            Assert.AreEqual("single-dash", StripDashes(true));

            AppArgs = new[] { "--double-dash" };
            Assert.AreEqual("double-dash", StripDashes(true));
            AppArgs = new[] { "-shortopt" };
            Assert.AreEqual("shortopt", StripDashes(false));
        }



        [TestMethod]
        public void TestHasArgumentInOption_HasArgs() {
            m_currentIndex = 0;
            AppArgs = new[] {
                // GNU/POSIX style
                "--has arg", "--has=arg"
            };

            Assert.IsTrue(HasArgumentInOption(out var optName, out var optVal));
            Assert.IsNotNull(optName);
            Assert.AreEqual("--has", optName);
            Assert.IsNotNull(optVal);
            Assert.AreEqual("arg", optVal);

            m_currentIndex++; // manually increment counter

            Assert.IsTrue(HasArgumentInOption(out optName, out optVal));
            Assert.IsNotNull(optName);
            Assert.AreEqual("--has", optName);
            Assert.IsNotNull(optVal);
            Assert.AreEqual("arg", optVal);
        }

        [TestMethod]
        public void TestMustStopParsing() {
            m_currentIndex = 0;
            ShortOpts = "+abcde:";
            AppArgs = new[] { "-abcdeTEST" };

            Assert.IsTrue(MustStopParsing());
        }

        [TestMethod]
        public void TestMustReturnChar1() {
            m_currentIndex = 0;
            ShortOpts = "-abcde:";
            AppArgs = new[] { "-abcdeTEST" };

            Assert.IsTrue(MustReturnChar1());
        }

        [TestMethod]
        public void TestArgumentSplitter() {
            const string TestString1 = "--test bla";
            const string TestString2 = "-test bla";
            const string TestString3 = "test bla";
            const string TestString4 = "/test bla";
            const string TestString5 = "--test=bla";
            const string TestString6 = "-test=bla";
            const string TestString7 = "test=bla";
            const string TestString8 = "/test=bla";

            Assert.IsTrue(ArgumentSplitter().IsMatch(TestString1));
            Assert.IsTrue(ArgumentSplitter().IsMatch(TestString2));
            Assert.IsTrue(ArgumentSplitter().IsMatch(TestString3));
            Assert.IsTrue(ArgumentSplitter().IsMatch(TestString4));
            Assert.IsTrue(ArgumentSplitter().IsMatch(TestString5));
            Assert.IsTrue(ArgumentSplitter().IsMatch(TestString6));
            Assert.IsTrue(ArgumentSplitter().IsMatch(TestString7));
            Assert.IsTrue(ArgumentSplitter().IsMatch(TestString8));

            Assert.IsTrue(ArgumentSplitter().Matches(TestString1).Count == 1);
            Assert.IsTrue(ArgumentSplitter().Matches(TestString2).Count == 1);
            Assert.IsTrue(ArgumentSplitter().Matches(TestString3).Count == 1);
            Assert.IsTrue(ArgumentSplitter().Matches(TestString4).Count == 1);
            Assert.IsTrue(ArgumentSplitter().Matches(TestString5).Count == 1);
            Assert.IsTrue(ArgumentSplitter().Matches(TestString6).Count == 1);
            Assert.IsTrue(ArgumentSplitter().Matches(TestString7).Count == 1);
            Assert.IsTrue(ArgumentSplitter().Matches(TestString8).Count == 1);

            Assert.AreEqual(3, ArgumentSplitter().Split(TestString1).Length);
            Assert.AreEqual(3, ArgumentSplitter().Split(TestString2).Length);
            Assert.AreEqual(3, ArgumentSplitter().Split(TestString3).Length);
            Assert.AreEqual(3, ArgumentSplitter().Split(TestString4).Length);
            Assert.AreEqual(3, ArgumentSplitter().Split(TestString5).Length);
            Assert.AreEqual(3, ArgumentSplitter().Split(TestString6).Length);
            Assert.AreEqual(3, ArgumentSplitter().Split(TestString7).Length);
            Assert.AreEqual(3, ArgumentSplitter().Split(TestString8).Length);

            Assert.AreEqual(2, ArgumentSplitter().Split(TestString1).Where(x => !string.IsNullOrEmpty(x) && !string.IsNullOrWhiteSpace(x) && x != "=").Count());
            Assert.AreEqual(2, ArgumentSplitter().Split(TestString2).Where(x => !string.IsNullOrEmpty(x) && !string.IsNullOrWhiteSpace(x) && x != "=").Count());
            Assert.AreEqual(2, ArgumentSplitter().Split(TestString3).Where(x => !string.IsNullOrEmpty(x) && !string.IsNullOrWhiteSpace(x) && x != "=").Count());
            Assert.AreEqual(2, ArgumentSplitter().Split(TestString4).Where(x => !string.IsNullOrEmpty(x) && !string.IsNullOrWhiteSpace(x) && x != "=").Count());
            Assert.AreEqual(2, ArgumentSplitter().Split(TestString5).Where(x => !string.IsNullOrEmpty(x) && !string.IsNullOrWhiteSpace(x) && x != "=").Count());
            Assert.AreEqual(2, ArgumentSplitter().Split(TestString6).Where(x => !string.IsNullOrEmpty(x) && !string.IsNullOrWhiteSpace(x) && x != "=").Count());
            Assert.AreEqual(2, ArgumentSplitter().Split(TestString7).Where(x => !string.IsNullOrEmpty(x) && !string.IsNullOrWhiteSpace(x) && x != "=").Count());
            Assert.AreEqual(2, ArgumentSplitter().Split(TestString8).Where(x => !string.IsNullOrEmpty(x) && !string.IsNullOrWhiteSpace(x) && x != "=").Count());
        }

    }

}