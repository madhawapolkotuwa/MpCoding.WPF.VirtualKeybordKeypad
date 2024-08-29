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
    public partial class VirtualKeyPad
    {
        #region constants
        private const string PopupStyle = "AttachedVirtualKeyPadPopupStyle";
        private const string KeyPadStyle = "AttachedVirtualKeyPadStyle";
        #endregion

        #region fields
        private static Popup _popupInstance;
        private static VirtualKeyPad _keyPad;
        private static FrameworkElement _currentOwner;
        private static ComponentResourceKey _virtualKeyPadPopupStyleKey;
        private static ComponentResourceKey _attachedVirtualKeyPadStyle;
        #endregion

        public static ComponentResourceKey AttachedVirtualKeyPadPopupStyleKey
        {
            get
            {
                return _virtualKeyPadPopupStyleKey ??
                    (_virtualKeyPadPopupStyleKey = new ComponentResourceKey(typeof(VirtualKeyPad), PopupStyle));
            }
        }

        public static ComponentResourceKey AttachedVirtualKeyPadStyle
        {
            get
            {
                return _attachedVirtualKeyPadStyle ??
                       (_attachedVirtualKeyPadStyle = new ComponentResourceKey(typeof(VirtualKeyPad), KeyPadStyle));
            }
        }

        public static readonly DependencyProperty IsKeyPadEnabledProperty =
            DependencyProperty.RegisterAttached("IsKeyPadEnabled", typeof(bool), typeof(VirtualKeyPad),
                new PropertyMetadata(false));

        public static readonly DependencyProperty KeyPadWidthProperty =
            DependencyProperty.RegisterAttached("KeyPadWidth", typeof(double), typeof(VirtualKeyPad),
                new PropertyMetadata(330d));

        public static readonly DependencyProperty KeyPadHeightProperty =
            DependencyProperty.RegisterAttached("KeyPadHeight", typeof(double), typeof(VirtualKeyPad),
                new PropertyMetadata(330d));

        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.RegisterAttached("Mode", typeof(VirtualMode), typeof(VirtualKeyPad),
                new PropertyMetadata(VirtualMode.Disabled, OnModeChanged));

        private static int KeyPadPopupDelay
        {
            get { return 50; }
        }

        public static bool GetIsKeyPadEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsKeyPadEnabledProperty);
        }

        internal static void SetIsKeyPadEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsKeyPadEnabledProperty, value);
        }

        public static double GetKeyPadWidth(DependencyObject obj)
        {
            return (double)obj.GetValue(KeyPadWidthProperty);
        }

        public static void SetKeyPadWidth(DependencyObject obj, double value)
        {
            obj.SetValue(KeyPadWidthProperty, value);
        }

        public static double GetKeyPadHeight(DependencyObject obj)
        {
            return (double)obj.GetValue(KeyPadHeightProperty);
        }

        public static void SetKeyPadHeight(DependencyObject obj, double value)
        {
            obj.SetValue(KeyPadHeightProperty, value);
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
                SetIsKeyPadEnabled(attachedControl, true);  //flag for binding

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
                SetIsKeyPadEnabled(attachedControl, false); //flag for binding

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
                var x = result.VisualHit.FindParent(GetIsKeyPadEnabled, 10);
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

            _keyPad.SetParentVisual(attachedControl);

            _popupInstance.IsOpen = false;
            _popupInstance.PlacementTarget = attachedControl;
            _popupInstance.Width = GetKeyPadWidth(attachedControl);
            _popupInstance.Height = GetKeyPadHeight(attachedControl);

            /*
			 * a delay to make sure that the existing popup is closed properly before the "new" one is opened
			 * This way the popup animations will show
			 */
            await Task.Delay(KeyPadPopupDelay);

            _popupInstance.IsOpen = true;
            Mouse.Capture(attachedControl, CaptureMode.SubTree);
        }

        private static Popup GetOrCreatePopup()
        {
            if (_popupInstance == null)
            {
                _keyPad = new VirtualKeyPad();
                _popupInstance = new Popup
                {
                    Child = _keyPad
                };
                if (_keyPad.Template.Resources.Contains(AttachedVirtualKeyPadStyle))
                {
                    _keyPad.Style = _keyPad.Template.Resources[AttachedVirtualKeyPadStyle] as Style;
                }

                if (_keyPad.Template.Resources.Contains(AttachedVirtualKeyPadPopupStyleKey))
                {
                    _popupInstance.Style = _keyPad.Template.Resources[AttachedVirtualKeyPadPopupStyleKey] as Style;
                }
                _keyPad.PreviewGotKeyboardFocus += (sender, args) => args.Handled = true;
                _keyPad.OkKeyClicked += (sender, args) => { HideVirtualKeyPadPopup(_currentOwner); };
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
