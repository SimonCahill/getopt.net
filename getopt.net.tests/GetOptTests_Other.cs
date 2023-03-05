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

    }

}