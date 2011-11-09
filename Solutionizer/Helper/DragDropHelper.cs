using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Solutionizer.Extensions;

namespace Solutionizer.Helper {
    public class DragDropHelper {
        // source and target
        private readonly DataFormat _format = DataFormats.GetDataFormat("DragDropItemsControl");
        private Point _initialMousePosition;
        private Vector _initialMouseOffset;
        private object _draggedData;
        private DraggedAdorner _draggedAdorner;
        private InsertionAdorner _insertionAdorner;
        private Window _topWindow;
        // source
        private ItemsControl _sourceItemsControl;
        private FrameworkElement _sourceItemContainer;
        // target
        private ItemsControl _targetItemsControl;
        private FrameworkElement _targetItemContainer;
        private bool _hasVerticalOrientation;
        private int _insertionIndex;
        private bool _isInFirstHalf;
        // singleton
        private static DragDropHelper _instance;

        private static DragDropHelper Instance {
            get { return _instance ?? (_instance = new DragDropHelper()); }
        }

        public static bool GetIsDragSource(DependencyObject obj) {
            return (bool) obj.GetValue(IsDragSourceProperty);
        }

        public static void SetIsDragSource(DependencyObject obj, bool value) {
            obj.SetValue(IsDragSourceProperty, value);
        }

        public static readonly DependencyProperty IsDragSourceProperty =
            DependencyProperty.RegisterAttached("IsDragSource", typeof (bool), typeof (DragDropHelper),
                                                new UIPropertyMetadata(false, IsDragSourceChanged));

        public static bool GetIsDropTarget(DependencyObject obj) {
            return (bool) obj.GetValue(IsDropTargetProperty);
        }

        public static void SetIsDropTarget(DependencyObject obj, bool value) {
            obj.SetValue(IsDropTargetProperty, value);
        }

        public static readonly DependencyProperty IsDropTargetProperty =
            DependencyProperty.RegisterAttached("IsDropTarget", typeof (bool), typeof (DragDropHelper),
                                                new UIPropertyMetadata(false, IsDropTargetChanged));

        public static DataTemplate GetDragDropTemplate(DependencyObject obj) {
            return (DataTemplate) obj.GetValue(DragDropTemplateProperty);
        }

        public static void SetDragDropTemplate(DependencyObject obj, DataTemplate value) {
            obj.SetValue(DragDropTemplateProperty, value);
        }

        public static readonly DependencyProperty DragDropTemplateProperty =
            DependencyProperty.RegisterAttached("DragDropTemplate", typeof (DataTemplate), typeof (DragDropHelper),
                                                new UIPropertyMetadata(null));

        public static ICommand GetDropCommand(DependencyObject obj) {
            return (ICommand) obj.GetValue(DropCommandProperty);
        }

        public static void SetDropCommand(DependencyObject obj, ICommand value) {
            obj.SetValue(DropCommandProperty, value);
        }
        
        public static readonly DependencyProperty DropCommandProperty =
            DependencyProperty.RegisterAttached("DropCommand", typeof (ICommand), typeof (DragDropHelper), new UIPropertyMetadata(null));

        private static void IsDragSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var dragSource = obj as ItemsControl;
            if (dragSource != null) {
                if (Equals(e.NewValue, true)) {
                    dragSource.PreviewMouseLeftButtonDown += Instance.DragSource_PreviewMouseLeftButtonDown;
                    dragSource.PreviewMouseLeftButtonUp += Instance.DragSource_PreviewMouseLeftButtonUp;
                    dragSource.PreviewMouseMove += Instance.DragSource_PreviewMouseMove;
                } else {
                    dragSource.PreviewMouseLeftButtonDown -= Instance.DragSource_PreviewMouseLeftButtonDown;
                    dragSource.PreviewMouseLeftButtonUp -= Instance.DragSource_PreviewMouseLeftButtonUp;
                    dragSource.PreviewMouseMove -= Instance.DragSource_PreviewMouseMove;
                }
            }
        }

        private static void IsDropTargetChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var dropTarget = obj as ItemsControl;
            if (dropTarget != null) {
                if (Equals(e.NewValue, true)) {
                    dropTarget.AllowDrop = true;
                    dropTarget.PreviewDrop += Instance.DropTarget_PreviewDrop;
                    dropTarget.PreviewDragEnter += Instance.DropTarget_PreviewDragEnter;
                    dropTarget.PreviewDragOver += Instance.DropTarget_PreviewDragOver;
                    dropTarget.PreviewDragLeave += Instance.DropTarget_PreviewDragLeave;
                } else {
                    dropTarget.AllowDrop = false;
                    dropTarget.PreviewDrop -= Instance.DropTarget_PreviewDrop;
                    dropTarget.PreviewDragEnter -= Instance.DropTarget_PreviewDragEnter;
                    dropTarget.PreviewDragOver -= Instance.DropTarget_PreviewDragOver;
                    dropTarget.PreviewDragLeave -= Instance.DropTarget_PreviewDragLeave;
                }
            }
        }

        // DragSource

        private void DragSource_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            _sourceItemsControl = (ItemsControl) sender;
            var visual = e.OriginalSource as Visual;

            _topWindow = Window.GetWindow(_sourceItemsControl);
            _initialMousePosition = e.GetPosition(_topWindow);

            _sourceItemContainer = visual.TryFindParent<FrameworkElement>();
            //_sourceItemContainer = _sourceItemsControl.ContainerFromElement(visual) as FrameworkElement;
            if (_sourceItemContainer != null) {
                _draggedData = _sourceItemContainer.DataContext;
            }
        }

        // Drag = mouse down + move by a certain amount
        private void DragSource_PreviewMouseMove(object sender, MouseEventArgs e) {
            if (_draggedData != null) {
                // Only drag when user moved the mouse by a reasonable amount.
                if (Utilities.IsMovementBigEnough(_initialMousePosition, e.GetPosition(_topWindow))) {
                    _initialMouseOffset = _initialMousePosition - _sourceItemContainer.TranslatePoint(new Point(0, 0), _topWindow);

                    var data = new DataObject(_format.Name, _draggedData);

                    // Adding events to the window to make sure dragged adorner comes up when mouse is not over a drop target.
                    var previousAllowDrop = _topWindow.AllowDrop;
                    _topWindow.AllowDrop = true;
                    _topWindow.DragEnter += TopWindow_DragEnter;
                    _topWindow.DragOver += TopWindow_DragOver;
                    _topWindow.DragLeave += TopWindow_DragLeave;

                    var effects = DragDrop.DoDragDrop((DependencyObject) sender, data, DragDropEffects.Move);

                    // Without this call, there would be a bug in the following scenario: Click on a data item, and drag
                    // the mouse very fast outside of the window. When doing this really fast, for some reason I don't get 
                    // the Window leave event, and the dragged adorner is left behind.
                    // With this call, the dragged adorner will disappear when we release the mouse outside of the window,
                    // which is when the DoDragDrop synchronous method returns.
                    RemoveDraggedAdorner();

                    _topWindow.AllowDrop = previousAllowDrop;
                    _topWindow.DragEnter -= TopWindow_DragEnter;
                    _topWindow.DragOver -= TopWindow_DragOver;
                    _topWindow.DragLeave -= TopWindow_DragLeave;

                    _draggedData = null;
                }
            }
        }

        private void DragSource_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            _draggedData = null;
        }

        // DropTarget

        private void DropTarget_PreviewDragEnter(object sender, DragEventArgs e) {
            _targetItemsControl = (ItemsControl) sender;
            var draggedItem = e.Data.GetData(_format.Name);

            DecideDropTarget(e);
            if (draggedItem != null) {
                // Dragged Adorner is created on the first enter only.
                ShowDraggedAdorner(e.GetPosition(_topWindow));
                CreateInsertionAdorner();
            }
            e.Handled = true;
        }

        private void DropTarget_PreviewDragOver(object sender, DragEventArgs e) {
            var draggedItem = e.Data.GetData(_format.Name);

            DecideDropTarget(e);
            if (draggedItem != null) {
                // Dragged Adorner is only updated here - it has already been created in DragEnter.
                ShowDraggedAdorner(e.GetPosition(_topWindow));
                UpdateInsertionAdornerPosition();
            }
            e.Handled = true;
        }

        private void DropTarget_PreviewDrop(object sender, DragEventArgs e) {
            var draggedItem = e.Data.GetData(_format.Name);
            var indexRemoved = -1;

            if (draggedItem != null) {
                if ((e.Effects & DragDropEffects.Move) != 0) {
                    indexRemoved = Utilities.RemoveItemFromItemsControl(_sourceItemsControl, draggedItem);
                }
                // This happens when we drag an item to a later position within the same ItemsControl.
                if (indexRemoved != -1 && _sourceItemsControl == _targetItemsControl && indexRemoved < _insertionIndex) {
                    _insertionIndex--;
                }

                var dropCommand = GetDropCommand(_targetItemsControl);
                if (dropCommand != null) {
                    dropCommand.Execute(draggedItem);
                } else {
                    Utilities.InsertItemInItemsControl(_targetItemsControl, draggedItem, _insertionIndex);
                }

                RemoveDraggedAdorner();
                RemoveInsertionAdorner();
            }
            e.Handled = true;
        }

        private void DropTarget_PreviewDragLeave(object sender, DragEventArgs e) {
            // Dragged Adorner is only created once on DragEnter + every time we enter the window. 
            // It's only removed once on the DragDrop, and every time we leave the window. (so no need to remove it here)
            var draggedItem = e.Data.GetData(_format.Name);

            if (draggedItem != null) {
                RemoveInsertionAdorner();
            }
            e.Handled = true;
        }

        // If the types of the dragged data and ItemsControl's source are compatible, 
        // there are 3 situations to have into account when deciding the drop target:
        // 1. mouse is over an items container
        // 2. mouse is over the empty part of an ItemsControl, but ItemsControl is not empty
        // 3. mouse is over an empty ItemsControl.
        // The goal of this method is to decide on the values of the following properties: 
        // targetItemContainer, insertionIndex and isInFirstHalf.
        private void DecideDropTarget(DragEventArgs e) {
            var targetItemsControlCount = _targetItemsControl.Items.Count;
            var draggedItem = e.Data.GetData(_format.Name);

            var dropCommand = GetDropCommand(_targetItemsControl);
            if (dropCommand != null) {
                if (dropCommand.CanExecute(draggedItem)) {
                    return;
                }
            }

            if (IsDropDataTypeAllowed(draggedItem)) {
                if (targetItemsControlCount > 0) {
                    _hasVerticalOrientation =
                        Utilities.HasVerticalOrientation(_targetItemsControl.ItemContainerGenerator.ContainerFromIndex(0) as FrameworkElement);
                    _targetItemContainer = _targetItemsControl.ContainerFromElement((DependencyObject) e.OriginalSource) as FrameworkElement;

                    if (_targetItemContainer != null) {
                        var positionRelativeToItemContainer = e.GetPosition(_targetItemContainer);
                        _isInFirstHalf = Utilities.IsInFirstHalf(_targetItemContainer, positionRelativeToItemContainer, _hasVerticalOrientation);
                        _insertionIndex = _targetItemsControl.ItemContainerGenerator.IndexFromContainer(_targetItemContainer);

                        if (!_isInFirstHalf) {
                            _insertionIndex++;
                        }
                    } else {
                        _targetItemContainer =
                            _targetItemsControl.ItemContainerGenerator.ContainerFromIndex(targetItemsControlCount - 1) as FrameworkElement;
                        _isInFirstHalf = false;
                        _insertionIndex = targetItemsControlCount;
                    }
                } else {
                    _targetItemContainer = null;
                    _insertionIndex = 0;
                }
            } else {
                _targetItemContainer = null;
                _insertionIndex = -1;
                e.Effects = DragDropEffects.None;
            }
        }

        // Can the dragged data be added to the destination collection?
        // It can if destination is bound to IList<allowed type>, IList or not data bound.
        private bool IsDropDataTypeAllowed(object draggedItem) {
            bool isDropDataTypeAllowed;
            var collectionSource = _targetItemsControl.ItemsSource;
            if (draggedItem != null) {
                if (collectionSource != null) {
                    var draggedType = draggedItem.GetType();
                    var collectionType = collectionSource.GetType();

                    var genericIListType = collectionType.GetInterface("IList`1");
                    if (genericIListType != null) {
                        var genericArguments = genericIListType.GetGenericArguments();
                        isDropDataTypeAllowed = genericArguments[0].IsAssignableFrom(draggedType);
                    } else if (typeof (IList).IsAssignableFrom(collectionType)) {
                        isDropDataTypeAllowed = true;
                    } else {
                        isDropDataTypeAllowed = false;
                    }
                } else // the ItemsControl's ItemsSource is not data bound.
                {
                    isDropDataTypeAllowed = true;
                }
            } else {
                isDropDataTypeAllowed = false;
            }
            return isDropDataTypeAllowed;
        }

        // Window

        private void TopWindow_DragEnter(object sender, DragEventArgs e) {
            ShowDraggedAdorner(e.GetPosition(_topWindow));
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void TopWindow_DragOver(object sender, DragEventArgs e) {
            ShowDraggedAdorner(e.GetPosition(_topWindow));
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void TopWindow_DragLeave(object sender, DragEventArgs e) {
            RemoveDraggedAdorner();
            e.Handled = true;
        }

        // Adorners

        // Creates or updates the dragged Adorner. 
        private void ShowDraggedAdorner(Point currentPosition) {
            if (_draggedAdorner == null) {
                var adornerLayer = AdornerLayer.GetAdornerLayer(_sourceItemsControl);
                _draggedAdorner = new DraggedAdorner(_draggedData, GetDragDropTemplate(_sourceItemsControl), _sourceItemContainer, adornerLayer);
            }
            _draggedAdorner.SetPosition(currentPosition.X - _initialMousePosition.X + _initialMouseOffset.X,
                                       currentPosition.Y - _initialMousePosition.Y + _initialMouseOffset.Y);
        }

        private void RemoveDraggedAdorner() {
            if (_draggedAdorner != null) {
                _draggedAdorner.Detach();
                _draggedAdorner = null;
            }
        }

        private void CreateInsertionAdorner() {
            if (_targetItemContainer != null) {
                // Here, I need to get adorner layer from targetItemContainer and not targetItemsControl. 
                // This way I get the AdornerLayer within ScrollContentPresenter, and not the one under AdornerDecorator (Snoop is awesome).
                // If I used targetItemsControl, the adorner would hang out of ItemsControl when there's a horizontal scroll bar.
                var adornerLayer = AdornerLayer.GetAdornerLayer(_targetItemContainer);
                _insertionAdorner = new InsertionAdorner(_hasVerticalOrientation, _isInFirstHalf, _targetItemContainer, adornerLayer);
            }
        }

        private void UpdateInsertionAdornerPosition() {
            if (_insertionAdorner != null) {
                _insertionAdorner.IsInFirstHalf = _isInFirstHalf;
                _insertionAdorner.InvalidateVisual();
            }
        }

        private void RemoveInsertionAdorner() {
            if (_insertionAdorner != null) {
                _insertionAdorner.Detach();
                _insertionAdorner = null;
            }
        }

        public static class Utilities {
            // Finds the orientation of the panel of the ItemsControl that contains the itemContainer passed as a parameter.
            // The orientation is needed to figure out where to draw the adorner that indicates where the item will be dropped.
            public static bool HasVerticalOrientation(FrameworkElement itemContainer) {
                var hasVerticalOrientation = true;
                if (itemContainer != null) {
                    var panel = VisualTreeHelper.GetParent(itemContainer) as Panel;
                    StackPanel stackPanel;
                    WrapPanel wrapPanel;

                    if ((stackPanel = panel as StackPanel) != null) {
                        hasVerticalOrientation = (stackPanel.Orientation == Orientation.Vertical);
                    } else if ((wrapPanel = panel as WrapPanel) != null) {
                        hasVerticalOrientation = (wrapPanel.Orientation == Orientation.Vertical);
                    }
                    // You can add support for more panel types here.
                }
                return hasVerticalOrientation;
            }

            public static void InsertItemInItemsControl(ItemsControl itemsControl, object itemToInsert, int insertionIndex) {
                if (itemToInsert != null) {
                    var itemsSource = itemsControl.ItemsSource;

                    if (itemsSource == null) {
                        itemsControl.Items.Insert(insertionIndex, itemToInsert);
                    }
                        // Is the ItemsSource IList or IList<T>? If so, insert the dragged item in the list.
                    else if (itemsSource is IList) {
                        ((IList)itemsSource).Insert(insertionIndex, itemToInsert);
                    } else {
                        var type = itemsSource.GetType();
                        var genericIListType = type.GetInterface("IList`1");
                        if (genericIListType != null) {
                            type.GetMethod("Insert").Invoke(itemsSource, new[] { insertionIndex, itemToInsert });
                        }
                    }
                }
            }

            public static int RemoveItemFromItemsControl(ItemsControl itemsControl, object itemToRemove) {
                var indexToBeRemoved = -1;
                if (itemToRemove != null) {
                    indexToBeRemoved = itemsControl.Items.IndexOf(itemToRemove);

                    if (indexToBeRemoved != -1) {
                        var itemsSource = itemsControl.ItemsSource;
                        if (itemsSource == null) {
                            itemsControl.Items.RemoveAt(indexToBeRemoved);
                        }
                            // Is the ItemsSource IList or IList<T>? If so, remove the item from the list.
                        else if (itemsSource is IList) {
                            ((IList)itemsSource).RemoveAt(indexToBeRemoved);
                        } else {
                            var type = itemsSource.GetType();
                            var genericIListType = type.GetInterface("IList`1");
                            if (genericIListType != null) {
                                type.GetMethod("RemoveAt").Invoke(itemsSource, new object[] { indexToBeRemoved });
                            }
                        }
                    }
                }
                return indexToBeRemoved;
            }

            public static bool IsInFirstHalf(FrameworkElement container, Point clickedPoint, bool hasVerticalOrientation) {
                if (hasVerticalOrientation) {
                    return clickedPoint.Y < container.ActualHeight / 2;
                }
                return clickedPoint.X < container.ActualWidth / 2;
            }

            public static bool IsMovementBigEnough(Point initialMousePosition, Point currentPosition) {
                return (Math.Abs(currentPosition.X - initialMousePosition.X) >= SystemParameters.MinimumHorizontalDragDistance ||
                     Math.Abs(currentPosition.Y - initialMousePosition.Y) >= SystemParameters.MinimumVerticalDragDistance);
            }
        }
    }
}