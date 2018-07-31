// -----------------------------------------------------------------------
// <copyright file="RegexParser.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.LanguageModel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;

    internal class RegexParser
    {
        public static Context Parse(string regex, CultureInfo cultureInfo)
        {
            if (regex == null)
            {
                throw new ArgumentNullException(nameof(regex));
            }

            return Token.Parse(regex, cultureInfo).ToContext();
        }

        private abstract class Token
        {
            private const char Escape = '\\';
            private const char GroupBegin = '(';
            private const char GroupEnd = ')';
            private const char GroupSeparator = '|';
            private const char RangeBegin = '[';
            private const char RangeEnd = ']';
            private const char RangeSeparator = '-';

            public int MinCount { get; set; } = 1;

            public int MaxCount { get; set; } = 1;

            public bool IsSingle => this.MinCount == 1 && this.MaxCount == 1;

            public static Token Parse(string s, CultureInfo cultureInfo)
            {
                if (string.IsNullOrEmpty(s))
                {
                    return new NullToken();
                }

                int length = s.Length;

                int startingPos = 0;
                return ParseUnion(ref startingPos, false).Optimize();

                Token ParseUnion(ref int pos, bool mustClose)
                {
                    UnionToken unionToken = new UnionToken();
                    ConcatenationToken concatenationToken = null;

                    int startpos = pos;
                    bool closed = false;
                    while (pos < length)
                    {
                        Token token = null;

                        if (TryMatchCharacter(ref pos, Token.GroupBegin))
                        {
                            // parse group
                            token = ParseUnion(ref pos, true);
                        }
                        else if (mustClose && TryMatchCharacter(ref pos, Token.GroupEnd))
                        {
                            closed = true;
                            break;
                        }
                        else if (TryMatchCharacter(ref pos, Token.GroupSeparator))
                        {
                            unionToken.Tokens.Add(concatenationToken as Token ?? new NullToken());
                            concatenationToken = null;
                            continue;
                        }
                        else if (TryMatchCharacter(ref pos, Token.RangeBegin))
                        {
                            // parse range
                            token = ParseRange(ref pos);
                        }
                        else if (TryParseCharacterClass(ref pos, out string characters))
                        {
                            token = new CharsetToken(characters);
                        }

                        int savedpos = pos;
                        if (TryParseQuantifier(ref pos, out int minlen, out int maxlen))
                        {
                            if (token == null)
                            {
                                throw new ArgumentException(string.Format(
                                    CultureInfo.InvariantCulture,
                                    Properties.Resources.E_InvalidRegex_TokenNotQuantifiable,
                                    s,
                                    savedpos));
                            }

                            token.MinCount = minlen;
                            token.MaxCount = maxlen;
                        }
                        else if (token == null && TryParseCharacter(ref pos, out char ch, out bool escaped))
                        {
                            token = new CharacterToken()
                            {
                                Character = ch,
                            };

                            if (TryParseQuantifier(ref pos, out minlen, out maxlen))
                            {
                                token.MinCount = minlen;
                                token.MaxCount = maxlen;
                            }
                        }

                        if (token != null)
                        {
                            if (concatenationToken == null)
                            {
                                concatenationToken = new ConcatenationToken();
                            }

                            concatenationToken.Tokens.Add(token);
                        }
                    }

                    if (mustClose && !closed)
                    {
                        throw new ArgumentException(string.Format(
                            CultureInfo.InvariantCulture,
                            Properties.Resources.E_InvalidRegex_GroupMissingClosingBracket,
                            s,
                            startpos - 1));
                    }

                    unionToken.Tokens.Add(concatenationToken as Token ?? new NullToken());

                    return unionToken;
                }

                Token ParseRange(ref int pos)
                {
                    CharsetToken token = new CharsetToken();

                    int startpos = pos;
                    bool closed = false;
                    bool separator = false;
                    char last = (char)0;
                    while (pos < length)
                    {
                        if (TryMatchCharacter(ref pos, Token.RangeEnd))
                        {
                            closed = true;
                            break;
                        }
                        else if (!separator && TryMatchCharacter(ref pos, Token.RangeSeparator))
                        {
                            if (last == (char)0)
                            {
                                token.Characters.Add(Token.RangeSeparator);
                            }
                            else
                            {
                                separator = true;
                            }
                        }
                        else if (TryParseCharacterClass(ref pos, out string characters))
                        {
                            if (last != (char)0)
                            {
                                if (separator)
                                {
                                    throw new ArgumentException(string.Format(
                                        CultureInfo.InvariantCulture,
                                        Properties.Resources.E_InvalidRegex_RangeWithSequence,
                                        s,
                                        pos - 3));
                                }

                                // add last character
                                token.Characters.Add(last);
                                last = (char)0;
                            }

                            token.Characters.UnionWith(characters);
                        }
                        else if (TryParseCharacter(ref pos, out char ch, out bool escaped))
                        {
                            if (last != (char)0)
                            {
                                if (separator)
                                {
                                    if (ch < last)
                                    {
                                        throw new ArgumentException(string.Format(
                                            CultureInfo.InvariantCulture,
                                            Properties.Resources.E_InvalidRegex_RangeOutOfOrder,
                                            s,
                                            pos - 2));
                                    }

                                    for (; last <= ch; last++)
                                    {
                                        token.Characters.Add(last);
                                    }

                                    // start anew
                                    last = (char)0;
                                    separator = false;
                                    continue;
                                }
                                else
                                {
                                    // just add last character
                                    token.Characters.Add(last);
                                }
                            }
                            else if (separator)
                            {
                                // something went wrong
                                throw new InvalidOperationException();
                            }

                            last = ch;
                            separator = false;
                        }
                    }

                    if (!closed)
                    {
                        throw new ArgumentException(string.Format(
                            CultureInfo.InvariantCulture,
                            Properties.Resources.E_InvalidRegex_RangeMissingClosingBracket,
                            s,
                            startpos - 1));
                    }
                    else
                    {
                        if (last != (char)0)
                        {
                            // add last character
                            token.Characters.Add(last);
                        }

                        if (separator)
                        {
                            // separator without range end - treat as character
                            token.Characters.Add(Token.RangeSeparator);
                        }
                    }

                    if (token.Characters.Count == 0)
                    {
                        throw new ArgumentException(string.Format(
                            CultureInfo.InvariantCulture,
                            Properties.Resources.E_InvalidRegex_RangeIsEmpty,
                            s,
                            startpos - 1));
                    }

                    return token;
                }

                bool TryMatchCharacter(ref int pos, char ch)
                {
                    if (pos >= length || s[pos] != ch)
                    {
                        return false;
                    }

                    pos++;
                    return true;
                }

                bool TryParseCharacter(ref int pos, out char ch, out bool escaped)
                {
                    ch = char.MinValue;
                    escaped = false;

                    if (pos >= length)
                    {
                        return false;
                    }

                    ch = s[pos++];

                    if (ch == Token.Escape)
                    {
                        if (pos >= length)
                        {
                            throw new ArgumentException(string.Format(
                                CultureInfo.InvariantCulture,
                                Properties.Resources.E_InvalidRegex_MayNotEndWithBackslash,
                                s,
                                pos - 1));
                        }

                        ch = s[pos++];
                        escaped = true;
                    }

                    return true;
                }

                bool TryParseCharacterClass(ref int pos, out string characters)
                {
                    characters = null;

                    int savedpos = pos;
                    if (!TryMatchCharacter(ref pos, '\\'))
                    {
                        return false;
                    }

                    if (TryMatchCharacter(ref pos, 's'))
                    {
                        // white space
                        characters = " ";
                        return true;
                    }
                    else if (TryMatchCharacter(ref pos, 'd'))
                    {
                        // digit
                        characters = "0123456789";
                        return true;
                    }
                    else if (TryMatchCharacter(ref pos, 'x'))
                    {
                        // hexade­cimal digit
                        characters = "0123456789ABCDEF";
                        return true;
                    }
                    else if (TryMatchCharacter(ref pos, 'O'))
                    {
                        // octal digit
                        characters = "01234567";
                        return true;
                    }
                    else if (TryMatchCharacter(ref pos, 'w'))
                    {
                        // word
                        if (string.IsNullOrEmpty(cultureInfo.Name) || // invariant culture - default to English
                            cultureInfo.Name.StartsWith("en", StringComparison.Ordinal))
                        {
                            // English
                            characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ";
                        }
                        else if (cultureInfo.Name.StartsWith("ru", StringComparison.Ordinal))
                        {
                            // Russian
                            characters = "АБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ0123456789 ";
                        }
                        else
                        {
                            throw new ArgumentException(string.Format(
                                CultureInfo.InvariantCulture,
                                Properties.Resources.E_InvalidRegex_UnsupportedCulture,
                                cultureInfo.Name,
                                s,
                                savedpos));
                        }

                        return true;
                    }

                    pos = savedpos;
                    return false;
                }

                bool TryParseNumber(ref int pos, out int value)
                {
                    value = 0;

                    if (pos < length)
                    {
                        char ch = s[pos];
                        if (char.IsDigit(ch))
                        {
                            value = (int)(ch - '0');
                            pos++;

                            while (pos < length)
                            {
                                ch = s[pos];
                                if (!char.IsDigit(ch))
                                {
                                    break;
                                }

                                value = (value * 10) + (int)(ch - '0');
                                pos++;
                            }

                            return true;
                        }
                    }

                    return false;
                }

                bool TryParseQuantifier(ref int pos, out int minlen, out int maxlen)
                {
                    minlen = maxlen = 1;

                    int savedpos = pos;
                    if (TryMatchCharacter(ref pos, '*'))
                    {
                        // zero or more
                        minlen = 0;
                        maxlen = int.MaxValue;
                        return true;
                    }
                    else if (TryMatchCharacter(ref pos, '+'))
                    {
                        // one or more
                        minlen = 1;
                        maxlen = int.MaxValue;
                        return true;
                    }
                    else if (TryMatchCharacter(ref pos, '?'))
                    {
                        // 0 or 1
                        minlen = 0;
                        maxlen = 1;
                        return true;
                    }
                    else if (TryMatchCharacter(ref pos, '{'))
                    {
                        // range
                        bool hasNumber = false;
                        if (TryParseNumber(ref pos, out minlen))
                        {
                            hasNumber = true;
                        }
                        else
                        {
                            minlen = 0;
                        }

                        if (TryMatchCharacter(ref pos, ','))
                        {
                            // from N1 to N2
                            if (TryParseNumber(ref pos, out maxlen))
                            {
                                hasNumber = true;
                            }
                            else
                            {
                                maxlen = int.MaxValue;
                            }
                        }
                        else
                        {
                            // exactly N
                            maxlen = minlen;
                        }

                        // must have a closing bracket
                        if (hasNumber && TryMatchCharacter(ref pos, '}'))
                        {
                            return true;
                        }
                    }

                    pos = savedpos;
                    minlen = maxlen = 1;
                    return false;
                }
            }

            public abstract Context ToContext();

            public virtual Token Optimize()
            {
                return this;
            }
        }

        private class NullToken : Token
        {
            public override Context ToContext()
            {
                return new NullContext();
            }
        }

        private class CharacterToken : Token
        {
            public char Character { get; set; }

            public override Context ToContext()
            {
                return new Charset(new char[] { this.Character }, this.MinCount, this.MaxCount);
            }
        }

        private class CharsetToken : Token
        {
            public CharsetToken()
            {
            }

            public CharsetToken(string characters)
            {
                this.Characters.UnionWith(characters);
            }

            public ISet<char> Characters { get; } = new HashSet<char>();

            public override Context ToContext()
            {
                return new Charset(this.Characters, this.MinCount, this.MaxCount);
            }

            public override Token Optimize()
            {
                return this.Characters.Count == 1 ?
                    new CharacterToken()
                    {
                        Character = this.Characters.First(),
                        MinCount = this.MinCount,
                        MaxCount = this.MaxCount,
                    }
                    : (Token)this;
            }
        }

        private class WordToken : Token
        {
            public string Text { get; set; }

            public override Context ToContext()
            {
                return new Vocabulary(new string[] { this.Text }, this.MinCount, this.MaxCount);
            }
        }

        private class WordsToken : Token
        {
            public ISet<string> Texts { get; } = new HashSet<string>();

            public override Context ToContext()
            {
                return new Vocabulary(this.Texts, this.MinCount, this.MaxCount);
            }

            public override Token Optimize()
            {
                return this.Texts.Count == 1 ?
                    new WordToken()
                    {
                        Text = this.Texts.First(),
                        MinCount = this.MinCount,
                        MaxCount = this.MaxCount,
                    }
                    : (Token)this;
            }
        }

        private abstract class CompositeToken : Token
        {
            public IList<Token> Tokens { get; } = new List<Token>();

            public override Token Optimize()
            {
                IList<Token> tkns = this.Tokens;
                for (int i = 0, ii = tkns.Count; i < ii; i++)
                {
                    tkns[i] = tkns[i].Optimize();
                }

                return this;
            }
        }

        private class UnionToken : CompositeToken
        {
            public override Context ToContext()
            {
                GraphContext graph = new GraphContext();

                IList<Token> tkns = this.Tokens;
                for (int i = 0, ii = tkns.Count; i < ii; i++)
                {
                    graph.Graph.AddVertex(tkns[i].ToContext());
                }

                return graph;
            }

            public override Token Optimize()
            {
                base.Optimize();

                IList<Token> tkns = this.Tokens;
                for (int i = 0; i + 1 < tkns.Count; i++)
                {
                    for (int j = i + 1; j < tkns.Count; j++)
                    {
                        if (UnionToken.TryUnion(tkns[i], tkns[j], out Token unionToken))
                        {
                            tkns[i] = unionToken;
                            tkns.RemoveAt(j--);
                        }
                    }
                }

                return tkns.Count == 1 ? tkns[0] : this;
            }

            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Many simple combination of four types of Context.")]
            private static bool TryUnion(Token token1, Token token2, out Token unionToken)
            {
                unionToken = null;

                if (token1.MinCount != token2.MinCount ||
                    token1.MaxCount != token2.MaxCount)
                {
                    return false;
                }

                CharacterToken characterToken2 = token2 as CharacterToken;
                CharsetToken charsetToken2 = token2 as CharsetToken;
                WordToken wordToken2 = token2 as WordToken;
                WordsToken wordsToken2 = token2 as WordsToken;

                if (token1 is CharacterToken characterToken1)
                {
                    if (characterToken2 != null)
                    {
                        CharsetToken charsetToken = new CharsetToken()
                        {
                            MinCount = token1.MinCount,
                            MaxCount = token2.MaxCount,
                        };

                        charsetToken.Characters.Add(characterToken1.Character);
                        charsetToken.Characters.Add(characterToken2.Character);

                        unionToken = charsetToken;
                        return true;
                    }
                    else if (charsetToken2 != null)
                    {
                        charsetToken2.Characters.Add(characterToken1.Character);

                        unionToken = charsetToken2;
                        return true;
                    }
                    else if (wordToken2 != null)
                    {
                        WordsToken wordsToken = new WordsToken()
                        {
                            MinCount = token1.MinCount,
                            MaxCount = token2.MaxCount,
                        };

                        wordsToken.Texts.Add(new string(characterToken1.Character, 1));
                        wordsToken.Texts.Add(wordToken2.Text);

                        unionToken = wordsToken;
                        return true;
                    }
                    else if (wordsToken2 != null)
                    {
                        wordsToken2.Texts.Add(new string(characterToken1.Character, 1));

                        unionToken = wordsToken2;
                        return true;
                    }
                }
                else if (token1 is CharsetToken charsetToken1)
                {
                    if (characterToken2 != null)
                    {
                        charsetToken1.Characters.Add(characterToken2.Character);

                        unionToken = charsetToken1;
                        return true;
                    }
                    else if (charsetToken2 != null)
                    {
                        charsetToken1.Characters.UnionWith(charsetToken2.Characters);

                        unionToken = charsetToken1;
                        return true;
                    }
                    else if (wordToken2 != null)
                    {
                        WordsToken wordsToken = new WordsToken()
                        {
                            MinCount = token1.MinCount,
                            MaxCount = token2.MaxCount,
                        };

                        wordsToken.Texts.UnionWith(charsetToken1.Characters.Select(x => new string(x, 1)));
                        wordsToken.Texts.Add(wordToken2.Text);

                        unionToken = wordsToken;
                        return true;
                    }
                    else if (wordsToken2 != null)
                    {
                        wordsToken2.Texts.UnionWith(charsetToken1.Characters.Select(x => new string(x, 1)));

                        unionToken = wordsToken2;
                        return true;
                    }
                }
                else if (token1 is WordToken wordToken1)
                {
                    if (characterToken2 != null)
                    {
                        WordsToken wordsToken = new WordsToken()
                        {
                            MinCount = token1.MinCount,
                            MaxCount = token2.MaxCount,
                        };

                        wordsToken.Texts.Add(wordToken1.Text);
                        wordsToken.Texts.Add(new string(characterToken2.Character, 1));

                        unionToken = wordsToken;
                        return true;
                    }
                    else if (charsetToken2 != null)
                    {
                        WordsToken wordsToken = new WordsToken()
                        {
                            MinCount = token1.MinCount,
                            MaxCount = token2.MaxCount,
                        };

                        wordsToken.Texts.Add(wordToken1.Text);
                        wordsToken.Texts.UnionWith(charsetToken2.Characters.Select(x => new string(x, 1)));

                        unionToken = wordsToken;
                        return true;
                    }
                    else if (wordToken2 != null)
                    {
                        WordsToken wordsToken = new WordsToken()
                        {
                            MinCount = token1.MinCount,
                            MaxCount = token2.MaxCount,
                        };

                        wordsToken.Texts.Add(wordToken1.Text);
                        wordsToken.Texts.Add(wordToken2.Text);

                        unionToken = wordsToken;
                        return true;
                    }
                    else if (wordsToken2 != null)
                    {
                        wordsToken2.Texts.Add(wordToken1.Text);

                        unionToken = wordsToken2;
                        return true;
                    }
                }
                else if (token1 is WordsToken wordsToken1)
                {
                    if (characterToken2 != null)
                    {
                        wordsToken1.Texts.Add(new string(characterToken2.Character, 1));

                        unionToken = wordsToken1;
                        return true;
                    }
                    else if (charsetToken2 != null)
                    {
                        wordsToken1.Texts.UnionWith(charsetToken2.Characters.Select(x => new string(x, 1)));

                        unionToken = wordsToken1;
                        return true;
                    }
                    else if (wordToken2 != null)
                    {
                        wordsToken1.Texts.Add(wordToken2.Text);

                        unionToken = wordsToken1;
                        return true;
                    }
                    else if (wordsToken2 != null)
                    {
                        wordsToken1.Texts.UnionWith(wordsToken2.Texts);

                        unionToken = wordsToken1;
                        return true;
                    }
                }

                return false;
            }
        }

        private class ConcatenationToken : CompositeToken
        {
            public override Context ToContext()
            {
                GraphContext graph = new GraphContext();
                Context last = null;

                IList<Token> tkns = this.Tokens;
                for (int i = 0, ii = tkns.Count; i < ii; i++)
                {
                    Context context = tkns[i].ToContext();
                    if (last == null)
                    {
                        graph.Graph.AddVertex(context);
                    }
                    else
                    {
                        graph.Graph.AddEdge(last, context);
                    }

                    last = context;
                }

                return graph;
            }

            public override Token Optimize()
            {
                base.Optimize();

                IList<Token> tkns = this.Tokens;
                for (int i = 0; i + 1 < tkns.Count; i++)
                {
                    if (ConcatenationToken.TryConcat(tkns[i], tkns[i + 1], out Token unionToken))
                    {
                        tkns[i] = unionToken;
                        tkns.RemoveAt(i + 1);
                        i--;
                    }
                }

                return tkns.Count == 1 ? tkns[0] : this;
            }

            private static bool TryConcat(Token token1, Token token2, out Token unionToken)
            {
                unionToken = null;

                if (!token1.IsSingle || !token2.IsSingle)
                {
                    return false;
                }

                CharacterToken characterToken2 = token2 as CharacterToken;
                WordToken wordToken2 = token2 as WordToken;

                if (token1 is CharacterToken characterToken1)
                {
                    if (characterToken2 != null)
                    {
                        unionToken = new WordToken()
                        {
                            Text = new string(new char[] { characterToken1.Character, characterToken2.Character }),
                        };
                        return true;
                    }
                    else if (wordToken2 != null)
                    {
                        unionToken = new WordToken()
                        {
                            Text = characterToken1.Character + wordToken2.Text,
                        };
                        return true;
                    }
                }
                else if (token1 is WordToken wordToken1)
                {
                    if (characterToken2 != null)
                    {
                        wordToken1.Text += characterToken2.Character;

                        unionToken = wordToken1;
                        return true;
                    }
                    else if (wordToken2 != null)
                    {
                        wordToken1.Text += wordToken2.Text;

                        unionToken = wordToken1;
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
