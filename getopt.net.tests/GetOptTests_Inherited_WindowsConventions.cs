using System;

namespace getopt.net.tests {

    [TestClass]
    public class GetOptTests_Inherited_Windows: GetOpt {

        public GetOptTests_Inherited_Windows() {
            AllowWindowsConventions = true;
            Options = new[] {
                new Option("windowsarg", ArgumentType.None, 'w'),
                new Option("AnotherWindowsArg", ArgumentType.None, 'A')
            };
        }

        [TestMethod]
        public void TestIsLongOption() {
            Assert.IsTrue(IsLongOption("/windowsarg"));
            Assert.IsTrue(IsLongOption("/AnotherWindowsArg"));
            Assert.IsFalse(IsLongOption("/a"));
            Assert.IsTrue(IsLongOption("--posix-like"));
        }

        [TestMethod]
        public void TestIsShortOption() {
            Assert.IsTrue(IsShortOption("/w"));
            Assert.IsTrue(IsShortOption("/W"));
            Assert.IsTrue(IsShortOption("/0"));
            Assert.IsFalse(IsShortOption(":m"));
            Assert.IsFalse(IsShortOption("O"));
            Assert.IsTrue(IsShortOption("-c"));
        }

        [TestMethod]
        public void TestStripDashes() {
            AppArgs = new[] { "no-dashes-or-slahes" };
            Assert.AreEqual("no-dashes-or-slahes", StripDashes(true));

            AppArgs = new[] { "-single-dash" };
            Assert.AreEqual("single-dash", StripDashes(true));

            AppArgs = new[] { "--double-dash" };
            Assert.AreEqual("double-dash", StripDashes(true));

            AppArgs = new[] { "-shortopt" };
            Assert.AreEqual("shortopt", StripDashes(false));

            AppArgs = new[] { "/single-slash" };
            Assert.AreEqual("single-slash", StripDashes(true));

            AppArgs = new[] { "/long-option" };
            Assert.AreEqual("long-option", StripDashes(true));
            
            AppArgs = new[] { "/shortopt" };
            Assert.AreEqual("shortopt", StripDashes(false));
        }

    }

}