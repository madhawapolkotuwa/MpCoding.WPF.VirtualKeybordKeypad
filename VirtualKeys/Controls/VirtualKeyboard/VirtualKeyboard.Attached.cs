using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;

namespace VirtualKeys.Controls
{
    public partial class VirtualKeyboard
    {
        #region constants
        private const string PopupStyle = "AttachedVirtualKeyboardPopupStyle";
        private const string KeyboardStyle = "AttachedVirtualKeyboardStyle";
        #endregion

        #region fields
        private static Popup _popupInstance;
        private static VirtualKeyboard _keyboard;
        private static FrameworkElement _currentOwner;
        private static ComponentResourceKey _virtualKeyboardPopupStyleKey;
        private static ComponentResourceKey _attachedVirtualKeyboardStyle;
        #endregion
        public static ComponentResourceKey AttachedVirtualKeyboardPopupStyleKey
        {
            get
            {
                return _virtualKeyboardPopupStyleKey ??
                    (_virtualKeyboardPopupStyleKey = new ComponentResourceKey(typeof(VirtualKeyboard), PopupStyle));
            }
        }

        public static ComponentResourceKey AttachedVirtualKeyboardStyle
        {
            get
            {
                return _attachedVirtualKeyboardStyle ??
                       (_attachedVirtualKeyboardStyle = new ComponentResourceKey(typeof(VirtualKeyboard), KeyboardStyle));
            }
        }

        public static readonly DependencyProperty IsKeyboardEnabledProperty =
            DependencyProperty.RegisterAttached("IsKeyboardEnabled", typeof(bool), typeof(VirtualKeyboard),
                new PropertyMetadata(false));

        public static readonly DependencyProperty KeyboardWidthProperty =
            DependencyProperty.RegisterAttached("KeyboardWidth", typeof(double), typeof(VirtualKeyboard),
                new PropertyMetadata(330d));

        public static readonly DependencyProperty KeyboardHeightProperty =
            DependencyProperty.RegisterAttached("KeyboardHeight", typeof(double), typeof(VirtualKeyboard),
                new PropertyMetadata(330d));

        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.RegisterAttached("Mode", typeof(VirtualMode), typeof(VirtualKeyboard),
                new PropertyMetadata(VirtualMode.Disabled, OnModeChanged));
        private static int KeyboardPopupDelay
        {
            get { return 50; }
        }
        public static bool GetIsKeyboardEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsKeyboardEnabledProperty);
        }
        internal static void SetIsKeyboardEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsKeyboardEnabledProperty, value);
        }
        public static double GetKeyboardWidth(DependencyObject obj)
        {
            return (double)obj.GetValue(KeyboardWidthProperty);
        }
        public static void SetKeyboardWidth(DependencyObject obj, double value)
        {
            obj.SetValue(KeyboardWidthProperty, value);
        }
        public static double GetKeyboardHeight(DependencyObject obj)
        {
            return (double)obj.GetValue(KeyboardHeightProperty);
        }
        public static void SetKeyboardHeight(DependencyObject obj, double value)
        {
            obj.SetValue(KeyboardHeightProperty, value);
        }
        public static VirtualMode GetMode(DependencyObject obj)
        {
            return (VirtualMode)obj.GetValue(ModeProperty);
        }
        public static void SetMode(DependencyObject obj, VirtualMode value)
        {
            obj.SetValue(ModeProperty, value);
        }
        private static void OnModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var attachedControl = d as FrameworkElement;
            var mode = (VirtualMode)e.NewValue;
            RegisterAttachedControl(attachedControl, mode);
        }
        private static void RegisterAttachedControl(UIElement attachedControl, VirtualMode mode)
        {
            if (attachedControl != null && mode != VirtualMode.Disabled)
            {
                SetIsKeyboardEnabled(attachedControl, true);  //flag for binding

                if (mode.HasFlag(VirtualMode.Touch))
                {
                    attachedControl.IsManipulationEnabled = true;
                    attachedControl.ManipulationStarted += AttachedControlOnManipulationStarting;
                }
                if (mode.HasFlag(VirtualMode.Mouse))
                {
                    attachedControl.PreviewMouseDown += AttachedControlOnMouseDown;
                }

                //these events with Mouse.Capture() make sure that the popup is closed when user clicks (touches) out of the attachedControl
                attachedControl.PreviewMouseRightButtonDown += AttachedControlOnPreviewMouseEvent;
                attachedControl.PreviewMouseRightButtonUp += AttachedControlOnPreviewMouseEvent;
                attachedControl.PreviewMouseLeftButtonDown += AttachedControlOnPreviewMouseEvent;
                attachedControl.PreviewMouseRightButtonUp += AttachedControlOnPreviewMouseEvent;

                // disables the OS on-screen keyboard which is shown when controls gets keyboardfocus
                InputMethod.SetIsInputMethodEnabled(attachedControl, false);

                attachedControl.PreviewKeyDown += AttachedControlOnKeyDown;
                attachedControl.PreviewKeyUp += AttachedControlOnPreviewKeyUp;

                attachedControl.LostKeyboardFocus += AttachedControlOnLostKeyboardFocus;
            }
            if (mode == VirtualMode.Disabled)
            {
                SetIsKeyboardEnabled(attachedControl, false); //flag for binding

                if (attachedControl != null)
                {
                    attachedControl.IsManipulationEnabled = false;
                    attachedControl.ManipulationStarted -= AttachedControlOnManipulationStarting;
                    attachedControl.PreviewMouseDown -= AttachedControlOnMouseDown;
                    attachedControl.LostKeyboardFocus -= AttachedControlOnLostKeyboardFocus;

                    attachedControl.PreviewMouseRightButtonDown -= AttachedControlOnPreviewMouseEvent;
                    attachedControl.PreviewMouseRightButtonUp -= AttachedControlOnPreviewMouseEvent;
                    attachedControl.PreviewMouseLeftButtonDown -= AttachedControlOnPreviewMouseEvent;
                    attachedControl.PreviewMouseRightButtonUp -= AttachedControlOnPreviewMouseEvent;

                    attachedControl.PreviewKeyDown -= AttachedControlOnKeyDown;
                    attachedControl.PreviewKeyUp -= AttachedControlOnPreviewKeyUp;

                    if (ReferenceEquals(attachedControl, _currentOwner) && _popupInstance != null && _popupInstance.IsOpen)
                    {
                        _popupInstance.IsOpen = false;
                    }
                }
            }
        }
        private static void AttachedControlOnPreviewKeyUp(object o, KeyEventArgs keyEventArgs)
        {
            //when keyboard is used, we dont need to show the numpad
            if (ReferenceEquals(o, _currentOwner))
            {
                HideVirtualKeyPadPopup(o);
            }
        }
        private static void AttachedControlOnKeyDown(object o, KeyEventArgs e)
        {
            //to recapture mouse as the capture was lost when numpad was used
            if (ReferenceEquals(_currentOwner, o))
            {
                Mouse.Capture(o as FrameworkElement, CaptureMode.SubTree);
            }
        }
        private static void AttachedControlOnPreviewMouseEvent(object o, MouseButtonEventArgs e)
        {
            // click outside of numpad or attachedControl
            var frameworkElement = o as FrameworkElement;
            if (!ReferenceEquals(Mouse.Captured, _currentOwner) ||
                _currentOwner == null ||
                _popupInstance == null ||
                !_popupInstance.IsOpen ||
                IsMouseInBounds(frameworkElement, e.GetPosition(frameworkElement)))
            {
                return;
            }


            HideVirtualKeyPadPopup(o);

            var hitTestResult = TryGetVirtualKeyPadEnabledElementUnderMousePoint(o, e);
            if (hitTestResult != null)
            {
                Keyboard.Focus(hitTestResult as IInputElement);
                ShowVirtualKeyPadPopup(hitTestResult as FrameworkElement);
            }

            e.Handled = true;
        }
        private static DependencyObject TryGetVirtualKeyPadEnabledElementUnderMousePoint(object o, MouseEventArgs e)
        {
            //todo what about Popup?
            var parentWindow = ((UIElement)o).FindParent<Window>();
            var position = e.GetPosition(parentWindow);
            DependencyObject hitTestResult = null;
            VisualTreeHelper.HitTest(parentWindow, null, result =>
            {
                var x = result.VisualHit.FindParent(GetIsKeyboardEnabled, 10);
                if (x != null)
                {
                    hitTestResult = x;
                    return HitTestResultBehavior.Stop;
                }

                return HitTestResultBehavior.Continue;
            },
                new PointHitTestParameters(position));
            return hitTestResult;
        }
        private static void AttachedControlOnManipulationStarting(object sender, ManipulationStartedEventArgs e)
        {
            var attachedControl = sender as FrameworkElement;
            ShowVirtualKeyPadPopup(attachedControl);
            e.Cancel(); // to allow normal mouse events
        }
        private static void AttachedControlOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var attachedControl = sender as FrameworkElement;
            ShowVirtualKeyPadPopup(attachedControl);
        }
        private static void AttachedControlOnLostKeyboardFocus(object o, KeyboardFocusChangedEventArgs e)
        {
            HideVirtualKeyPadPopup(o);
        }
        private static void HideVirtualKeyPadPopup(object o)
        {
            //only hide if the "request" came from currentOwner
            if (_popupInstance != null && ReferenceEquals(_currentOwner, o))
            {
                Mouse.Capture(null);
                _currentOwner = null;
                _popupInstance.IsOpen = false;
            }
        }
        private static async void ShowVirtualKeyPadPopup(FrameworkElement attachedControl)
        {
            if (ReferenceEquals(attachedControl, _currentOwner) && _popupInstance != null && _popupInstance.IsOpen)
            {
                //already open
                return;
            }

            _currentOwner = attachedControl;

            _popupInstance = GetOrCreatePopup();

            //_keyboard.SetParentVisual(attachedControl);

            _popupInstance.IsOpen = false;
            _popupInstance.PlacementTarget = attachedControl;
            _popupInstance.Width = GetKeyboardWidth(attachedControl);
            _popupInstance.Height = GetKeyboardHeight(attachedControl);

            /*
			 * a delay to make sure that the existing popup is closed properly before the "new" one is opened
			 * This way the popup animations will show
			 */
            await Task.Delay(KeyboardPopupDelay);

            _popupInstance.IsOpen = true;
            Mouse.Capture(attachedControl, CaptureMode.SubTree);
        }
        private static Popup GetOrCreatePopup()
        {
            if (_popupInstance == null)
            {
                _keyboard = new VirtualKeyboard();
                _popupInstance = new Popup
                {
                    Child = _keyboard
                };
                if (_keyboard.Template.Resources.Contains(AttachedVirtualKeyboardStyle))
                {
                    _keyboard.Style = _keyboard.Template.Resources[AttachedVirtualKeyboardStyle] as Style;
                }

                if (_keyboard.Template.Resources.Contains(AttachedVirtualKeyboardPopupStyleKey))
                {
                    _popupInstance.Style = _keyboard.Template.Resources[AttachedVirtualKeyboardPopupStyleKey] as Style;
                }
                _keyboard.PreviewGotKeyboardFocus += (sender, args) => args.Handled = true;
                _keyboard.OkKeyClicked += (sender, args) => { HideVirtualKeyPadPopup(_currentOwner); };
                FocusManager.SetIsFocusScope(_popupInstance, true);
            }
            return _popupInstance;
        }
        private static bool IsMouseInBounds(FrameworkElement element, Point point)
        {
            var bounds = new Rect(0, 0, element.ActualWidth, element.ActualHeight);
            return bounds.Contains(point);
        }
    }
}
