using System.Windows;
using System.Windows.Media;

namespace Solutionizer.Extensions {
    public static class DependencyObjectExtensions {
        /// <summary>
        /// Finds a parent of a given item on the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="child">A direct or indirect child of the
        /// queried item.</param>
        /// <returns>The first parent item that matches the submitted
        /// type parameter. If not matching item can be found, a null
        /// reference is being returned.</returns>
        public static T TryFindParent<T>(this DependencyObject child)
            where T : DependencyObject {
            //get parent item
            DependencyObject parentObject = GetParentObject(child);

            //we've reached the end of the tree
            if (parentObject == null) {
                return null;
            }

            //check if the parent matches the type we're looking for
            var parent = parentObject as T;
            if (parent != null) {
                return parent;
            } else {
                //use recursion to proceed with next level
                return TryFindParent<T>(parentObject);
            }
        }

        /// <summary>
        /// This method is an alternative to WPF's
        /// <see cref="VisualTreeHelper.GetParent"/> method, which also
        /// supports content elements. Keep in mind that for content element,
        /// this method falls back to the logical tree of the element!
        /// </summary>
        /// <param name="child">The item to be processed.</param>
        /// <returns>The submitted item's parent, if available. Otherwise
        /// null.</returns>
        public static DependencyObject GetParentObject(this DependencyObject child) {
            if (child == null) {
                return null;
            }

            //handle content elements separately
            var contentElement = child as ContentElement;
            if (contentElement != null) {
                DependencyObject parent = ContentOperations.GetParent(contentElement);
                if (parent != null) {
                    return parent;
                }

                var fce = contentElement as FrameworkContentElement;
                return fce != null ? fce.Parent : null;
            }

            //also try searching for parent in framework elements (such as DockPanel, etc)
            var frameworkElement = child as FrameworkElement;
            if (frameworkElement != null) {
                DependencyObject parent = frameworkElement.Parent;
                if (parent != null) {
                    return parent;
                }
            }

            //if it's not a ContentElement/FrameworkElement, rely on VisualTreeHelper
            return VisualTreeHelper.GetParent(child);
        }

        /// <summary>
        /// Search for an element of a certain type in the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of element to find.</typeparam>
        /// <param name="visual">The parent element.</param>
        /// <returns></returns>
        public static T FindVisualChild<T>(this DependencyObject visual) where T : DependencyObject {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(visual); i++) {
                var child = (Visual)VisualTreeHelper.GetChild(visual, i);
                if (child != null) {
                    var correctlyTyped = child as T;
                    if (correctlyTyped != null) {
                        return correctlyTyped;
                    }

                    var descendent = FindVisualChild<T>(child);
                    if (descendent != null) {
                        return descendent;
                    }
                }
            }

            return null;
        }

    }
}