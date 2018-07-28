namespace Genix.DNN.Test
{
    using System;
    using System.Globalization;
    using Genix.DNN.LanguageModel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ContextTest
    {
        [TestMethod]
        public void FromRegexTest0()
        {
            Context context = Context.FromRegex(string.Empty, CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(NullContext));
            Assert.AreEqual(string.Empty, context.ToString());

            context = Context.FromRegex(@"\s?", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(Charset));
            Assert.AreEqual(" ?", context.ToString());

            context = Context.FromRegex(@"[\\\d]?", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(Charset));
            Assert.AreEqual(@"(0|1|2|3|4|5|6|7|8|9|\)?", context.ToString());

            context = Context.FromRegex(@"\O?", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(Charset));
            Assert.AreEqual("(0|1|2|3|4|5|6|7)?", context.ToString());

            context = Context.FromRegex(@"\x?", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(Charset));
            Assert.AreEqual("(0|1|2|3|4|5|6|7|8|9|A|B|C|D|E|F)?", context.ToString());

            context = Context.FromRegex(@"\w?", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(Charset));
            Assert.AreEqual("( |0|1|2|3|4|5|6|7|8|9|A|B|C|D|E|F|G|H|I|J|K|L|M|N|O|P|Q|R|S|T|U|V|W|X|Y|Z)?", context.ToString());

            context = Context.FromRegex(@"\w?", CultureInfo.GetCultureInfo("en"));
            Assert.IsInstanceOfType(context, typeof(Charset));
            Assert.AreEqual("( |0|1|2|3|4|5|6|7|8|9|A|B|C|D|E|F|G|H|I|J|K|L|M|N|O|P|Q|R|S|T|U|V|W|X|Y|Z)?", context.ToString());

            context = Context.FromRegex(@"\w?", CultureInfo.GetCultureInfo("ru"));
            Assert.IsInstanceOfType(context, typeof(Charset));
            Assert.AreEqual("( |0|1|2|3|4|5|6|7|8|9|А|Б|В|Г|Д|Е|Ж|З|И|Й|К|Л|М|Н|О|П|Р|С|Т|У|Ф|Х|Ц|Ч|Ш|Щ|Ъ|Ы|Ь|Э|Ю|Я)?", context.ToString());

            context = Context.FromRegex("A*", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(Charset));
            Assert.AreEqual("A*", context.ToString());

            context = Context.FromRegex("AB*", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(GraphContext));
            Assert.AreEqual("A~B*", context.ToString());

            context = Context.FromRegex("A+B", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(GraphContext));
            Assert.AreEqual("A+~B", context.ToString());

            context = Context.FromRegex("A|B?", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(GraphContext));
            Assert.AreEqual("A, B?", context.ToString());

            context = Context.FromRegex("A|", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(GraphContext));
            Assert.AreEqual("A, ", context.ToString());

            context = Context.FromRegex("|A", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(GraphContext));
            Assert.AreEqual(", A", context.ToString());

            context = Context.FromRegex("AB", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(Vocabulary));
            Assert.AreEqual("AB", context.ToString());

            context = Context.FromRegex("AB|CD", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(Vocabulary));
            Assert.AreEqual("(AB|CD)", context.ToString());

            context = Context.FromRegex("(AB|CD)", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(Vocabulary));
            Assert.AreEqual("(AB|CD)", context.ToString());

            context = Context.FromRegex("(AB|(CD|EF))", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(Vocabulary));
            Assert.AreEqual("(AB|CD|EF)", context.ToString());

            context = Context.FromRegex("[A]{3}", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(Charset));
            Assert.AreEqual("A{3}", context.ToString());

            context = Context.FromRegex("[A-]{,3}", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(Charset));
            Assert.AreEqual("(-|A){,3}", context.ToString());

            context = Context.FromRegex("[-A]{3,}", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(Charset));
            Assert.AreEqual("(-|A){3,}", context.ToString());

            context = Context.FromRegex("[A-CD]{3,14}", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(Charset));
            Assert.AreEqual("(A|B|C|D){3,14}", context.ToString());

            context = Context.FromRegex(@"[\d]?", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(Charset));
            Assert.AreEqual("(0|1|2|3|4|5|6|7|8|9)?", context.ToString());

            context = Context.FromRegex(@"[\d-A]?", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(Charset));
            Assert.AreEqual("(-|0|1|2|3|4|5|6|7|8|9|A)?", context.ToString());
        }

        [TestMethod]
        public void FromRegexTestConcatenationOptimization()
        {
            Context context = Context.FromRegex("AB", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(Vocabulary));
            Assert.AreEqual("AB", context.ToString());

            context = Context.FromRegex("A(BC)", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(Vocabulary));
            Assert.AreEqual("ABC", context.ToString());

            context = Context.FromRegex("(BC)A", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(Vocabulary));
            Assert.AreEqual("BCA", context.ToString());

            context = Context.FromRegex("(BC)(DE)", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(Vocabulary));
            Assert.AreEqual("BCDE", context.ToString());

            context = Context.FromRegex("A[BC]", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(GraphContext));
            Assert.AreEqual("A~(B|C)", context.ToString());
        }

        [TestMethod]
        public void FromRegexTestUnionOptimization()
        {
            // merge into character
            Context context = Context.FromRegex("A|B", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(Charset));
            Assert.AreEqual("(A|B)", context.ToString());

            context = Context.FromRegex("A|[BC]", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(Charset));
            Assert.AreEqual("(A|B|C)", context.ToString());

            context = Context.FromRegex("A|BC", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(Vocabulary));
            Assert.AreEqual("(A|BC)", context.ToString());

            context = Context.FromRegex("A|(BC|DE)", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(Vocabulary));
            Assert.AreEqual("(A|BC|DE)", context.ToString());

            // merge into charset
            context = Context.FromRegex("[AB]|C", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(Charset));
            Assert.AreEqual("(A|B|C)", context.ToString());

            context = Context.FromRegex("[AB]|[CD]", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(Charset));
            Assert.AreEqual("(A|B|C|D)", context.ToString());

            context = Context.FromRegex("[AB]|CD", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(Vocabulary));
            Assert.AreEqual("(A|B|CD)", context.ToString());

            context = Context.FromRegex("[AB]|(CD|EF)", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(Vocabulary));
            Assert.AreEqual("(A|B|CD|EF)", context.ToString());

            // merge into word
            context = Context.FromRegex("AB|C", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(Vocabulary));
            Assert.AreEqual("(AB|C)", context.ToString());

            context = Context.FromRegex("AB|[CD]", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(Vocabulary));
            Assert.AreEqual("(AB|C|D)", context.ToString());

            context = Context.FromRegex("AB|CD", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(Vocabulary));
            Assert.AreEqual("(AB|CD)", context.ToString());

            context = Context.FromRegex("AB|(CD|EF)", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(Vocabulary));
            Assert.AreEqual("(AB|CD|EF)", context.ToString());

            // merge into words
            context = Context.FromRegex("(AB|CD)|E", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(Vocabulary));
            Assert.AreEqual("(AB|CD|E)", context.ToString());

            context = Context.FromRegex("(AB|CD)|[EF]", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(Vocabulary));
            Assert.AreEqual("(AB|CD|E|F)", context.ToString());

            context = Context.FromRegex("(AB|CD)|EF", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(Vocabulary));
            Assert.AreEqual("(AB|CD|EF)", context.ToString());

            context = Context.FromRegex("(AB|CD)|(EF|GH)", CultureInfo.InvariantCulture);
            Assert.IsInstanceOfType(context, typeof(Vocabulary));
            Assert.AreEqual("(AB|CD|EF|GH)", context.ToString());
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void FromRegexTestNull()
        {
            Context.FromRegex(null, CultureInfo.CurrentCulture);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void FromRegexTestInvalidRegexTokenNotQuantifiable()
        {
            try
            {
                Assert.IsNotNull(Context.FromRegex("*", CultureInfo.InvariantCulture));
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(
                    new ArgumentException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_InvalidRegex_TokenNotQuantifiable,
                        "*",
                        0)).Message,
                    e.Message);
                throw;
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void FromRegexTestInvalidRegexGroupMissingClosingBracket()
        {
            try
            {
                Assert.IsNotNull(Context.FromRegex("(a", CultureInfo.InvariantCulture));
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(
                    new ArgumentException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_InvalidRegex_GroupMissingClosingBracket,
                        "(a",
                        0)).Message,
                    e.Message);
                throw;
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void FromRegexTestInvalidRegexRangeWithSequence()
        {
            try
            {
                Assert.IsNotNull(Context.FromRegex(@"[a-\w]", CultureInfo.InvariantCulture));
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(
                    new ArgumentException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_InvalidRegex_RangeWithSequence,
                        @"[a-\w]",
                        2)).Message,
                    e.Message);
                throw;
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void FromRegexTestInvalidRegexRangeOutOfOrder()
        {
            try
            {
                Assert.IsNotNull(Context.FromRegex("[z-a]", CultureInfo.InvariantCulture));
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(
                    new ArgumentException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_InvalidRegex_RangeOutOfOrder,
                        "[z-a]",
                        2)).Message,
                    e.Message);
                throw;
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void FromRegexTestInvalidRegexRangeMissingClosingBracket()
        {
            try
            {
                Assert.IsNotNull(Context.FromRegex("[a-z", CultureInfo.InvariantCulture));
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(
                    new ArgumentException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_InvalidRegex_RangeMissingClosingBracket,
                        "[a-z",
                        0)).Message,
                    e.Message);
                throw;
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void FromRegexTestInvalidRegexRangeIsEmpty()
        {
            try
            {
                Assert.IsNotNull(Context.FromRegex("a[]", CultureInfo.InvariantCulture));
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(
                    new ArgumentException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_InvalidRegex_RangeIsEmpty,
                        "a[]",
                        1)).Message,
                    e.Message);
                throw;
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void FromRegexTestInvalidRegexMayNotEndWithBackslash()
        {
            try
            {
                Assert.IsNotNull(Context.FromRegex(@"a\", CultureInfo.InvariantCulture));
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(
                    new ArgumentException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_InvalidRegex_MayNotEndWithBackslash,
                        @"a\",
                        1)).Message,
                    e.Message);
                throw;
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void FromRegexTestInvalidRegexUnsupportedCulture()
        {
            try
            {
                Assert.IsNotNull(Context.FromRegex(@"a\w", CultureInfo.GetCultureInfo("es")));
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(
                    new ArgumentException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_InvalidRegex_UnsupportedCulture,
                        "es",
                        @"a\w",
                        1)).Message,
                    e.Message);
                throw;
            }
        }

        /*[TestMethod]
        public void FromRegexTest2()
        {
            Charset charset = Context.FromRegex(@"\d", CultureInfo.CurrentCulture);

            Assert.AreEqual("0123456789", new string(charset.Characters.Keys.ToArray()));
            Assert.AreEqual(1, charset.MinRepeatCount);
            Assert.AreEqual(1, charset.MaxRepeatCount);
        }

        [TestMethod]
        public void FromRegexTest3()
        {
            Charset charset = Context.FromRegex(@"\d*", CultureInfo.CurrentCulture);

            Assert.AreEqual("0123456789", new string(charset.Characters.Keys.ToArray()));
            Assert.AreEqual(0, charset.MinRepeatCount);
            Assert.AreEqual(int.MaxValue, charset.MaxRepeatCount);
        }

        [TestMethod]
        public void FromRegexTest4()
        {
            Charset charset = Context.FromRegex(@"\d+", CultureInfo.CurrentCulture);

            Assert.AreEqual("0123456789", new string(charset.Characters.Keys.ToArray()));
            Assert.AreEqual(1, charset.MinRepeatCount);
            Assert.AreEqual(int.MaxValue, charset.MaxRepeatCount);
        }

        [TestMethod]
        public void FromRegexTest5()
        {
            Charset charset = Context.FromRegex(@"\d?", CultureInfo.CurrentCulture);

            Assert.AreEqual("0123456789", new string(charset.Characters.Keys.ToArray()));
            Assert.AreEqual(0, charset.MinRepeatCount);
            Assert.AreEqual(1, charset.MaxRepeatCount);
        }

        [TestMethod]
        public void FromRegexTest6()
        {
            Charset charset = Context.FromRegex(@"\d{2}", CultureInfo.CurrentCulture);

            Assert.AreEqual("0123456789", new string(charset.Characters.Keys.ToArray()));
            Assert.AreEqual(2, charset.MinRepeatCount);
            Assert.AreEqual(2, charset.MaxRepeatCount);
        }

        [TestMethod]
        public void FromRegexTest7()
        {
            Charset charset = Context.FromRegex(@"\d{2,}", CultureInfo.CurrentCulture);

            Assert.AreEqual("0123456789", new string(charset.Characters.Keys.ToArray()));
            Assert.AreEqual(2, charset.MinRepeatCount);
            Assert.AreEqual(int.MaxValue, charset.MaxRepeatCount);
        }

        [TestMethod]
        public void FromRegexTest8()
        {
            Charset charset = Context.FromRegex(@"\d{,2}", CultureInfo.CurrentCulture);

            Assert.AreEqual("0123456789", new string(charset.Characters.Keys.ToArray()));
            Assert.AreEqual(0, charset.MinRepeatCount);
            Assert.AreEqual(2, charset.MaxRepeatCount);
        }

        [TestMethod]
        public void FromRegexTest9()
        {
            Charset charset = Context.FromRegex(@"\d{2,3}", CultureInfo.CurrentCulture);

            Assert.AreEqual("0123456789", new string(charset.Characters.Keys.ToArray()));
            Assert.AreEqual(2, charset.MinRepeatCount);
            Assert.AreEqual(3, charset.MaxRepeatCount);
        }

        [TestMethod]
        public void FromRegexTest10()
        {
            Charset charset = Context.FromRegex(@"[0-9]{2,3}", CultureInfo.CurrentCulture);

            Assert.AreEqual("0123456789", new string(charset.Characters.Keys.ToArray()));
            Assert.AreEqual(2, charset.MinRepeatCount);
            Assert.AreEqual(3, charset.MaxRepeatCount);
        }

        [TestMethod]
        public void FromRegexTest11()
        {
            Charset charset = Context.FromRegex(@"0{2,3}", CultureInfo.CurrentCulture);

            Assert.AreEqual("0", new string(charset.Characters.Keys.ToArray()));
            Assert.AreEqual(2, charset.MinRepeatCount);
            Assert.AreEqual(3, charset.MaxRepeatCount);
        }

        [TestMethod]
        public void FromRegexTest12()
        {
            Charset charset = Context.FromRegex(@"[05-7]{2,3}", CultureInfo.CurrentCulture);

            Assert.AreEqual("0567", new string(charset.Characters.Keys.ToArray()));
            Assert.AreEqual(2, charset.MinRepeatCount);
            Assert.AreEqual(3, charset.MaxRepeatCount);
        }*/
    }
}
