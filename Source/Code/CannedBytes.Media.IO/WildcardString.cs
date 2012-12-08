namespace CannedBytes.Media.IO
{
    static class WildcardString
    {
        public const string AlphaWildcard = "*";
        public const char AlphaWildcardChar = '*';
        public const string NumberWildcard = "#";
        public const char NumberWildcardChar = '#';

        public static bool HasWildcard(this string thisValue)
        {
            if (string.IsNullOrWhiteSpace(thisValue)) return false;

            return (thisValue.Contains(AlphaWildcard) || thisValue.Contains(NumberWildcard));
        }

        public static bool MatchesWith(this string thisValue, string thatValue)
        {
            if (thisValue == thatValue) return true;

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
                            match = (thisChar == thatChar);
                        }
                    }

                    return match;
                }
            }

            return (thisValue == null && thatValue == null);
        }
    }
}