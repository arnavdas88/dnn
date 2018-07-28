// -----------------------------------------------------------------------
// <copyright file="StringExtensions.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace System
{
    using System.Collections.Generic;

    /// <summary>
    /// Provides extension methods for the <see cref="string"/> class.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Qualifies string on both ends with the specified character qualifier.
        /// </summary>
        /// <param name="s">The string to modify.</param>
        /// <param name="qualifier">The character to qualify the string with.</param>
        /// <returns>The modified string.</returns>
        /// <remarks>
        /// If <c>s</c> contains <c>qualifier</c>, the method duplicates each occurrence of <c>qualifier</c> in <c>s</c>.
        /// </remarks>
        public static string Qualify(this string s, char qualifier)
        {
            return s.Qualify(qualifier.ToString());
        }

        /// <summary>
        /// Qualifies string on both ends with the specified string qualifier.
        /// </summary>
        /// <param name="s">The string to modify.</param>
        /// <param name="qualifier">The string to qualify the string with.</param>
        /// <returns>The modified string.</returns>
        /// <remarks>
        /// If <c>s</c> contains <c>qualifier</c>, the method duplicates each occurrence of <c>qualifier</c> in <c>s</c>.
        /// </remarks>
        public static string Qualify(this string s, string qualifier = "\"")
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Concat(qualifier, qualifier);
            }

            if (!s.Contains(qualifier))
            {
                return string.Concat(qualifier, s, qualifier);
            }

            return string.Concat(qualifier, s.Replace(qualifier, string.Concat(qualifier, qualifier)), qualifier);
        }

        /// <summary>
        /// Check is string has specified qualifier string on both ends.
        /// </summary>
        /// <param name="s">The string to check.</param>
        /// <param name="qualifier">The qualifier character to find.</param>
        /// <returns><b>True</b> is string is qualified; otherwise, <b>false</b>.</returns>
        public static bool IsQualified(this string s, char qualifier)
        {
            return s.IsQualified(qualifier.ToString());
        }

        /// <summary>
        /// Check is string has specified qualifier on both ends.
        /// </summary>
        /// <param name="s">The string to check.</param>
        /// <param name="qualifier">The string that contains qualifier to find.</param>
        /// <returns><b>True</b> is string is qualified; otherwise, <b>false</b>.</returns>
        public static bool IsQualified(this string s, string qualifier)
        {
            if (string.IsNullOrEmpty(s) || string.IsNullOrEmpty(qualifier))
            {
                return false;
            }

            return s != qualifier &&
                   s.StartsWith(qualifier, StringComparison.Ordinal) &&
                   s.EndsWith(qualifier, StringComparison.Ordinal);
        }

        /// <summary>
        /// Removes starting end ending qualifier from the string.
        /// </summary>
        /// <param name="s">The string to modify.</param>
        /// <param name="qualifiers">The qualifiers to remove.</param>
        /// <returns>The modified string.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para><c>qualifiers</c> is <b>null</b>.</para>
        /// </exception>
        public static string Unqualify(this string s, params char[] qualifiers)
        {
            if (qualifiers == null || qualifiers.Length == 0)
            {
                throw new ArgumentNullException(nameof(qualifiers));
            }

            if (!string.IsNullOrEmpty(s))
            {
                int length = s.Length;
                if (length >= 2)
                {
                    foreach (char qualifier in qualifiers)
                    {
                        if (s[0] == qualifier && s[length - 1] == qualifier)
                        {
                            return s.Substring(1, length - 2)
                                    .Replace(string.Concat(qualifier, qualifier), qualifier.ToString());
                        }
                    }
                }
            }

            return s;
        }

        /// <summary>
        /// Returns a zero-based, one-dimensional array containing a specified number of substrings that may be surrounded by a qualifier.
        /// </summary>
        /// <param name="s"><b>String</b> expression containing substrings, qualifiers, and delimiters.</param>
        /// <param name="delimiter">Any single character used to identify substring limits.</param>
        /// <param name="qualifier">Any single character used to qualify substrings that include delimiter.</param>
        /// <returns>
        /// If <c>s</c> is a zero-length string (""), the method returns a single-element array containing a zero-length string.
        /// If <c>delimiter</c> does not appear anywhere in <c>s</c>, the method returns a single-element array containing the entire <c>s</c> string.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>s</c> is <b>null</b>.
        /// </exception>
        public static string[] SplitQualified(this string s, char delimiter, char qualifier = '\"')
        {
            if (s == null)
            {
                throw new ArgumentNullException(nameof(s));
            }

            if (string.IsNullOrEmpty(s))
            {
                return new string[0];
            }

            if (s.IndexOf(delimiter) == -1)
            {
                return new string[] { s };
            }

            if (s.IndexOf(qualifier) == -1)
            {
                return s.Split(delimiter);
            }

            List<string> result = new List<string>();
            for (int i = 0, length = s.Length; i < length; i++)
            {
                if (s[i] == qualifier)
                {
                    // substring starts with qualifier - look for next qualifier
                    int pos = s.IndexOf(qualifier, i + 1);
                    while (pos != -1)
                    {
                        // ending qualifier is the last character in the string ot it is followed by separator
                        if (pos + 1 == length || s[pos + 1] == delimiter)
                        {
                            break;
                        }

                        pos = s.IndexOf(qualifier, pos + 1);
                    }

                    if (pos == -1)
                    {
                        // ending qualifier is not found - parse the remainder of the string
                        result.AddRange(s.Substring(i).Split(delimiter));
                        break;
                    }
                    else
                    {
                        result.Add(s.Substring(i, pos - i + 1).Unqualify(qualifier));
                        i = pos;

                        // skip next delimiter
                        if (i + 1 == length || s[i + 1] == delimiter)
                        {
                            i++;
                        }
                    }
                }
                else
                {
                    // substring does not start with qualifier - look for next delimiter
                    int pos = s.IndexOf(delimiter, i);
                    if (pos == -1)
                    {
                        result.Add(s.Substring(i));
                        break;
                    }
                    else
                    {
                        result.Add(s.Substring(i, pos - i));
                        i = pos;
                    }
                }
            }

            return result.ToArray();
        }
    }
}
