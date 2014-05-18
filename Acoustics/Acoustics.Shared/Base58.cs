namespace Acoustics.Shared
{
    using System;

    /// <summary>
    /// "Base58 is what you get after taking Base62 [a-zA-Z0-9] and removing any character that may 
    /// induce to error when introduced by hand: 0 (zero), O (uppercase 'o'), I (uppercase 'i'), and l (lowercase 'L'). 
    /// This concept was introduced to the general public by Flickr".
    /// </summary>
    /// <remarks>
    /// See http://icoloma.blogspot.com.au/2010/03/create-your-own-bitly-using-base58.html and 
    /// http://dl.dropbox.com/u/1844215/FlickrBaseEncoder.java for more.
    /// </remarks>
    public class Base58
    {
        private const string AlphabetString = "123456789abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ";

        private static readonly char[] Alphabet = AlphabetString.ToCharArray();
        private static readonly int BaseCount = Alphabet.Length;

        /// <summary>
        /// Convert number into alphanumeric representation.
        /// </summary>
        /// <param name="num">
        /// The num.
        /// </param>
        /// <returns>
        /// Alphanumeric representation of number.
        /// </returns>
        public static string Encode(long num)
        {
            string result = string.Empty;
            long div;
            int mod = 0;

            while (num >= BaseCount)
            {
                div = num / BaseCount;
                mod = (int)(num - (BaseCount * (long)div));
                result = Alphabet[mod] + result;
                num = (long)div;
            }

            if (num > 0)
            {
                result = Alphabet[(int)num] + result;
            }

            return result;
        }

        /// <summary>
        /// Convert a link into numbers.
        /// </summary>
        /// <param name="link">
        /// The link.
        /// </param>
        /// <returns>
        /// Numerical representation of link.
        /// </returns>
        public static long Decode(string link)
        {
            long result = 0;
            long multi = 1;
            while (link.Length > 0)
            {
                string digit = link.Substring(link.Length - 1);
                result = result + (multi * AlphabetString.LastIndexOf(digit, StringComparison.Ordinal));
                multi = multi * BaseCount;
                link = link.Substring(0, link.Length - 1);
            }

            return result;
        }
    }
}
