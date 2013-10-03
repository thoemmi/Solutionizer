using System.Diagnostics;
using System.Windows.Documents;
using System.Windows.Interactivity;
using System.Windows.Navigation;

namespace Solutionizer.Infrastructure {
    public class HyperlinkNavigateBehavior : Behavior<Hyperlink> {
        protected override void OnAttached() {
            base.OnAttached();
            AssociatedObject.RequestNavigate += AssociatedObjectRequestNavigate;
        }

        protected override void OnDetaching() {
            AssociatedObject.RequestNavigate -= AssociatedObjectRequestNavigate;
            base.OnDetaching();
        }

        private void AssociatedObjectRequestNavigate(object sender, RequestNavigateEventArgs e) {
            var uri = AssociatedObject.NavigateUri;

            if (uri != null) {
                Process.Start(new ProcessStartInfo(uri.ToString()));
            }
        }
    }
}