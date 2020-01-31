using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnippetMan.Controls.Utils
{
    public static class GeneralHelper
    {
        /// <summary>
        /// Returns true if the first letter is the given character.
        /// Does no localization, no options and thus is performance-wise a lot better than calling "StartsWith"
        /// </summary>
        /// <param name="haystack"></param>
        /// <param name="needle"></param>
        /// <returns>False if the first letter is not the given character or if the string is empty</returns>
        public static bool IsFirstLetter(this string haystack, char needle)
        {
            return String.IsNullOrEmpty(haystack) ? false : haystack[0] == needle;
        }
    }
}
