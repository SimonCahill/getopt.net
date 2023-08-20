using System;

namespace getopt.net {

    using System.Diagnostics;
    using System.Text;

    /// <summary>
    /// A description generator class which takes all <see cref="Option"/> values from a <see cref="GetOpt"/>
    /// instance and generates a "help text" for use by your application.
    /// </summary>
    public partial class DescriptionGenerator {

        internal DescriptionGenerator(params Option[] options) {
            Options = options;
        }

        /// <summary>
        /// Gets or sets the default text; this text is used for options which have no <see cref="Option.Description"/>.
        /// </summary>
        /// <value>The default option text.</value>
        public string DefaultOptionText { get; set; } = "No description found.";

        /// <summary>
        /// Gets the list of options supplied to the <see cref="GetOpt"/> instance.
        /// </summary>
        /// <value>A list of options to use in the help text.</value>
        public Option[] Options { 
            get => m_options;
            internal set {
                if (Object.Equals(m_options, value)) { return; }

                m_options = value;
                LongOptColumnWidth = GetLongOptColummnWidth();
            }
        }
        private Option[] m_options = Array.Empty<Option>();

        /// <summary>
        /// Gets the minimum width for the "name" column.
        /// </summary>
        /// <value>The length (in chars) of the longest option name in <see cref="Options"/></value>
        public int LongOptColumnWidth { get; private set; }

        /// <summary>
        /// Gets or sets the maximum line length.
        /// </summary>
        /// <value>The maximum length (in chars) of a description line.</value>
        public int MaxLineWidth { get; set; } = 80;

        /// <summary>
        /// Gets or sets the text alignment for long options.
        /// </summary>
        public TextAlignment LongOptAlignment { get; set; } = TextAlignment.LeftAligned;

        /// <summary>
        /// Gets or sets the text alignment for option descriptions.
        /// </summary>
        public TextAlignment DescriptionAlignment { get; set; } = TextAlignment.LeftAligned;

        /// <summary>
        /// Gets the aligned long option for a given convention and option.
        /// </summary>
        /// <param name="convention">The convention to use.</param>
        /// <param name="opt">The option to use.</param>
        /// <returns>The aligned option name.</returns>
        private string GetAlignedLongOpt(OptionConvention convention, Option opt) {
            var longConventionPrefix = GetLongOptPrefixFor(convention);
            var alignedLongOpt = $"{longConventionPrefix}{opt.Name},";

            switch (LongOptAlignment) {
                default:
                case TextAlignment.LeftAligned:
                    alignedLongOpt = alignedLongOpt.AlignLeft(LongOptColumnWidth);
                    break;
                case TextAlignment.RightAligned:
                    alignedLongOpt = alignedLongOpt.AlignRight(LongOptColumnWidth);
                    break;
                case TextAlignment.Centred:
                    alignedLongOpt = alignedLongOpt.AlignCentre(LongOptColumnWidth);
                    break;
            }

            return alignedLongOpt;
        }

        /// <summary>
        /// Gets the aligned option description.
        /// </summary>
        /// <param name="opt">The option to align.</param>
        /// <param name="width">The width of the string.</param>
        /// <returns>The aligned description.</returns>
        private string GetAlignedDescription(Option opt, int width) {
            var alignedText = opt.Description ?? DefaultOptionText;

            switch (DescriptionAlignment) {
                case TextAlignment.LeftAligned:
                default:
                    return alignedText.AlignLeft(width);
                case TextAlignment.RightAligned:
                    return alignedText.AlignRight(width);
                case TextAlignment.Centred:
                    return alignedText.AlignCentre(width);
            }
        }
        
        /// <summary>
        /// Gets the minimum width for the "name" column.
        /// </summary>
        /// <returns>The length (in chars) of the longest option name in <see cref="Options"/></returns>
        private int GetLongOptColummnWidth() {
            int longestWidth = 0;

            foreach (var opt in Options.Where(x => x.Name is not null && x.Name.Length > 0)) {
                if (opt.Name?.Length > longestWidth) { longestWidth = opt.Name.Length; }
            }

            return longestWidth;
        }

        /// <summary>
        /// Gets the description for a single option.
        /// </summary>
        /// <param name="opt">The opt for which to retrieve the description.</param>
        /// <param name="convention" >The option convention to use when printing the argument.</param>
        /// <returns>A string detailling the use of an option.</returns>
        /// 
        /// <example >
        /// Given the Option `new Option("help", 'h', ArgumentType.None, "Displays this text and exits")`:
        /// 
        /// --help, 
        /// </example>
        public string GetDescriptionFor(OptionConvention convention, Option opt) {
            
            var shortConventionPrefix = GetShortOptPrefixFor(convention);
            var colWidth = LongOptColumnWidth;
            
            var formattedOpts = string.Format(
                $"{{0}} {{1,-2}}",
                GetAlignedLongOpt(convention, opt),
                $"{shortConventionPrefix}{(char)opt.Value}"
            );

            var alignedDescription = GetAlignedDescription(opt, MaxLineWidth - formattedOpts.Length);
            var remainingPadding = MaxLineWidth - alignedDescription.Length > 0 ? MaxLineWidth - alignedDescription.Length : 1;

            return string.Format(
                "{0}{1}{2}",
                formattedOpts,
                new string(' ', remainingPadding),
                alignedDescription
            );
        }

        public string GenerateDescription(OptionConvention convention) {
            var sBuilder = new StringBuilder();
            var longConventionPrefix = GetLongOptPrefixFor(convention);
            var shortConventionPrefix = GetShortOptPrefixFor(convention);
            var colWidth = LongOptColumnWidth;

            sBuilder.AppendFormat(
                "{0} {1} {2}",
                "Long opt", "Short Opt", "Opt Description"
            );
            sBuilder.AppendLine();

            foreach (var opt in Options) {
                sBuilder.AppendLine(GetDescriptionFor(convention, opt));
            }
            sBuilder.AppendLine();

            return sBuilder.ToString();
        }
    }
}