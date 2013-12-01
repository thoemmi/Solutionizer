using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Solutionizer.Controls {
    /// <summary>
    /// Enum for specifying where the ellipsis should appear.
    /// </summary>
    public enum EllipsisPlacement {
        Left,
        Center,
        Right,
        Path
    }

    /// <summary>
    /// This is a subclass of TextBox with the ability to show an ellipsis 
    /// when the Text doesn't fit in the visible area.
    /// </summary>
    public class EllipsisTextBox : TextBox {
        // Constructor
        public EllipsisTextBox() {
            // Initialize inherited stuff as desired.
            IsReadOnlyCaretVisible = true;

            // Initialize stuff added by this class
            FudgePix = 3.0;
            _internalEnabled = true;

            LayoutUpdated += TextBoxWithEllipsis_LayoutUpdated;
            SizeChanged += TextBoxWithEllipsis_SizeChanged;
        }

        #region LongText
        public static readonly DependencyProperty LongTextProperty = DependencyProperty.Register(
            "LongText", typeof(string), typeof(EllipsisTextBox), new PropertyMetadata(default(string), LongTextChanged));

        public string LongText {
            get { return (string)GetValue(LongTextProperty); }
            set { SetValue(LongTextProperty, value); }
        }

        private static void LongTextChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
            var textBox = dependencyObject as EllipsisTextBox;

            if (textBox == null)
                return;

            textBox.PrepareForLayout();
        }
        #endregion // LongText

        #region EllipsisPlacement
        public static readonly DependencyProperty EllipsisPlacementProperty = DependencyProperty.Register(
            "EllipsisPlacement", typeof(EllipsisPlacement), typeof(EllipsisTextBox), new PropertyMetadata(EllipsisPlacement.Right, EllispsisPlacementChanged));

        public EllipsisPlacement EllipsisPlacement {
            get { return (EllipsisPlacement)GetValue(EllipsisPlacementProperty); }
            set { SetValue(EllipsisPlacementProperty, value); }
        }

        private static void EllispsisPlacementChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
            var textBox = dependencyObject as EllipsisTextBox;

            if (textBox == null)
                return;

            if (textBox.DoEllipsis) {
                textBox.PrepareForLayout();
            }
        }

        #endregion // EllipsisPlacement

        #region IsEllipsisEnabled
        /// <summary>
        /// If true, Text/LongText will be truncated with ellipsis
        /// to fit in the visible area of the TextBox
        /// (except when it has the focus).
        /// </summary>
        public static readonly DependencyProperty IsEllipsisEnabledProperty = DependencyProperty.Register(
            "IsEllipsisEnabled", typeof(bool), typeof(EllipsisTextBox), new PropertyMetadata(true, IsEllipsisEnabledChanged));

        public bool IsEllipsisEnabled {
            get { return (bool)GetValue(IsEllipsisEnabledProperty); }
            set { SetValue(IsEllipsisEnabledProperty, value); }
        }

        private static void IsEllipsisEnabledChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
            var textBox = dependencyObject as EllipsisTextBox;

            if (textBox == null)
                return;

            textBox._externalEnabled = textBox.IsEllipsisEnabled;
            textBox.PrepareForLayout();

            if (textBox.DoEllipsis) {
                textBox.TextBoxWithEllipsis_LayoutUpdated(textBox, EventArgs.Empty);
            }
        }
        #endregion // IsEllipsisEnabled

        #region UseLongTextForToolTip
        /// <summary>
        /// If true, ToolTip will be set to LongText whenever
        /// LongText doesn't fit in the visible area.  
        /// If false, ToolTip will be set to null unless
        /// the user sets it to something other than LongText.
        /// </summary>
        public static readonly DependencyProperty UseLongTextForToolTipProperty = DependencyProperty.Register(
            "UseLongTextForToolTip", typeof(bool), typeof(EllipsisTextBox), new PropertyMetadata(true, UseLongTextForToolTipChanged));


        public bool UseLongTextForToolTip {
            get { return (bool)GetValue(UseLongTextForToolTipProperty); }
            set { SetValue(UseLongTextForToolTipProperty, value); }
        }

        private static void UseLongTextForToolTipChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
            var textBox = dependencyObject as EllipsisTextBox;

            if (textBox == null)
                return;

            if (textBox.UseLongTextForToolTip) {
                if (textBox.ExtentWidth > textBox.ViewportWidth || textBox.Text != textBox.LongText) {
                    // When turning it on, set ToolTip to
                    // _longText if the current Text is too long.
                    textBox.ToolTip = textBox.LongText;
                } else {
                    // When turning it off, set ToolTip to null
                    // unless user has set it to something other
                    // than _longText;
                    if (textBox.LongText.Equals(textBox.ToolTip)) {
                        textBox.ToolTip = null;
                    }
                }
            }
        }
        #endregion // UseLongTextForToolTip

        public double FudgePix {
            get;
            set;
        }

        protected bool DoEllipsis { get { return IsEllipsisEnabled && _internalEnabled; } }

        #region Private Properties
        // Last length of substring of LongText known to fit.
        // Used while calculating the correct length to fit.
        private int _lastFitLen;

        // Last length of substring of LongText known to be too long.
        // Used while calculating the correct length to fit.
        private int _lastLongLen;

        // Length of substring of LongText currently assigned to the Text property.
        // Used while calculating the correct length to fit.
        private int _curLen;

        // Used to detect whether the OnTextChanged event occurs due to an
        // external change vs. an internal one.
        private bool _externalChange = true;

        // Used to disable ellipsis internally (primarily while
        // the control has the focus).
        private bool _internalEnabled = true;

        // Backer for IsEllipsisEnabled
        private bool _externalEnabled = true;
        #endregion // Private Properties

        #region TextBox Overrides
        // OnTextChanged is overridden so we can avoid 
        // raising the TextChanged event when we change 
        // the Text property internally while searching 
        // for the longest substring that fits.
        // If Text is changed externally, we copy the
        // new Text into LongText before we overwrite Text 
        // with the truncated version (if IsEllipsisEnabled).
        protected override void OnTextChanged(TextChangedEventArgs e) {
            if (_externalChange) {
                LongText = Text;

                PrepareForLayout();
                base.OnTextChanged(e);
            }
        }

        // Makes the entire text available for editing, selecting, and scrolling
        // until focus is lost.
        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e) {
            _internalEnabled = false;
            SetText(LongText);
            base.OnGotKeyboardFocus(e);
        }

        // Returns to trimming and showing ellipsis.
        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e) {
            _internalEnabled = true;
            PrepareForLayout();
            base.OnLostKeyboardFocus(e);
        }
        #endregion // TextBox Overrides

        // Sets the Text property without raising the TextChanged event.
        private void SetText(string text) {
            if (Text != text) {
                _externalChange = false;
                Text = text; // Will trigger Layout event.
                _externalChange = true;
            }

        }

        // Arranges for the next LayoutUpdated event to trim _longText and add ellipsis.
        // Also triggers layout by setting Text.
        private void PrepareForLayout() {
            if (LongText != null) {
                _lastFitLen = 0;
                _lastLongLen = LongText.Length;
                _curLen = LongText.Length;
            } else {
                _lastFitLen = 0;
                _lastLongLen = 0;
                _curLen = 0;
            }

            // This raises the LayoutUpdated event, whose
            // handler does the ellipsis.
            SetText(LongText);
        }

        private void TextBoxWithEllipsis_SizeChanged(object sender, SizeChangedEventArgs e) {
            if (DoEllipsis && e.NewSize.Width != e.PreviousSize.Width) {
                // We need to recalculate the longest substring of LongText that will fit (with ellipsis).
                // Prepare for the LayoutUpdated event, which does the recalc and is raised after this.
                PrepareForLayout();
            }
        }

        // Called when Text or Size changes (and maybe at other times we don't care about).
        private void TextBoxWithEllipsis_LayoutUpdated(object sender, EventArgs e) {
            if (DoEllipsis) {
                // This does a binary search (bisection) to determine the maximum substring
                // of _longText that will fit in visible area.  Instead of a loop, it
                // uses a type of recursion that happens because this event is raised
                // again if we set the Text property in here.

                if (ViewportWidth + FudgePix < ExtentWidth) {
                    // The current Text (whose length without ellipsis is _curLen) is too long.
                    _lastLongLen = _curLen;
                } else {
                    // The current Text is not too long.
                    _lastFitLen = _curLen;
                }

                // Try a new substring whose length is halfway between the last length
                // known to fit and the last length known to be too long.
                int newLen = (_lastFitLen + _lastLongLen) / 2;

                if (_curLen == newLen) {
                    // We're done! Usually, _lastLongLen is _lastFitLen + 1.
                    if (UseLongTextForToolTip) {
                        ToolTip = Text == LongText ? null : LongText;
                    }
                } else {
                    _curLen = newLen;

                    // This sets the Text property without raising the TextChanged event.
                    // However it does raise the LayoutUpdated event again, though
                    // not recursively.
                    CalcText();
                }
            } else if (UseLongTextForToolTip) {
                ToolTip = ViewportWidth < ExtentWidth ? LongText : null;
            }
        }

        // Sets Text to a substring of LongText based on _placement and _curLen.
        private void CalcText() {
            if (LongText == null) {
                switch (EllipsisPlacement) {
                    case EllipsisPlacement.Left:
                    case EllipsisPlacement.Center:
                    case EllipsisPlacement.Right:
                    case EllipsisPlacement.Path:
                        SetText(LongText);
                        break;
                }

                return;
            }

            switch (EllipsisPlacement) {
                case EllipsisPlacement.Right:
                    SetText(LongText.Substring(0, _curLen) + "\u2026");
                    break;

                case EllipsisPlacement.Center:
                    int firstLen = _curLen / 2;
                    int secondLen = _curLen - firstLen;
                    SetText(LongText.Substring(0, firstLen) + "\u2026" + LongText.Substring(LongText.Length - secondLen));
                    break;

                case EllipsisPlacement.Left:
                    int start = LongText.Length - _curLen;
                    SetText("\u2026" + LongText.Substring(start));
                    break;

                case EllipsisPlacement.Path:
                    var sb = new StringBuilder(_curLen + 1);
                    PathCompactPathEx(sb, LongText, _curLen, 0);
                    SetText(sb.ToString());
                    break;

                default:
                    throw new Exception("Unexpected switch value: " + EllipsisPlacement.ToString());
            }
        }

        [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
        static extern bool PathCompactPathEx([Out] StringBuilder pszOut, string szPath, int cchMax, int dwFlags);
    }
}