namespace NetVips
{
    using System;
    using System.Linq;

    public static class SampleExtensions
    {
        /// <summary>
        /// Make first letter of a string upper case.
        /// </summary>
        /// <param name="str">The input string.</param>
        /// <returns>A new string with the first letter upper case.</returns>
        internal static string FirstLetterToUpper(this string str)
        {
            if (str == null)
            {
                return null;
            }

            if (str.Length > 1)
            {
                return char.ToUpper(str[0]) + str[1..];
            }

            return str.ToUpper();
        }

        /// <summary>
        /// Make first letter of a string lower case.
        /// </summary>
        /// <param name="str">The input string.</param>
        /// <returns>A new string with the first letter lower case.</returns>
        internal static string FirstLetterToLower(this string str)
        {
            if (str == null)
            {
                return null;
            }

            if (str.Length > 1)
            {
                return char.ToLower(str[0]) + str[1..];
            }

            return str.ToLower();
        }

        /// <summary>
        /// Convert snake case (my_string) to camel case (MyString).
        /// </summary>
        /// <param name="str">The input string.</param>
        /// <returns>A new camel cased string.</returns>
        internal static string ToPascalCase(this string str)
        {
            return str.Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => char.ToUpperInvariant(s[0]) + s[1..])
                .Aggregate(string.Empty, (s1, s2) => s1 + s2);
        }

        public static double NextDouble(this Random random, double minValue, double maxValue)
        {
            return random.NextDouble() * (maxValue - minValue) + minValue;
        }
    }
}