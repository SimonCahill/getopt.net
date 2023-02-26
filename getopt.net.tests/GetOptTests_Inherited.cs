namespace getopt.net.tests { 

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

    }

}