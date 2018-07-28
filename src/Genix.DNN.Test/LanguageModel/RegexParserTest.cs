namespace Genix.DNN.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RegexParserTest
    {
#if false
        [TestMethod]
        public void TryParseCharacterClassTest0()
        {
            int pos = 0;
            string characters;
            Assert.IsTrue(RegexParser.TryParseCharacterClass(CultureInfo.InvariantCulture, @"\s", ref pos, out characters));
            Assert.AreEqual(2, pos);
            Assert.AreEqual(" ", characters);

            pos = 0;
            Assert.IsTrue(RegexParser.TryParseCharacterClass(CultureInfo.InvariantCulture, @"\d", ref pos, out characters));
            Assert.AreEqual(2, pos);
            Assert.AreEqual("0123456789", characters);

            pos = 0;
            Assert.IsTrue(RegexParser.TryParseCharacterClass(CultureInfo.InvariantCulture, @"\x", ref pos, out characters));
            Assert.AreEqual(2, pos);
            Assert.AreEqual("0123456789ABCDEF", characters);

            pos = 0;
            Assert.IsTrue(RegexParser.TryParseCharacterClass(CultureInfo.InvariantCulture, @"\o", ref pos, out characters));
            Assert.AreEqual(2, pos);
            Assert.AreEqual("01234567", characters);

            pos = 0;
            Assert.IsTrue(RegexParser.TryParseCharacterClass(CultureInfo.InvariantCulture, @"\w", ref pos, out characters));
            Assert.AreEqual(2, pos);
            Assert.AreEqual("ABCDEFGHIJKLMNOPQRSTUVWXYZ", characters);

            pos = 0;
            Assert.IsTrue(RegexParser.TryParseCharacterClass(CultureInfo.GetCultureInfo("en-US"), @"\w", ref pos, out characters));
            Assert.AreEqual(2, pos);
            Assert.AreEqual("ABCDEFGHIJKLMNOPQRSTUVWXYZ", characters);

            pos = 0;
            Assert.IsTrue(RegexParser.TryParseCharacterClass(CultureInfo.GetCultureInfo("ru"), @"\w", ref pos, out characters));
            Assert.AreEqual(2, pos);
            Assert.AreEqual("АБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ", characters);
        }

        [TestMethod]
        public void TryParseCharacterClassTest1()
        {
            int pos = 0;
            string characters;
            Assert.IsFalse(RegexParser.TryParseCharacterClass(CultureInfo.InvariantCulture, null, ref pos, out characters));
            Assert.AreEqual(0, pos);

            Assert.IsFalse(RegexParser.TryParseCharacterClass(CultureInfo.InvariantCulture, string.Empty, ref pos, out characters));
            Assert.AreEqual(0, pos);

            Assert.IsFalse(RegexParser.TryParseCharacterClass(CultureInfo.InvariantCulture, "A", ref pos, out characters));
            Assert.AreEqual(0, pos);

            Assert.IsFalse(RegexParser.TryParseCharacterClass(CultureInfo.InvariantCulture, @"\S", ref pos, out characters));
            Assert.AreEqual(0, pos);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TryParseCharacterClassTest2()
        {
            try
            {
                int pos = 0;
                string characters;
                Assert.IsFalse(RegexParser.TryParseCharacterClass(CultureInfo.GetCultureInfo("es"), @"\w", ref pos, out characters));
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(
                    new ArgumentException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_InvalidRegex_UnsupportedCulture,
                        "es",
                         @"\w",
                        0)).Message,
                    e.Message);
                throw;
            }
        }

        [TestMethod]
        public void TryParseRangeTest0()
        {
            int pos = 0;
            string characters;
            Assert.IsTrue(RegexParser.TryParseRange("[A]", ref pos, out characters));
            Assert.AreEqual(3, pos);
            Assert.AreEqual("A", characters);

            pos = 0;
            Assert.IsTrue(RegexParser.TryParseRange("[A-C]", ref pos, out characters));
            Assert.AreEqual(5, pos);
            Assert.AreEqual("ABC", characters);

            pos = 0;
            Assert.IsTrue(RegexParser.TryParseRange("[0A-C9]", ref pos, out characters));
            Assert.AreEqual(7, pos);
            Assert.AreEqual("09ABC", characters);

            pos = 0;
            Assert.IsTrue(RegexParser.TryParseRange("[0A-]", ref pos, out characters));
            Assert.AreEqual(5, pos);
            Assert.AreEqual("-0A", characters);

            pos = 0;
            Assert.IsTrue(RegexParser.TryParseRange(@"[\]]", ref pos, out characters));
            Assert.AreEqual(4, pos);
            Assert.AreEqual("]", characters);
        }

        [TestMethod]
        public void TryParseRangeTest1()
        {
            int pos = 0;
            string characters;
            Assert.IsFalse(RegexParser.TryParseRange(null, ref pos, out characters));
            Assert.AreEqual(0, pos);

            Assert.IsFalse(RegexParser.TryParseRange(string.Empty, ref pos, out characters));
            Assert.AreEqual(0, pos);

            Assert.IsFalse(RegexParser.TryParseRange("A", ref pos, out characters));
            Assert.AreEqual(0, pos);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TryParseRangeTest2()
        {
            try
            {
                int pos = 0;
                string characters;
                Assert.IsFalse(RegexParser.TryParseRange(@"[", ref pos, out characters));
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(
                    new ArgumentException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_InvalidRegex_RangeMissingClosingBracket,
                        @"[",
                        1)).Message,
                    e.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TryParseRangeTest3()
        {
            try
            {
                int pos = 0;
                string characters;
                Assert.IsFalse(RegexParser.TryParseRange(@"[\", ref pos, out characters));
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(
                    new ArgumentException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_InvalidRegex_RangeMissingClosingBracket,
                        @"[\",
                        1)).Message,
                    e.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TryParseRangeTest4()
        {
            try
            {
                int pos = 0;
                string characters;
                Assert.IsFalse(RegexParser.TryParseRange("[9-0]", ref pos, out characters));
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(
                    new ArgumentException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_InvalidRegex_RangeOutOfOrder,
                        "[9-0]",
                        1)).Message,
                    e.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TryParseRangeTest5()
        {
            try
            {
                int pos = 0;
                string characters;
                Assert.IsFalse(RegexParser.TryParseRange("[]", ref pos, out characters));
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(
                    new ArgumentException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_InvalidRegex_RangeIsEmpty,
                        "[]",
                        1)).Message,
                    e.Message);
                throw;
            }
        }

        [TestMethod]
        public void TryParseGroupTest0()
        {
            int pos = 0;
            IList<string> words;
            Assert.IsTrue(RegexParser.TryParseGroup("(A)", ref pos, out words));
            Assert.AreEqual(1, words.Count);
            CollectionAssert.AreEqual(new string[] { "A" }, words.ToArray());

            pos = 0;
            Assert.IsTrue(RegexParser.TryParseGroup("(A|AB|ABC)", ref pos, out words));
            Assert.AreEqual(3, words.Count);
            CollectionAssert.AreEqual(new string[] { "A", "AB", "ABC" }, words.ToArray());
        }

        [TestMethod]
        public void TryParseGroupTest1()
        {
            int pos = 0;
            IList<string> words;
            Assert.IsFalse(RegexParser.TryParseGroup(null, ref pos, out words));
            Assert.AreEqual(0, pos);

            Assert.IsFalse(RegexParser.TryParseGroup(string.Empty, ref pos, out words));
            Assert.AreEqual(0, pos);

            Assert.IsFalse(RegexParser.TryParseGroup("A", ref pos, out words));
            Assert.AreEqual(0, pos);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TryParseGroupTest2()
        {
            try
            {
                int pos = 0;
                IList<string> words;
                Assert.IsFalse(RegexParser.TryParseGroup(@"(", ref pos, out words));
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(
                    new ArgumentException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_InvalidRegex_GroupMissingClosingBracket,
                        @"(",
                        1)).Message,
                    e.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TryParseGroupTest3()
        {
            try
            {
                int pos = 0;
                IList<string> words;
                Assert.IsFalse(RegexParser.TryParseGroup(@"(\", ref pos, out words));
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(
                    new ArgumentException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_InvalidRegex_GroupMissingClosingBracket,
                        @"(\",
                        1)).Message,
                    e.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TryParseGroupTest4()
        {
            try
            {
                int pos = 0;
                IList<string> words;
                Assert.IsFalse(RegexParser.TryParseGroup("()", ref pos, out words));
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(
                    new ArgumentException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_InvalidRegex_GroupIsEmpty,
                        "()",
                        1)).Message,
                    e.Message);
                throw;
            }
        }

        [TestMethod]
        public void TryParseQuantifierTest0()
        {
            int minlen;
            int maxlen;

            int pos = 0;
            Assert.IsTrue(RegexParser.TryParseQuantifier("*", ref pos, out minlen, out maxlen));
            Assert.AreEqual(1, pos);
            Assert.AreEqual(0, minlen);
            Assert.AreEqual(int.MaxValue, maxlen);

            pos = 0;
            Assert.IsTrue(RegexParser.TryParseQuantifier("+", ref pos, out minlen, out maxlen));
            Assert.AreEqual(1, pos);
            Assert.AreEqual(1, minlen);
            Assert.AreEqual(int.MaxValue, maxlen);

            pos = 0;
            Assert.IsTrue(RegexParser.TryParseQuantifier("?", ref pos, out minlen, out maxlen));
            Assert.AreEqual(1, pos);
            Assert.AreEqual(0, minlen);
            Assert.AreEqual(1, maxlen);

            pos = 0;
            Assert.IsTrue(RegexParser.TryParseQuantifier("{1,2}", ref pos, out minlen, out maxlen));
            Assert.AreEqual(5, pos);
            Assert.AreEqual(1, minlen);
            Assert.AreEqual(2, maxlen);

            pos = 0;
            Assert.IsTrue(RegexParser.TryParseQuantifier("{22}", ref pos, out minlen, out maxlen));
            Assert.AreEqual(4, pos);
            Assert.AreEqual(22, minlen);
            Assert.AreEqual(22, maxlen);

            pos = 0;
            Assert.IsTrue(RegexParser.TryParseQuantifier("{1,}", ref pos, out minlen, out maxlen));
            Assert.AreEqual(4, pos);
            Assert.AreEqual(1, minlen);
            Assert.AreEqual(int.MaxValue, maxlen);

            pos = 0;
            Assert.IsTrue(RegexParser.TryParseQuantifier("{,2}", ref pos, out minlen, out maxlen));
            Assert.AreEqual(4, pos);
            Assert.AreEqual(0, minlen);
            Assert.AreEqual(2, maxlen);
        }

        [TestMethod]
        public void TryParseQuantifierTest1()
        {
            int minlen;
            int maxlen;

            int pos = 0;
            Assert.IsFalse(RegexParser.TryParseQuantifier(null, ref pos, out minlen, out maxlen));
            Assert.AreEqual(0, pos);
            Assert.AreEqual(1, minlen);
            Assert.AreEqual(1, maxlen);

            Assert.IsFalse(RegexParser.TryParseQuantifier(string.Empty, ref pos, out minlen, out maxlen));
            Assert.AreEqual(0, pos);
            Assert.AreEqual(1, minlen);
            Assert.AreEqual(1, maxlen);

            Assert.IsFalse(RegexParser.TryParseQuantifier("A", ref pos, out minlen, out maxlen));
            Assert.AreEqual(0, pos);
            Assert.AreEqual(1, minlen);
            Assert.AreEqual(1, maxlen);

            Assert.IsFalse(RegexParser.TryParseQuantifier("{1", ref pos, out minlen, out maxlen));
            Assert.AreEqual(0, pos);
            Assert.AreEqual(1, minlen);
            Assert.AreEqual(1, maxlen);

            Assert.IsFalse(RegexParser.TryParseQuantifier("{1,", ref pos, out minlen, out maxlen));
            Assert.AreEqual(0, pos);
            Assert.AreEqual(1, minlen);
            Assert.AreEqual(1, maxlen);

            Assert.IsFalse(RegexParser.TryParseQuantifier("{1,2", ref pos, out minlen, out maxlen));
            Assert.AreEqual(0, pos);
            Assert.AreEqual(1, minlen);
            Assert.AreEqual(1, maxlen);

            Assert.IsFalse(RegexParser.TryParseQuantifier("{,2", ref pos, out minlen, out maxlen));
            Assert.AreEqual(0, pos);
            Assert.AreEqual(1, minlen);
            Assert.AreEqual(1, maxlen);

            Assert.IsFalse(RegexParser.TryParseQuantifier("{}", ref pos, out minlen, out maxlen));
            Assert.AreEqual(0, pos);
            Assert.AreEqual(1, minlen);
            Assert.AreEqual(1, maxlen);

            Assert.IsFalse(RegexParser.TryParseQuantifier("{,}", ref pos, out minlen, out maxlen));
            Assert.AreEqual(0, pos);
            Assert.AreEqual(1, minlen);
            Assert.AreEqual(1, maxlen);
        }
#endif
    }
}
