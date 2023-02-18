using System;

namespace getopt.net {

	using System.Linq;

	public static class Extensions {

		public static Option? FindOptionOrDefault(this Option[] list, string optName) {
			foreach (var opt in list) {
				if (opt.Name?.Equals(optName, StringComparison.InvariantCulture) == true) {
					return opt;
				}
			}

			return null;
		}

		public static Option? FindOptionOrDefault(this Option[] list, char optVal) {
			foreach (var opt in list) {
				if (opt.Value == optVal) {
					return opt;
				}
			}

			return null;
		}

	}
}

