namespace Accord.DNN.Test
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Accord.DNN.LanguageModel;
    using Newtonsoft.Json;
    using System.IO;

    [TestClass]
    public class CharsetTest
    {
        [TestMethod]
        public void PrivateConstructorTest0()
        {
            Charset charset = Helpers.CreateClassWithPrivateConstructor<Charset>();

            Assert.AreEqual(0, charset.Characters.Count);
            Assert.AreEqual(1, charset.MinRepeatCount);
            Assert.AreEqual(1, charset.MaxRepeatCount);
            Assert.IsTrue(charset.IsTail);
            Assert.IsNull(charset.Parent);
        }

        [TestMethod]
        public void ConstructorTest10()
        {
            const string Characters = "0Az?";
            Charset charset = new Charset(Characters, 0, 1);

            Assert.AreEqual(4, charset.Characters.Count);
            Assert.AreEqual(0, charset.MinRepeatCount);
            Assert.AreEqual(1, charset.MaxRepeatCount);
            Assert.IsTrue(charset.IsTail);
            Assert.IsNull(charset.Parent);

            foreach (char ch in Characters)
            {
                Assert.AreEqual(1.0f / Characters.Length, charset.Characters[ch]);
            }
        }

        [TestMethod]
        [Description("Duplicate characters in charset - collapse and sum counts.")]
        public void ConstructorTest101()
        {
            const string Characters = "0AzA";
            Charset charset = new Charset(Characters, 0, 1);

            Assert.AreEqual(3, charset.Characters.Count);
            Assert.AreEqual(0, charset.MinRepeatCount);
            Assert.AreEqual(1, charset.MaxRepeatCount);
            Assert.IsTrue(charset.IsTail);
            Assert.IsNull(charset.Parent);

            Assert.AreEqual(0.25f, charset.Characters['0']);
            Assert.AreEqual(0.5f, charset.Characters['A']);
            Assert.AreEqual(0.25f, charset.Characters['z']);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest11()
        {
            Assert.IsNotNull(new Charset((IEnumerable<char>)null, 0, 1));
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void ConstructorTest12()
        {
            try
            {
                Assert.IsNotNull(new Charset(string.Empty, 0, 1));
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(new ArgumentException(Properties.Resources.E_LanguageModel_EmptyCharset).Message, e.Message);
                throw;
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void ConstructorTest13()
        {
            try
            {
                Assert.IsNotNull(new Charset("0Az", -1, 1));
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
                Assert.IsNotNull(new Charset("0Az", 1, 0));
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
            List<(char, int)> characters = new List<(char, int)>()
            {
                ('0', 1),
                ('A', 2),
                ('z', 3),
                ('?', 4)
            };
            Charset charset = new Charset(characters, 0, 1);

            Assert.AreEqual(4, charset.Characters.Count);
            Assert.AreEqual(0, charset.MinRepeatCount);
            Assert.AreEqual(1, charset.MaxRepeatCount);
            Assert.IsTrue(charset.IsTail);
            Assert.IsNull(charset.Parent);

            foreach ((char ch, int count) in characters)
            {
                Assert.AreEqual((float)count / 10, charset.Characters[ch]);
            }
        }

        [TestMethod]
        [Description("Duplicate characters in charset - should sum counts.")]
        public void ConstructorTest201()
        {
            List<(char, int)> characters = new List<(char, int)>()
            {
                ('0', 1),
                ('A', 2),
                ('A', 1),
            };
            Charset charset = new Charset(characters, 0, 1);

            Assert.AreEqual(2, charset.Characters.Count);
            Assert.AreEqual(0, charset.MinRepeatCount);
            Assert.AreEqual(1, charset.MaxRepeatCount);
            Assert.IsTrue(charset.IsTail);
            Assert.IsNull(charset.Parent);

            Assert.AreEqual(0.25f, charset.Characters['0']);
            Assert.AreEqual(0.75f, charset.Characters['A']);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        [Description("Invalid count in charset.")]
        public void ConstructorTest202()
        {
            try
            {
                List<(char, int)> characters = new List<(char, int)>()
                {
                    ('0', 0)
                };

                Assert.IsNotNull(new Charset(characters, 0, 1));
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(
                    new ArgumentException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_LanguageModel_InvalidCharacterCount, 0, '0')).Message,
                    e.Message);
                throw;
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest21()
        {
            Assert.IsNotNull(new Charset((IEnumerable<(char, int)>)null, 0, 1));
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void ConstructorTest22()
        {
            try
            {
                Assert.IsNotNull(new Charset(new List<(char, int)>(), 0, 1));
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(new ArgumentException(Properties.Resources.E_LanguageModel_EmptyCharset).Message, e.Message);
                throw;
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void ConstructorTest23()
        {
            try
            {
                List<(char, int)> characters = new List<(char, int)>()
                {
                    ('0', 1)
                };

                Assert.IsNotNull(new Charset(characters, -1, 1));
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
                List<(char, int)> characters = new List<(char, int)>()
                {
                    ('0', 1)
                };

                Assert.IsNotNull(new Charset(characters, 1, 0));
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(new ArgumentException(Properties.Resources.E_LanguageModel_InvalidMaximumRepeatCount).Message, e.Message);
                throw;
            }
        }

        [TestMethod]
        public void ConstructorTest30()
        {
            List<(char, float)> characters = new List<(char, float)>()
            {
                ('0', 1.0f),
                ('A', 2.0f),
                ('z', 3.0f),
                ('?', 4.0f)
            };
            Charset charset = new Charset(characters, 0, 1);

            Assert.AreEqual(4, charset.Characters.Count);
            Assert.AreEqual(0, charset.MinRepeatCount);
            Assert.AreEqual(1, charset.MaxRepeatCount);
            Assert.IsTrue(charset.IsTail);
            Assert.IsNull(charset.Parent);

            foreach ((char ch, float frequency) in characters)
            {
                Assert.AreEqual(frequency / 10, charset.Characters[ch]);
            }
        }

        [TestMethod]
        [Description("Duplicate characters in charset - should sum frequencies.")]
        public void ConstructorTest301()
        {
            List<(char, float)> characters = new List<(char, float)>()
            {
                ('0', 1.0f),
                ('A', 2.0f),
                ('A', 1.0f),
            };
            Charset charset = new Charset(characters, 0, 1);

            Assert.AreEqual(2, charset.Characters.Count);
            Assert.AreEqual(0, charset.MinRepeatCount);
            Assert.AreEqual(1, charset.MaxRepeatCount);
            Assert.IsTrue(charset.IsTail);
            Assert.IsNull(charset.Parent);

            Assert.AreEqual(0.25f, charset.Characters['0']);
            Assert.AreEqual(0.75f, charset.Characters['A']);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        [Description("Invalid count in charset.")]
        public void ConstructorTest302()
        {
            try
            {
                List<(char, float)> characters = new List<(char, float)>()
                {
                    ('0', 0.0f)
                };

                Assert.IsNotNull(new Charset(characters, 0, 1));
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(
                    new ArgumentException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_LanguageModel_InvalidCharacterFrequency, 0.0f, '0')).Message,
                    e.Message);
                throw;
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest31()
        {
            Assert.IsNotNull(new Charset((IEnumerable<(char, float)>)null, 0, 1));
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void ConstructorTest32()
        {
            try
            {
                Assert.IsNotNull(new Charset(new List<(char, float)>(), 0, 1));
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(new ArgumentException(Properties.Resources.E_LanguageModel_EmptyCharset).Message, e.Message);
                throw;
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void ConstructorTest33()
        {
            try
            {
                List<(char, float)> characters = new List<(char, float)>()
                {
                    ('0', 1.0f)
                };

                Assert.IsNotNull(new Charset(characters, -1, 1));
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(new ArgumentException(Properties.Resources.E_LanguageModel_InvalidMinimumRepeatCount).Message, e.Message);
                throw;
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void ConstructorTest34()
        {
            try
            {
                List<(char, float)> characters = new List<(char, float)>()
                {
                    ('0', 1.0f)
                };

                Assert.IsNotNull(new Charset(characters, 1, 0));
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
            const string Characters = "0Az?";
            Charset charset = new Charset(new Charset(Characters, 0, 1));

            Assert.AreEqual(4, charset.Characters.Count);
            Assert.AreEqual(0, charset.MinRepeatCount);
            Assert.AreEqual(1, charset.MaxRepeatCount);
            Assert.IsTrue(charset.IsTail);
            Assert.IsNull(charset.Parent);

            foreach (char ch in Characters)
            {
                Assert.AreEqual(1.0f / Characters.Length, charset.Characters[ch]);
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void CopyConstructorTest1()
        {
            Assert.IsNotNull(new Charset(null));
        }

        [TestMethod]
        public void EqualsTest()
        {
            Charset charset1 = new Charset("0Az", 1, 2);

            Assert.IsFalse(charset1.Equals(null));
            Assert.IsTrue(charset1.Equals(charset1));
            Assert.IsTrue(charset1.Equals(new Charset("0Az", 1, 2)));
            Assert.IsFalse(charset1.Equals(new Charset("0A", 1, 2)));
            Assert.IsFalse(charset1.Equals(new Charset("0Az", 2, 2)));
            Assert.IsFalse(charset1.Equals(new Charset("0Az", 1, 1)));
        }

        [TestMethod]
        public void GetHashCodeTest0()
        {
            Charset charset = new Charset("0Az", 1, 2);
            Assert.AreEqual(1, charset.GetHashCode());
        }

        [TestMethod]
        public void GetHashCodeTest1()
        {
            Charset charset = Helpers.CreateClassWithPrivateConstructor<Charset>();
            Assert.AreEqual(1, charset.GetHashCode());
        }

        [TestMethod]
        public void CloneTest()
        {
            Charset charset1 = new Charset("0Az", 1, 2);
            Charset charset2 = charset1.Clone() as Charset;

            Assert.AreEqual(charset1, charset2);
            Assert.AreEqual(JsonConvert.SerializeObject(charset1), JsonConvert.SerializeObject(charset2));
        }

        [TestMethod]
        public void SerializeTest()
        {
            Charset charset1 = new Charset("0Az", 1, 2);
            string s1 = JsonConvert.SerializeObject(charset1);
            Charset charset2 = JsonConvert.DeserializeObject<Charset>(s1);
            string s2 = JsonConvert.SerializeObject(charset2);

            Assert.AreEqual(charset1, charset2);
            Assert.AreEqual(s1, s2);
        }

        [TestMethod]
        public void SaveToStringTest()
        {
            Charset charset1 = new Charset("0Az", 1, 2);
            string s1 = charset1.SaveToString();
            Charset charset2 = Charset.FromString(s1);

            Assert.AreEqual(charset1, charset2);
        }

        [TestMethod]
        public void SaveToMemoryTest()
        {
            Charset charset1 = new Charset("0Az", 1, 2);
            byte[] b1 = charset1.SaveToMemory();
            Charset charset2 = Charset.FromMemory(b1);

            Assert.AreEqual(charset1, charset2);
        }

        [TestMethod]
        public void SaveToFileTest()
        {
            string tempFileName = Path.GetTempFileName();
            try
            {
                Charset charset1 = new Charset("0Az", 1, 2);
                charset1.SaveToFile(tempFileName);
                Charset charset2 = Charset.FromFile(tempFileName);

                Assert.AreEqual(charset1, charset2);
            }
            finally
            {
                File.Delete(tempFileName);
            }
        }

        [TestMethod]
        public void InitialStateMethodTest0()
        {
            Charset charset = new Charset("0Az", 1, 2);
            Context.ContextState state = charset.InitialState as Context.ContextState;

            Assert.AreEqual((char)0, state.Char);
            Assert.IsFalse(state.WordEnd);
            Assert.IsFalse(state.ContextWordEnd);
            Assert.AreEqual(0, state.RepeatCount);
            Assert.AreEqual(0.0f, state.CharProbability);
            Assert.AreEqual(0.0f, state.WordEndProbability);
        }

        [TestMethod]
        public void InitialStateMethodTest1()
        {
            Charset charset = new Charset("0Az", 0, 2);
            Context.ContextState state = charset.InitialState as Context.ContextState;

            Assert.AreEqual((char)0, state.Char);
            Assert.IsFalse(state.WordEnd);
            Assert.IsTrue(state.ContextWordEnd);
            Assert.AreEqual(0, state.RepeatCount);
            Assert.AreEqual(0.0f, state.CharProbability);
            Assert.AreEqual(0.0f, state.WordEndProbability);
        }

        [TestMethod]
        [Description("Repetition count: 0 to 1.")]
        public void NextStatesTest0()
        {
            const string Characters = "AB";
            Charset charset = new Charset(Characters, 0, 1);
            State[] states = charset.InitialState.NextStates().Values.ToArray();

            Assert.AreEqual(2, states.Length);

            for (int i = 0; i < states.Length; i++)
            {
                Context.ContextState state = states[i] as Context.ContextState;

                Assert.AreEqual(Characters[i], state.Char);
                Assert.IsTrue(state.WordEnd);
                Assert.IsTrue(state.ContextWordEnd);
                Assert.AreEqual(1, state.RepeatCount);
                Assert.AreEqual(0.5f, state.CharProbability);
                Assert.AreEqual(1.0f, state.WordEndProbability);

                Assert.IsNull(state.NextStates());
            }
        }

        [TestMethod]
        [Description("Repetition count: 1 to 1.")]
        public void NextStatesTest1()
        {
            const string Characters = "AB";
            Charset charset = new Charset(Characters, 1, 1);
            State[] states = charset.InitialState.NextStates().Values.ToArray();

            Assert.AreEqual(2, states.Length);

            for (int i = 0; i < states.Length; i++)
            {
                Context.ContextState state = states[i] as Context.ContextState;

                Assert.AreEqual(Characters[i], state.Char);
                Assert.IsTrue(state.WordEnd);
                Assert.IsTrue(state.ContextWordEnd);
                Assert.AreEqual(1, state.RepeatCount);
                Assert.AreEqual(0.5f, state.CharProbability);
                Assert.AreEqual(1.0f, state.WordEndProbability);

                Assert.IsNull(state.NextStates());
            }
        }

        [TestMethod]
        [Description("Repetition count: 1 to 2.")]
        public void NextStatesTest2()
        {
            const string Characters = "AB";
            Charset charset = new Charset(Characters, 1, 2);
            State[] states = charset.InitialState.NextStates().Values.ToArray();

            Assert.AreEqual(2, states.Length);

            for (int i = 0; i < states.Length; i++)
            {
                Context.ContextState state = states[i] as Context.ContextState;

                Assert.AreEqual(Characters[i], state.Char);
                Assert.IsTrue(state.WordEnd);
                Assert.IsTrue(state.ContextWordEnd);
                Assert.AreEqual(1, state.RepeatCount);
                Assert.AreEqual(0.5f, state.CharProbability);
                Assert.AreEqual(1.0f, state.WordEndProbability);

                State[] states2 = state.NextStates().Values.ToArray();

                Assert.AreEqual(2, states.Length);

                for (int j = 0; j < states2.Length; j++)
                {
                    Context.ContextState state2 = states2[j] as Context.ContextState;

                    Assert.AreEqual(Characters[j], state2.Char);
                    Assert.IsTrue(state2.WordEnd);
                    Assert.IsTrue(state2.ContextWordEnd);
                    Assert.AreEqual(2, state2.RepeatCount);
                    Assert.AreEqual(0.5f, state2.CharProbability);
                    Assert.AreEqual(1.0f, state2.WordEndProbability);

                    Assert.IsNull(state2.NextStates());
                }
            }
        }

        [TestMethod]
        [Description("Repetition count: 2 to 2.")]
        public void NextStatesTest3()
        {
            const string Characters = "AB";
            Charset charset = new Charset(Characters, 2, 2);
            State[] states = charset.InitialState.NextStates().Values.ToArray();

            Assert.AreEqual(2, states.Length);

            for (int i = 0; i < states.Length; i++)
            {
                Context.ContextState state = states[i] as Context.ContextState;

                Assert.AreEqual(Characters[i], state.Char);
                Assert.IsFalse(state.WordEnd);
                Assert.IsFalse(state.ContextWordEnd);
                Assert.AreEqual(1, state.RepeatCount);
                Assert.AreEqual(0.5f, state.CharProbability);
                Assert.AreEqual(1.0f, state.WordEndProbability);

                State[] states2 = state.NextStates().Values.ToArray();

                Assert.AreEqual(2, states.Length);

                for (int j = 0; j < states2.Length; j++)
                {
                    Context.ContextState state2 = states2[j] as Context.ContextState;

                    Assert.AreEqual(Characters[j], state2.Char);
                    Assert.IsTrue(state2.WordEnd);
                    Assert.IsTrue(state2.ContextWordEnd);
                    Assert.AreEqual(2, state2.RepeatCount);
                    Assert.AreEqual(0.5f, state2.CharProbability);
                    Assert.AreEqual(1.0f, state2.WordEndProbability);

                    Assert.IsNull(state2.NextStates());
                }
            }
        }

        [TestMethod]
        [Description("Repetition count: 1 to 1.")]
        public void EnumerateTest1()
        {
            Charset charset = new Charset("AB ", 1, 1);

            (string s, float f)[] expected = new (string, float)[]
            {
                ("A", 0.0f),
                ("B", 0.0f),
            };

            (string s, float f)[] actual = charset.InitialState.Enumerate().ToArray();

            CollectionAssert.AreEqual(expected.Select(x => x.s).ToArray(), actual.Select(x => x.s).ToArray());
        }

        [TestMethod]
        [Description("Repetition count: 1 to 2.")]
        public void EnumerateTest2()
        {
            Charset charset = new Charset("AB ", 1, 2);

            (string s, float f)[] expected = new (string, float)[]
            {
                ("A", 0.0f),
                ("AA", 0.0f),
                ("AB", 0.0f),
                ("B", 0.0f),
                ("BA", 0.0f),
                ("BB", 0.0f),
            };

            (string s, float f)[] actual = charset.InitialState.Enumerate().ToArray();

            CollectionAssert.AreEqual(expected.Select(x => x.s).ToArray(), actual.Select(x => x.s).ToArray());
        }

        [TestMethod]
        [Description("Repetition count: 2 to 2.")]
        public void EnumerateTest3()
        {
            Charset charset = new Charset("AB ", 2, 2);

            (string s, float f)[] expected = new (string, float)[]
            {
                ("AA", 0.0f),
                ("AB", 0.0f),
                ("BA", 0.0f),
                ("BB", 0.0f),
            };

            (string s, float f)[] actual = charset.InitialState.Enumerate().ToArray();

            CollectionAssert.AreEqual(expected.Select(x => x.s).ToArray(), actual.Select(x => x.s).ToArray());
        }

        [TestMethod]
        [Description("Repetition count: 1 to 3.")]
        public void EnumerateTest4()
        {
            Charset charset = new Charset("AB ", 1, 3);

            (string s, float f)[] expected = new (string, float)[]
            {
                ("A", 0.0f),
                ("A A", 0.0f),
                ("A B", 0.0f),
                ("AA", 0.0f),
                ("AAA", 0.0f),
                ("AAB", 0.0f),
                ("AB", 0.0f),
                ("ABA", 0.0f),
                ("ABB", 0.0f),
                ("B", 0.0f),
                ("B A", 0.0f),
                ("B B", 0.0f),
                ("BA", 0.0f),
                ("BAA", 0.0f),
                ("BAB", 0.0f),
                ("BB", 0.0f),
                ("BBA", 0.0f),
                ("BBB", 0.0f),
            };

            (string s, float f)[] actual = charset.InitialState.Enumerate().ToArray();

            CollectionAssert.AreEqual(expected.Select(x => x.s).ToArray(), actual.Select(x => x.s).ToArray());
        }
    }
}
