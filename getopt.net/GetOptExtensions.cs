using System;

namespace getopt.net {

    using System.Text;

    /// <summary>
    /// A collection of extension methods provided with GetOpt.
    /// </summary>
    public static class GetOptExtensions {

        /// <summary>
        /// Left-aligns a given string and pads the string to the right with <paramref name="fillChar"/>.
        /// </summary>
        /// <param name="text">The current string instance to work on.</param>
        /// <param name="totalLength">The total desired length of the string.</param>
        /// <param name="fillChar">The char to pad the string with.</param>
        /// <returns>The left-aligned string.</returns>
        public static string AlignLeft(this string text, int totalLength, char fillChar = ' ') {
            if (text.Length > totalLength) { throw new ArgumentOutOfRangeException(nameof(text), "Text length must be less than or equal to totalLength!"); }

            var sBuilder = new StringBuilder().Append(text);

            for (int i = sBuilder.Length; i < totalLength; i++) {
                sBuilder.Append(fillChar);
            }

            return sBuilder.ToString();
        }

        /// <summary>
        /// Right-aligns a given string by applying a padding string on the left side of the string consisting of <paramref name="fillChar"/>.
        /// </summary>
        /// <param name="text">The current string instance to work on.</param>
        /// <param name="totalLength">The total desired length of the string.</param>
        /// <param name="fillChar">The char to pad the string with.</param>
        /// <returns>The right-aligned string.</returns>
        public static string AlignRight(this string text, int totalLength, char fillChar = ' ') {
            if (text.Length > totalLength) { throw new ArgumentOutOfRangeException(nameof(text), "Text length must be less than or equal to totalLength!"); }

            var sBuilder = new StringBuilder();

            var padLength = totalLength - text.Length;
            for (int i = 0; i < padLength; i++) {
                sBuilder.Append(fillChar);
            }
            sBuilder.Append(text);

            return sBuilder.ToString();
        }

        /// <summary>
        /// Centre-aligns a given string by applying a padding string on both sides of <paramref name="text"/>.
        /// </summary>
        /// <param name="text">The current string instance to work on.</param>
        /// <param name="totalLength">The total length of the string.</param>
        /// <param name="fillChar">The char to pad the string with.</param>
        /// <returns>The centred string.</returns>
        public static string AlignCentre(this string text, int totalLength, char fillChar = ' ') {
            var sBuilder = new StringBuilder();

            var strBegin = (double)totalLength / 2 - (double)text.Length / 2;
            for (int i = 0; i < strBegin; i++) {
                sBuilder.Append(fillChar);
            }
            sBuilder.Append(text);
            for (int i = sBuilder.Length; i < totalLength; i++) {
                sBuilder.Append(fillChar);
            }

            return sBuilder.ToString();
        }

        /// <summary>
        /// Center-aligns a given string by applying a padding string on both sides of <paramref name="text"/>.
        /// </summary>
        /// <param name="text">The current string instance to work on.</param>
        /// <param name="totalLength">The total length of the string.</param>
        /// <param name="fillChar">The char to pad the string with.</param>
        /// <returns>The centered string.</returns>
        public static string AlignCenter(this string text, int totalLength, char fillChar = ' ') => text.AlignCentre(totalLength, fillChar);

    }
}

