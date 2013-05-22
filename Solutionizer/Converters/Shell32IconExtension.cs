using System;
using System.Windows.Markup;
using System.Windows.Media;
using Solutionizer.Helper;

namespace Solutionizer.Converters {
    [MarkupExtensionReturnType(typeof (ImageSource))]
    public class Shell32IconExtension : MarkupExtension {
        public Shell32IconExtension() {
        }

        public Shell32IconExtension(string filename, int iconIndex) {
            FileName = filename;
            IconIndex = iconIndex;
        }

        [ConstructorArgument("filename")]
        public string FileName { get; set; }

        [ConstructorArgument("iconIndex")]
        public int IconIndex { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return Icons.GetImageFromFileAndIndex(FileName, IconIndex);
        }
    }
}