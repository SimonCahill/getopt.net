using System;

namespace getopt.net.tests {

    [TestClass]
    public class ExtensionsTests {

        [TestMethod]
        public void TestLeftAlignString_WhitespacePadding() {
            const string testString = "This is left-aligned";
            const string desiredString = $"{testString}            ";

            Assert.AreEqual(testString.AlignLeft(desiredString.Length), desiredString);
        }

        [TestMethod]
        public void TestLeftAlignString_PlusSignPadding() {
            const string testString = "This is left-aligned";
            const string desiredString = $"{testString}++++++++++++";

            Assert.AreEqual(testString.AlignLeft(desiredString.Length, '+'), desiredString);
        }

        [TestMethod]
        public void TestLeftAlignString_UmlautPadding() {
            const string testString = "This is left-aligned";
            const string desiredString = $"{testString}üüüüüüüüüüüü";

            Assert.AreEqual(testString.AlignLeft(desiredString.Length, 'ü'), desiredString);
        }

        [TestMethod]
        public void TestRightAlignString_WhitespacePadding() {
            const string testString = "This is right-aligned";
            const string desiredString = $"            {testString}";

            Assert.AreEqual(testString.AlignRight(desiredString.Length), desiredString);
        }

        [TestMethod]
        public void TestRightAlignString_PlusSignPadding() {
            const string testString = "This is right-aligned";
            const string desiredString = $"++++++++++++{testString}";

            Assert.AreEqual(testString.AlignRight(desiredString.Length, '+'), desiredString);
        }

        [TestMethod]
        public void TestRightAlignString_UmlautPadding() {
            const string testString = "This is right-aligned";
            const string desiredString = $"üüüüüüüüüüüü{testString}";

            Assert.AreEqual(testString.AlignRight(desiredString.Length, 'ü'), desiredString);
        }

        [TestMethod]
        public void TestCentreAlignString_WhitespacePadding() {
            const string testString = "This is centre-aligned";
            const string desiredString = $"      {testString}      ";

            Assert.AreEqual(testString.AlignCentre(desiredString.Length), desiredString);
        }

        [TestMethod]
        public void TestCentreAlignString_PlusSignPadding() {
            const string testString = "This is centre-aligned";
            const string desiredString = $"++++++{testString}++++++";

            Assert.AreEqual(testString.AlignCentre(desiredString.Length, '+'), desiredString);
        }

        [TestMethod]
        public void TestCentreAlignString_UmlautPadding() {
            const string testString = "This is centre-aligned";
            const string desiredString = $"üüüüüü{testString}üüüüüü";

            Assert.AreEqual(testString.AlignCentre(desiredString.Length, 'ü'), desiredString);
        }

    }
}

