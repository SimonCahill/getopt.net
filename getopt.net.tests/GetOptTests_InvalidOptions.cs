namespace getopt.net.tests {

    [TestClass]
    public class GetOptTests_InvalidOptions {

        [TestMethod]
        public void TestNonOption_DoNotIgnoreInvalidOptions_ReturnsInvalidOptChar() {
            var getopt = new GetOpt {
                AppArgs = new[] { "filename.txt" },
                IgnoreInvalidOptions = false,
                ShortOpts = "k"
            };

            var optChar = getopt.GetNextOpt(out var optArg);

            Assert.AreEqual(GetOpt.InvalidOptChar, optChar);
            Assert.AreEqual("filename.txt", optArg);
        }

        [TestMethod]
        public void TestNonOption_InOrderParsing_DoNotIgnoreInvalidOptions_ReturnsNonOptChar() {
            var getopt = new GetOpt {
                AppArgs = new[] { "filename.txt" },
                IgnoreInvalidOptions = false,
                ShortOpts = "-k"
            };

            var optChar = getopt.GetNextOpt(out var optArg);

            Assert.AreEqual(GetOpt.NonOptChar, optChar);
            Assert.AreEqual("filename.txt", optArg);
        }

        [DataTestMethod]
        [DataRow("--invalid", false, false)]
        [DataRow("-i", false, false)]
        [DataRow("/invalid", true, false)]
        [DataRow("-invalid", false, true)]
        public void TestUnknownOption_DoNotIgnoreInvalidOptions_Throws(
            string appArg,
            bool allowWindowsConventions,
            bool allowPowershellConventions
        ) {
            var getopt = new GetOpt {
                AppArgs = new[] { appArg },
                AllowWindowsConventions = allowWindowsConventions,
                AllowPowershellConventions = allowPowershellConventions,
                IgnoreInvalidOptions = false,
                Options = new[] { new Option("known", ArgumentType.None, 'k') },
                ShortOpts = "k"
            };

            Assert.IsFalse(getopt.IgnoreInvalidOptions);
            Assert.ThrowsException<ParseException>(() => getopt.GetNextOpt(out var _));
        }

        [DataTestMethod]
        [DataRow("--invalid", false, false)]
        [DataRow("-i", false, false)]
        [DataRow("/invalid", true, false)]
        [DataRow("-invalid", false, true)]
        public void TestUnknownOption_IgnoreInvalidOptions_ReturnsInvalidOptChar(
            string appArg,
            bool allowWindowsConventions,
            bool allowPowershellConventions
        ) {
            var getopt = new GetOpt {
                AppArgs = new[] { appArg },
                AllowWindowsConventions = allowWindowsConventions,
                AllowPowershellConventions = allowPowershellConventions,
                IgnoreInvalidOptions = true,
                Options = new[] { new Option("known", ArgumentType.None, 'k') },
                ShortOpts = "k"
            };

            Assert.AreEqual(GetOpt.InvalidOptChar, getopt.GetNextOpt(out var _));
        }
    }
}
