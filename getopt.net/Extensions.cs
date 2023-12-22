using System;

namespace getopt.net {

    using System.Linq;
    using System.Text;

    /// <summary >
    /// This class contains extension methods specific to getopt.net.
    /// If these extension methods help you in your program, you're free to use them too!
    /// </summary>
    public static class Extensions {

        /// <summary>
        /// Finds an option with the name <paramref name="optName" />
        /// </summary>
        /// <param name="list">The list of options to search.</param>
        /// <param name="optName">The name of the argument to search for.</param>
        /// <returns>The <see cref="Option" /> with the name <paramref name="optName" />, or <code >null</code> if no option was found matching the name.</returns>
        public static Option? FindOptionOrDefault(this Option[] list, string optName) {
            if (string.IsNullOrEmpty(optName)) { throw new ArgumentNullException(nameof(optName), "optName must not be null!"); }

            return list.FirstOrDefault(o => o.Name?.Equals(optName, StringComparison.InvariantCulture) == true);
        }

        /// <summary>
        /// Finds an option in the list <paramref name="list" /> with the <see cref="Option.Value" /> <paramref name="optVal" />.
        /// </summary>
        /// <param name="list">The list of options to search.</param>
        /// <param name="optVal">The value to search for.</param>
        /// <returns>The <see cref="Option" /> with the <see cref="Option.Value" /> <paramref name="optVal" />, or <code >null</code> if no option was found matching the name.</returns>
        public static Option? FindOptionOrDefault(this Option[] list, char optVal) => list.FirstOrDefault(o => o.Value == optVal);

        /// <summary>
        /// Creates a short opt string from an array of <see cref="Option"/> objects.
        /// </summary>
        /// <param name="list">The options to convert.</param>
        /// <returns><code>null</code> if the option list is empty or null. A string contain a shortopt-form string representing all the options from <paramref name="list"/>.</returns>
        public static string? ToShortOptString(this Option[] list) {
            if (list is null || list.Length == 0) { return null; }

            var sBuilder = new StringBuilder();

            foreach (var opt in list) {
                sBuilder.Append((char)opt.Value);
                switch (opt.ArgumentType) {
                    case ArgumentType.Required:
                        sBuilder.Append(':');
                        break;
                    case ArgumentType.Optional:
                        sBuilder.Append(';');
                        break;
                    default: break;
                }
            }

            return sBuilder.ToString();
        }

    }
}

