﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;

namespace AE.Net.Mail {
    internal static class Utilities {
        internal static string DecodeQuotedPrintable(string value, Encoding encoding = null) {
            if (encoding == null) {
                encoding = System.Text.Encoding.UTF8;
            }

            value = Regex.Replace(value, @"\=[\r\n]+", string.Empty, RegexOptions.Singleline);
            var matches = Regex.Matches(value, @"\=[0-9A-F]{2}");
            foreach (var match in matches.Cast<Match>().Reverse()) {
                int ascii = int.Parse(match.Value.Substring(1), System.Globalization.NumberStyles.HexNumber);

                //http://stackoverflow.com/questions/1318933/c-sharp-int-to-byte
                byte[] result = BitConverter.GetBytes(ascii);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(result);

                value = value.Substring(0, match.Index) + encoding.GetString(result) + value.Substring(match.Index + match.Length);
            }
            return value;
        }

        internal static string DecodeBase64(string data, Encoding encoding = null) {
            var bytes = Convert.FromBase64String(data);
            return (encoding ?? System.Text.Encoding.UTF8).GetString(bytes);
        }

        #region OpenPOP.NET
        internal static string DecodeWords(string encodedWords) {
            if (string.IsNullOrEmpty(encodedWords))
                return string.Empty;

            string decodedWords = encodedWords;

            // Notice that RFC2231 redefines the BNF to
            // encoded-word := "=?" charset ["*" language] "?" encoded-text "?="
            // but no usage of this BNF have been spotted yet. It is here to
            // ease debugging if such a case is discovered.

            // This is the regex that should fit the BNF
            // RFC Says that NO WHITESPACE is allowed in this encoding, but there are examples
            // where whitespace is there, and therefore this regex allows for such.
            const string strRegEx = @"\=\?(?<Charset>\S+?)\?(?<Encoding>\w)\?(?<Content>.+?)\?\=";
            // \w	Matches any word character including underscore. Equivalent to "[A-Za-z0-9_]".
            // \S	Matches any nonwhite space character. Equivalent to "[^ \f\n\r\t\v]".
            // +?   non-gready equivalent to +
            // (?<NAME>REGEX) is a named group with name NAME and regular expression REGEX

            var matches = Regex.Matches(encodedWords, strRegEx);
            foreach (Match match in matches) {
                // If this match was not a success, we should not use it
                if (!match.Success) continue;

                string fullMatchValue = match.Value;

                string encodedText = match.Groups["Content"].Value;
                string encoding = match.Groups["Encoding"].Value;
                string charset = match.Groups["Charset"].Value;

                // Get the encoding which corrosponds to the character set
                Encoding charsetEncoding = ParseCharsetToEncoding(charset);

                // Store decoded text here when done
                string decodedText;

                // Encoding may also be written in lowercase
                switch (encoding.ToUpperInvariant()) {
                    // RFC:
                    // The "B" encoding is identical to the "BASE64" 
                    // encoding defined by RFC 2045.
                    // http://tools.ietf.org/html/rfc2045#section-6.8
                    case "B":
                        decodedText = DecodeBase64(encodedText, charsetEncoding);
                        break;

                    // RFC:
                    // The "Q" encoding is similar to the "Quoted-Printable" content-
                    // transfer-encoding defined in RFC 2045.
                    // There are more details to this. Please check
                    // http://tools.ietf.org/html/rfc2047#section-4.2
                    // 
                    case "Q":
                        decodedText = DecodeQuotedPrintable(encodedText, charsetEncoding);
                        break;

                    default:
                        throw new ArgumentException("The encoding " + encoding + " was not recognized");
                }

                // Repalce our encoded value with our decoded value
                decodedWords = decodedWords.Replace(fullMatchValue, decodedText);
            }

            return decodedWords;
        }

        //http://www.opensourcejavaphp.net/csharp/openpopdotnet/HeaderFieldParser.cs.html
        /// Parse a character set into an encoding.
        /// </summary>
        /// <param name="characterSet">The character set to parse</param>
        /// <returns>An encoding which corresponds to the character set</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="characterSet"/> is <see langword="null"/></exception>
        public static Encoding ParseCharsetToEncoding(string characterSet) {
            if (string.IsNullOrEmpty(characterSet))
                return null;

            string charSetUpper = characterSet.ToUpperInvariant();
            if (charSetUpper.Contains("WINDOWS") || charSetUpper.Contains("CP")) {
                // It seems the character set contains an codepage value, which we should use to parse the encoding
                charSetUpper = charSetUpper.Replace("CP", ""); // Remove cp
                charSetUpper = charSetUpper.Replace("WINDOWS", ""); // Remove windows
                charSetUpper = charSetUpper.Replace("-", ""); // Remove - which could be used as cp-1554

                // Now we hope the only thing left in the characterSet is numbers.
                int codepageNumber = int.Parse(charSetUpper, System.Globalization.CultureInfo.InvariantCulture);

                return Encoding.GetEncoding(codepageNumber);
            }

            // It seems there is no codepage value in the characterSet. It must be a named encoding
            return Encoding.GetEncoding(characterSet);
        }
        #endregion


        #region IsValidBase64
        //stolen from http://stackoverflow.com/questions/3355407/validate-string-is-base64-format-using-regex
        private const char Base64Padding = '=';

        private static readonly HashSet<char> Base64Characters = new HashSet<char>() { 
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 
            'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f', 
            'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 
            'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '+', '/'
        };

        internal static bool IsValidBase64String(string param) {
            if (param == null) {
                // null string is not Base64 
                return false;
            }

            // replace optional CR and LF characters
            param = param.Replace("\r", String.Empty).Replace("\n", String.Empty);

            int lengthWPadding = param.Length;
            if (lengthWPadding == 0 || (lengthWPadding % 4) != 0) {
                // Base64 string should not be empty
                // Base64 string length should be multiple of 4
                return false;
            }

            // replace pad chacters
            int lengthWOPadding;

            param = param.TrimEnd(Base64Padding);
            lengthWOPadding = param.Length;

            if ((lengthWPadding - lengthWOPadding) > 2) {
                // there should be no more than 2 pad characters
                return false;
            }

            foreach (char c in param) {
                if (!Base64Characters.Contains(c)) {
                    // string contains non-Base64 character
                    return false;
                }
            }

            // nothing invalid found
            return true;
        }
        #endregion

        internal static VT Get<KT, VT>(this IDictionary<KT, VT> dictionary, KT key, VT defaultValue = default(VT)) {
            if (dictionary == null) return defaultValue;
            VT value;
            if (dictionary.TryGetValue(key, out value))
                return value;
            return defaultValue;
        }

        internal static void Set<KT, VT>(this IDictionary<KT, VT> dictionary, KT key, VT value) {
            if (!dictionary.ContainsKey(key))
                lock (dictionary)
                    if (!dictionary.ContainsKey(key)) {
                        dictionary.Add(key, value);
                        return;
                    }

            dictionary[key] = value;
        }


        internal static void Fire<T>(this EventHandler<T> events, object sender, T args) where T : EventArgs {
            if (events == null) return;
            events(sender, args);
        }

        internal static MailAddress ToEmailAddress(this string input) {
            try {
                return new MailAddress(input);
            } catch (Exception) {
                return null;
            }
        }

        internal static bool Is(this string input, string other) {
            return string.Equals(input, other, StringComparison.OrdinalIgnoreCase);
        }
    }
}
