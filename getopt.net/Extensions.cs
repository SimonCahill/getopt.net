using System;

namespace getopt.net {

    using System.Linq;

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

    }
}

