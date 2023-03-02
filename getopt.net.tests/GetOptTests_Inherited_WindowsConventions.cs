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

        [TestMethod]
        public void TestHasArgumentInOption_NoArgs() {
            m_currentIndex = 0; // ensure we always start at first index
            AppArgs = new[] {
                // no arguments here
                "/noarg", "-c", "--noarg"
            };

            Assert.IsFalse(HasArgumentInOption(out var optName, out var optVal));
            Assert.IsNotNull(optName);
            Assert.AreEqual("noarg", optName);
            Assert.IsNull(optVal);

            m_currentIndex++; // manually increment counter

            Assert.IsFalse(HasArgumentInOption(out optName, out optVal));
            Assert.IsNotNull(optName);
            Assert.AreEqual("c", optName);
            Assert.IsNull(optVal);

            m_currentIndex++;

            Assert.IsFalse(HasArgumentInOption(out optName, out optVal));
            Assert.IsNotNull(optName);
            Assert.AreEqual("noarg", optName);
            Assert.IsNull(optVal);

        }
        
        [TestMethod]
        public void TestHasArgumentInOption_WinStyleArgs() {
            m_currentIndex = 0;
            AppArgs = new[] {
                // Windows style
                "/has:arg", "/has arg", "/has=arg",
            };

            Assert.IsTrue(HasArgumentInOption(out var optName, out var optVal));
            Assert.IsNotNull(optName);
            Assert.AreEqual("/has", optName);
            Assert.IsNotNull(optVal);
            Assert.AreEqual("arg", optVal);

            m_currentIndex++; // manually increment counter

            Assert.IsTrue(HasArgumentInOption(out optName, out optVal));
            Assert.IsNotNull(optName);
            Assert.AreEqual("/has", optName);
            Assert.IsNotNull(optVal);
            Assert.AreEqual("arg", optVal);

            m_currentIndex++;

            Assert.IsTrue(HasArgumentInOption(out optName, out optVal));
            Assert.IsNotNull(optName);
            Assert.AreEqual("/has", optName);
            Assert.IsNotNull(optVal);
            Assert.AreEqual("arg", optVal);
        }

        [TestMethod]
        public void TestHasArgumentInOption_GnuPosixStyleArgs() {
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

    }

}