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

        public void TestHelpGeneration() {
            var getopt = new GetOpt {
                Options = new Option[] {
                    new Option("help", ArgumentType.None, 'h', "Displays this help text."),
                    new Option("version", ArgumentType.None, 'v', "Displays the version of this program.")
                }
            };

            var helpText = getopt.GenerateHelpText();
            Console.WriteLine(helpText);
            Assert.IsNotNull(helpText);
            Assert.IsTrue(helpText.Contains("help"));
            Assert.IsTrue(helpText.Contains("version"));
        }

    }

}