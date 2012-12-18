using System.Text;

namespace CannedBytes.Media.IO
{
    /// <summary>
    /// Implements the logic for detecting and matching wildcards ('*' and '#').
    /// </summary>
    internal static class WildcardString
    {
        /// <summary>Alphanumeric wildcard '*'.</summary>
        public const string AlphaWildcard = "*";

        /// <summary>Alphanumeric wildcard '*'.</summary>
        public const char AlphaWildcardChar = '*';

        /// <summary>Numeric wildcard '#'.</summary>
        public const string NumberWildcard = "#";

        /// <summary>Numeric wildcard '#'.</summary>
        public const char NumberWildcardChar = '#';

        /// <summary>
        /// Indicates if <paramref name="thisValue"/> contains any of the wildcard characters.
        /// </summary>
        /// <param name="thisValue">Can be null.</param>
        /// <returns>Returns true if there are wildcard characters in <paramref name="thisValue"/>.</returns>
        public static bool HasWildcard(this string thisValue)
        {
            if (string.IsNullOrWhiteSpace(thisValue))
            {
                return false;
            }

            return thisValue.Contains(AlphaWildcard) || thisValue.Contains(NumberWildcard);
        }

        /// <summary>
        /// Indicates if there is a wildcard match between <paramref name="thisValue"/> and <paramref name="thatValue"/>.
        /// </summary>
        /// <param name="thisValue">Can be null.</param>
        /// <param name="thatValue">Can be null.</param>
        /// <returns>Returns true if there is a wildcard match.</returns>
        public static bool MatchesWith(this string thisValue, string thatValue)
        {
            if (thisValue == thatValue)
            {
                return true;
            }

            if (thisValue != null && thatValue != null)
            {
                if (thisValue.Length == thatValue.Length)
                {
                    var match = true;
                    var count = thisValue.Length;

                    // exit loop when no longer a match
                    for (int i = 0; i < count && match; i++)
                    {
                        var thisChar = thisValue[i];
                        var thatChar = thatValue[i];

                        if (thisChar == NumberWildcardChar)
                        {
                            match = char.IsDigit(thatChar);
                        }
                        else if (thatChar == NumberWildcardChar)
                        {
                            match = char.IsDigit(thisChar);
                        }
                        else if (thisChar != AlphaWildcardChar &&
                            thatChar != AlphaWildcardChar)
                        {
                            match = thisChar == thatChar;
                        }
                    }

                    return match;
                }
            }

            return thisValue == null && thatValue == null;
        }

        public static string Merge(this string thisValue, string thatValue)
        {
            StringBuilder result = new StringBuilder();

            if (thisValue != null && thatValue != null)
            {
                if (thisValue.Length == thatValue.Length)
                {
                    var count = thisValue.Length;

                    for (int i = 0; i < count; i++)
                    {
                        var thisChar = thisValue[i];
                        var thatChar = thatValue[i];

                        if (thisChar == NumberWildcardChar ||
                            thisChar == AlphaWildcardChar)
                        {
                            result.Append(thatChar);
                        }
                        else
                        {
                            result.Append(thisChar);
                        }
                    }
                }
            }

            return result.ToString();
        }
    }
}