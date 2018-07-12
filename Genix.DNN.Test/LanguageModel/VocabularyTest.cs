namespace Genix.DNN.Test
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Genix.DNN.LanguageModel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class VocabularyTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string path = Path.Combine(Path.GetDirectoryName(assembly.Location), @"..\TestData\tinyshakespeare.txt");

            Vocabulary context = new Vocabulary(
                File.ReadAllLines(path)
                    .Select(x => x.ToUpperInvariant())
                    .SelectMany(x => x.Split(new char[] { ' ', '.', ',', '!', '?', '-', ':', ';', '&', '\\' }, StringSplitOptions.RemoveEmptyEntries))
                    .Select(x => x.Trim('\'')),
                1,
                1);

            /*string s1 = JsonConvert.SerializeObject(context);
            Vocabulary vocabulary2 = JsonConvert.DeserializeObject<Vocabulary>(s1);
            string s2 = JsonConvert.SerializeObject(vocabulary2);*/

            ////Assert.AreEqual(context, vocabulary2);
            ////Assert.AreEqual(s1, s2);

            foreach (var text in context.InitialState.Enumerate().OrderByDescending(x => x.Probability))
            {
                ////Console.WriteLine($"{s.Item1} {s.Item2}");
            }
        }

        [TestMethod]
        public void PrivateConstructorTest0()
        {
            Vocabulary context = Helpers.CreateClassWithPrivateConstructor<Vocabulary>();

            Assert.AreEqual(0, context.CharCount);
            Assert.AreEqual(0, context.WordCount);
            Assert.AreEqual(0, context.UniqueWordCount);
            Assert.AreEqual(0, context.MaxWordLength);

            Assert.AreEqual(1, context.MinRepeatCount);
            Assert.AreEqual(1, context.MaxRepeatCount);
            Assert.IsTrue(context.IsTail);
            Assert.IsNull(context.Parent);
        }

        [TestMethod]
        public void ConstructorTest10()
        {
            Vocabulary context = new Vocabulary(new string[] { "AB", "CD" }, 0, 1);

            Assert.AreEqual(4, context.CharCount);
            Assert.AreEqual(2, context.WordCount);
            Assert.AreEqual(2, context.UniqueWordCount);
            Assert.AreEqual(2, context.MaxWordLength);

            Assert.AreEqual(0, context.MinRepeatCount);
            Assert.AreEqual(1, context.MaxRepeatCount);
            Assert.IsTrue(context.IsTail);
            Assert.IsNull(context.Parent);

            /*foreach (char ch in words)
            {
                Assert.AreEqual(1.0f / words.Length, context.Characters[ch]);
            }*/
        }

        [TestMethod]
        [Description("Duplicate words in context - collapse and sum counts.")]
        public void ConstructorTest101()
        {
            Vocabulary context = new Vocabulary(new string[] { "AB", "CD", "AB" }, 0, 1);

            Assert.AreEqual(6, context.CharCount);
            Assert.AreEqual(3, context.WordCount);
            Assert.AreEqual(2, context.UniqueWordCount);
            Assert.AreEqual(2, context.MaxWordLength);

            Assert.AreEqual(0, context.MinRepeatCount);
            Assert.AreEqual(1, context.MaxRepeatCount);
            Assert.IsTrue(context.IsTail);
            Assert.IsNull(context.Parent);

            /*Assert.AreEqual(0.25f, context.Characters['0']);
            Assert.AreEqual(0.5f, context.Characters['A']);
            Assert.AreEqual(0.25f, context.Characters['z']);*/
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest11()
        {
            Assert.IsNotNull(new Vocabulary((IEnumerable<string>)null, 0, 1));
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void ConstructorTest12()
        {
            try
            {
                Assert.IsNotNull(new Vocabulary(new string[] { }, 0, 1));
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(new ArgumentException(Properties.Resources.E_LanguageModel_EmptyVocabulary).Message, e.Message);
                throw;
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void ConstructorTest13()
        {
            try
            {
                Assert.IsNotNull(new Vocabulary(new string[] { "TEST" }, -1, 1));
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(new ArgumentException(Properties.Resources.E_LanguageModel_InvalidMinimumRepeatCount).Message, e.Message);
                throw;
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void ConstructorTest14()
        {
            try
            {
                Assert.IsNotNull(new Vocabulary(new string[] { "TEST" }, 1, 0));
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(new ArgumentException(Properties.Resources.E_LanguageModel_InvalidMaximumRepeatCount).Message, e.Message);
                throw;
            }
        }

        [TestMethod]
        public void ConstructorTest20()
        {
            List<(string, int)> words = new List<(string, int)>()
            {
                ("AB", 1),
                ("CD", 3)
            };
            Vocabulary context = new Vocabulary(words, 0, 1);

            Assert.AreEqual(8, context.CharCount);
            Assert.AreEqual(4, context.WordCount);
            Assert.AreEqual(2, context.UniqueWordCount);
            Assert.AreEqual(2, context.MaxWordLength);

            Assert.AreEqual(0, context.MinRepeatCount);
            Assert.AreEqual(1, context.MaxRepeatCount);
            Assert.IsTrue(context.IsTail);
            Assert.IsNull(context.Parent);

            /*foreach ((string, int) ch in words)
            {
                Assert.AreEqual((float)ch.Item2 / 10, context.Characters[ch.Item1]);
            }*/
        }

        [TestMethod]
        [Description("Duplicate words in context - should sum counts.")]
        public void ConstructorTest201()
        {
            List<(string, int)> words = new List<(string, int)>()
            {
                ("AB", 1),
                ("CD", 3),
                ("AB", 1)
            };
            Vocabulary context = new Vocabulary(words, 0, 1);

            Assert.AreEqual(10, context.CharCount);
            Assert.AreEqual(5, context.WordCount);
            Assert.AreEqual(2, context.UniqueWordCount);
            Assert.AreEqual(2, context.MaxWordLength);

            Assert.AreEqual(0, context.MinRepeatCount);
            Assert.AreEqual(1, context.MaxRepeatCount);
            Assert.IsTrue(context.IsTail);
            Assert.IsNull(context.Parent);

            /*Assert.AreEqual(0.25f, context.Characters['0']);
            Assert.AreEqual(0.75f, context.Characters['A']);*/
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        [Description("Invalid count in context.")]
        public void ConstructorTest202()
        {
            try
            {
                List<(string, int)> words = new List<(string, int)>()
                {
                    ("0", 0)
                };

                Assert.IsNotNull(new Vocabulary(words, 0, 1));
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(
                    new ArgumentException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_LanguageModel_InvalidWordCount, 0, '0')).Message,
                    e.Message);
                throw;
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest21()
        {
            Assert.IsNotNull(new Vocabulary((IEnumerable<(string, int)>)null, 0, 1));
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void ConstructorTest22()
        {
            try
            {
                Assert.IsNotNull(new Vocabulary(new List<(string, int)>(), 0, 1));
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(new ArgumentException(Properties.Resources.E_LanguageModel_EmptyVocabulary).Message, e.Message);
                throw;
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void ConstructorTest23()
        {
            try
            {
                List<(string, int)> words = new List<(string, int)>()
                {
                    ("0", 1)
                };

                Assert.IsNotNull(new Vocabulary(words, -1, 1));
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(new ArgumentException(Properties.Resources.E_LanguageModel_InvalidMinimumRepeatCount).Message, e.Message);
                throw;
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void ConstructorTest24()
        {
            try
            {
                List<(string, int)> words = new List<(string, int)>()
                {
                    ("0", 1)
                };

                Assert.IsNotNull(new Vocabulary(words, 1, 0));
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(new ArgumentException(Properties.Resources.E_LanguageModel_InvalidMaximumRepeatCount).Message, e.Message);
                throw;
            }
        }

        [TestMethod]
        public void CopyConstructorTest0()
        {
            Vocabulary context = new Vocabulary(new Vocabulary(new string[] { "TEST" }, 0, 1));

            Assert.AreEqual(4, context.CharCount);
            Assert.AreEqual(1, context.WordCount);
            Assert.AreEqual(1, context.UniqueWordCount);
            Assert.AreEqual(4, context.MaxWordLength);

            Assert.AreEqual(0, context.MinRepeatCount);
            Assert.AreEqual(1, context.MaxRepeatCount);
            Assert.IsTrue(context.IsTail);
            Assert.IsNull(context.Parent);

            /*foreach (char ch in words)
            {
                Assert.AreEqual(1.0f / words.Length, context.Characters[ch]);
            }*/
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void CopyConstructorTest1()
        {
            Assert.IsNotNull(new Vocabulary(null));
        }

        [TestMethod]
        public void EqualsTest()
        {
            Vocabulary vocabulary1 = new Vocabulary(new string[] { "TEST" }, 1, 2);

            Assert.IsFalse(vocabulary1.Equals(null));
            Assert.IsTrue(vocabulary1.Equals(vocabulary1));
            Assert.IsTrue(vocabulary1.Equals(new Vocabulary(new string[] { "TEST" }, 1, 2)));
            Assert.IsFalse(vocabulary1.Equals(new Vocabulary(new string[] { "TEST2" }, 1, 2)));
            Assert.IsFalse(vocabulary1.Equals(new Vocabulary(new string[] { "TEST" }, 2, 2)));
            Assert.IsFalse(vocabulary1.Equals(new Vocabulary(new string[] { "TEST" }, 1, 1)));
        }

        [TestMethod]
        public void GetHashCodeTest0()
        {
            Vocabulary context = new Vocabulary(new string[] { "TEST" }, 1, 2);
            Assert.AreEqual(6, context.GetHashCode());
        }

        [TestMethod]
        public void GetHashCodeTest1()
        {
            Vocabulary context = Helpers.CreateClassWithPrivateConstructor<Vocabulary>();
            Assert.AreEqual(1, context.GetHashCode());
        }

        [TestMethod]
        public void CloneTest()
        {
            Vocabulary vocabulary1 = new Vocabulary(new string[] { "TEST" }, 1, 2);
            Vocabulary vocabulary2 = vocabulary1.Clone() as Vocabulary;

            Assert.AreEqual(vocabulary1, vocabulary2);
            Assert.AreEqual(JsonConvert.SerializeObject(vocabulary1), JsonConvert.SerializeObject(vocabulary2));
        }

        [TestMethod]
        public void SerializeTest()
        {
            Vocabulary vocabulary1 = new Vocabulary(new string[] { "TEST" }, 1, 2);
            string s1 = JsonConvert.SerializeObject(vocabulary1);
            Vocabulary vocabulary2 = JsonConvert.DeserializeObject<Vocabulary>(s1);
            string s2 = JsonConvert.SerializeObject(vocabulary2);

            Assert.AreEqual(vocabulary1, vocabulary2);
            Assert.AreEqual(s1, s2);
        }

        [TestMethod]
        public void SaveToStringTest()
        {
            Vocabulary vocabulary1 = new Vocabulary(new string[] { "TEST" }, 1, 2);
            string s1 = vocabulary1.SaveToString();
            Vocabulary vocabulary2 = Vocabulary.FromString(s1);

            Assert.AreEqual(vocabulary1, vocabulary2);
        }

        [TestMethod]
        public void SaveToMemoryTest()
        {
            Vocabulary vocabulary1 = new Vocabulary(new string[] { "TEST" }, 1, 2);
            byte[] b1 = vocabulary1.SaveToMemory();
            Vocabulary vocabulary2 = Vocabulary.FromMemory(b1);

            Assert.AreEqual(vocabulary1, vocabulary2);
        }

        [TestMethod]
        public void SaveToFileTest()
        {
            string tempFileName = Path.GetTempFileName();
            try
            {
                Vocabulary vocabulary1 = new Vocabulary(new string[] { "TEST" }, 1, 2);
                vocabulary1.SaveToFile(tempFileName);
                Vocabulary vocabulary2 = Vocabulary.FromFile(tempFileName);

                Assert.AreEqual(vocabulary1, vocabulary2);
            }
            finally
            {
                File.Delete(tempFileName);
            }
        }

        [TestMethod]
        public void InitialStateMethodTest0()
        {
            Vocabulary context = new Vocabulary(new string[] { "TEST" }, 1, 2);
            Vocabulary.VocabularyState state = context.InitialState as Vocabulary.VocabularyState;

            Assert.AreEqual((char)0, state.Char);
            Assert.IsFalse(state.WordEnd);
            Assert.AreEqual(0.0f, state.CharProbability);
            Assert.AreEqual(0.0f, state.WordEndProbability);
            Assert.IsFalse(state.ContextWordEnd);
            Assert.AreEqual(1, state.RepeatCount);
            Assert.AreSame(context, state.Context);
            Assert.AreEqual(0, state.Seek);
        }

        [TestMethod]
        public void InitialStateMethodTest1()
        {
            Vocabulary context = new Vocabulary(new string[] { "TEST" }, 0, 2);
            Vocabulary.VocabularyState state = context.InitialState as Vocabulary.VocabularyState;

            Assert.AreEqual((char)0, state.Char);
            Assert.IsFalse(state.WordEnd);
            Assert.AreEqual(0.0f, state.CharProbability);
            Assert.AreEqual(0.0f, state.WordEndProbability);
            Assert.IsTrue(state.ContextWordEnd);
            Assert.AreEqual(1, state.RepeatCount);
            Assert.AreSame(context, state.Context);
            Assert.AreEqual(0, state.Seek);
        }

        [TestMethod]
        [Description("Repetition counts: 0 to 1, 1 to 1.")]
        public void NextStatesTest1()
        {
            const int MaxRepeatCount = 1;
            for (int minRepeatCount = 0; minRepeatCount <= MaxRepeatCount; minRepeatCount++)
            {
                Vocabulary context = new Vocabulary(new string[] { "AB" }, minRepeatCount, MaxRepeatCount);

                State[] states = context.InitialState.NextStates().Values.ToArray();
                Assert.AreEqual(1, states.Length);

                Context.ContextState state = states[0] as Context.ContextState;
                Assert.AreEqual('A', state.Char);
                Assert.IsFalse(state.WordEnd);
                Assert.IsFalse(state.ContextWordEnd);
                Assert.AreEqual(1, state.RepeatCount);
                Assert.AreEqual(1.0f, state.CharProbability);
                Assert.AreEqual(0.0f, state.WordEndProbability);

                states = state.NextStates().Values.ToArray();
                Assert.AreEqual(1, states.Length);

                state = states[0] as Context.ContextState;
                Assert.AreEqual('B', state.Char);
                Assert.IsTrue(state.WordEnd);
                Assert.IsTrue(state.ContextWordEnd);
                Assert.AreEqual(1, state.RepeatCount);
                Assert.AreEqual(1.0f, state.CharProbability);
                Assert.AreEqual(1.0f, state.WordEndProbability);

                Assert.IsNull(state.NextStates());
            }
        }

        [TestMethod]
        [Description("Repetition count: 1 to 1, 1 to 2.")]
        public void NextStatesTest2()
        {
            const int MaxRepeatCount = 2;
            for (int minRepeatCount = 1; minRepeatCount <= MaxRepeatCount; minRepeatCount++)
            {
                Vocabulary context = new Vocabulary(new string[] { "AB" }, minRepeatCount, MaxRepeatCount);

                State[] states = context.InitialState.NextStates().Values.ToArray();
                Assert.AreEqual(1, states.Length);

                Context.ContextState state = states[0] as Context.ContextState;
                Assert.AreEqual('A', state.Char);
                Assert.IsFalse(state.WordEnd);
                Assert.IsFalse(state.ContextWordEnd);
                Assert.AreEqual(1, state.RepeatCount);
                Assert.AreEqual(1.0f, state.CharProbability);
                Assert.AreEqual(0.0f, state.WordEndProbability);

                states = state.NextStates().Values.ToArray();
                Assert.AreEqual(1, states.Length);

                state = states[0] as Context.ContextState;
                Assert.AreEqual('B', state.Char);
                Assert.AreEqual(minRepeatCount < MaxRepeatCount, state.WordEnd);
                Assert.AreEqual(minRepeatCount < MaxRepeatCount, state.ContextWordEnd);
                Assert.AreEqual(1, state.RepeatCount);
                Assert.AreEqual(1.0f, state.CharProbability);
                Assert.AreEqual(1.0f, state.WordEndProbability);

                states = state.NextStates().Values.ToArray();
                Assert.AreEqual(1, states.Length);

                state = states[0] as Context.ContextState;
                Assert.AreEqual(' ', state.Char);
                Assert.IsFalse(state.WordEnd);
                Assert.IsFalse(state.ContextWordEnd);
                Assert.AreEqual(2, state.RepeatCount);
                Assert.AreEqual(1.0f, state.CharProbability);
                Assert.AreEqual(0.0f, state.WordEndProbability);

                states = state.NextStates().Values.ToArray();
                Assert.AreEqual(1, states.Length);

                state = states[0] as Context.ContextState;
                Assert.AreEqual('A', state.Char);
                Assert.IsFalse(state.WordEnd);
                Assert.IsFalse(state.ContextWordEnd);
                Assert.AreEqual(2, state.RepeatCount);
                Assert.AreEqual(1.0f, state.CharProbability);
                Assert.AreEqual(0.0f, state.WordEndProbability);

                states = state.NextStates().Values.ToArray();
                Assert.AreEqual(1, states.Length);

                state = states[0] as Context.ContextState;
                Assert.AreEqual('B', state.Char);
                Assert.IsTrue(state.WordEnd);
                Assert.IsTrue(state.ContextWordEnd);
                Assert.AreEqual(2, state.RepeatCount);
                Assert.AreEqual(1.0f, state.CharProbability);
                Assert.AreEqual(1.0f, state.WordEndProbability);

                Assert.IsNull(state.NextStates());
            }
        }

        [TestMethod]
        [Description("Repetition count: 1 to 1.")]
        public void EnumerateTest1()
        {
            Vocabulary context = new Vocabulary(new string[] { "TEST" }, 1, 1);

            (string s, float f)[] expected = new(string, float)[]
            {
                ("TEST", 0.0f),
            };

            (string s, float f)[] actual = context.InitialState.Enumerate().ToArray();

            CollectionAssert.AreEqual(expected.Select(x => x.s).ToArray(), actual.Select(x => x.s).ToArray());
        }

        [TestMethod]
        [Description("Repetition count: 1 to 2.")]
        public void EnumerateTest2()
        {
            Vocabulary context = new Vocabulary(new string[] { "TEST" }, 1, 2);

            (string s, float f)[] expected = new(string, float)[]
            {
                ("TEST", 0.0f),
                ("TEST TEST", 0.0f),
            };

            (string s, float f)[] actual = context.InitialState.Enumerate().ToArray();

            CollectionAssert.AreEqual(expected.Select(x => x.s).ToArray(), actual.Select(x => x.s).ToArray());
        }

        [TestMethod]
        [Description("Repetition count: 2 to 2.")]
        public void EnumerateTest3()
        {
            Vocabulary context = new Vocabulary(new string[] { "TEST" }, 2, 2);

            (string s, float f)[] expected = new(string, float)[]
            {
                ("TEST TEST", 0.0f),
            };

            (string s, float f)[] actual = context.InitialState.Enumerate().ToArray();

            CollectionAssert.AreEqual(expected.Select(x => x.s).ToArray(), actual.Select(x => x.s).ToArray());
        }

        [TestMethod]
        [Description("Repetition count: 1 to 3.")]
        public void EnumerateTest4()
        {
            Vocabulary context = new Vocabulary(new string[] { "TEST" }, 1, 3);

            (string s, float f)[] expected = new(string, float)[]
            {
                ("TEST", 0.0f),
                ("TEST TEST", 0.0f),
                ("TEST TEST TEST", 0.0f),
            };

            (string s, float f)[] actual = context.InitialState.Enumerate().ToArray();

            CollectionAssert.AreEqual(expected.Select(x => x.s).ToArray(), actual.Select(x => x.s).ToArray());
        }

        [TestMethod]
        [Description("Validate statistics. Repetition count: 1.")]
        public void EnumerateTest5()
        {
            Vocabulary context = new Vocabulary(new string[] { "A", "B", "AA", "AB", "AAA" }, 1, 1);

            (string s, float f)[] expected = new(string, float)[]
            {
                ("A", 0.2f),
                ("AA", 0.2f),
                ("AAA", 0.2f),
                ("AB", 0.2f),
                ("B", 0.2f),
            };

            (string s, float f)[] actual = context.InitialState.Enumerate().ToArray();

            CollectionAssert.AreEqual(expected.Select(x => x.s).ToArray(), actual.Select(x => x.s).ToArray());
            CollectionAssert.AreEqual(expected.Select(x => x.f).ToArray(), actual.Select(x => x.f).ToArray());

            Assert.AreEqual(1.0f, expected.Sum(x => x.f), 1e-4f);
        }
    }
}
