using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using VirtualKeys.Commands;

namespace VirtualKeys.Controls
{
    [TemplatePart(Name = ContentPresenterPart, Type = typeof(ContentControl))]

    public partial class VirtualKeyboard : ContentControl
    {
        public const string ContentPresenterPart = "PART_ContentPresenter";

        #region dll imports
        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);
        #endregion

        #region fields
        private const int WmKeyDown = 0x0100;
        private IntPtr _handle;

        private ICommand _keyPressCommand;
        #endregion

        #region properties
        public event EventHandler OkKeyClicked;

        public ICommand KeyPressCommand
        {
            get { return _keyPressCommand ?? (_keyPressCommand = new CommandBase(ExecuteKeyPress)); }
        }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty IsShiftLockedProperty =
            DependencyProperty.RegisterAttached("IsShiftLocked", typeof(bool), typeof(VirtualKeyboard));

        public static readonly DependencyProperty IsShiftedProperty =
            DependencyProperty.RegisterAttached("IsShifted", typeof(bool), typeof(VirtualKeyboard), new PropertyMetadata(OnIsShiftedChanged));

        public static readonly DependencyProperty IsCapsLockedProperty =
            DependencyProperty.RegisterAttached("IsCapsLocked", typeof(bool), typeof(VirtualKeyboard), new PropertyMetadata(OnIsCapsLockedChanged));
        #endregion

        private ContentPresenter _contentControl;


        #region Properties
        public bool IsShifted
        {
            get { return (bool)GetValue(IsShiftedProperty); }
            set { SetValue(IsShiftedProperty, value); }
        }

        public bool IsShiftLocked
        {
            get { return (bool)GetValue(IsShiftLockedProperty); }
            set { SetValue(IsShiftLockedProperty, value); }
        }

        public bool IsCapsLocked
        {
            get { return (bool)GetValue(IsCapsLockedProperty); }
            set { SetValue(IsCapsLockedProperty, value); }
        }
        #endregion

        static VirtualKeyboard()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VirtualKeyboard), new FrameworkPropertyMetadata(typeof(VirtualKeyboard)));

            EventManager.RegisterClassHandler(typeof(VirtualKeyboard), KeyBase.ShiftModifierChangedProperty, new ModifierChangedRoutedEventHandler(OnShiftModified));
            EventManager.RegisterClassHandler(typeof(VirtualKeyboard), KeyBase.CapsModifierChangedProperty, new ModifierChangedRoutedEventHandler(OnCapsModified));
            EventManager.RegisterClassHandler(typeof(VirtualKeyboard), KeyBase.ClickEvent, new RoutedEventHandler(OnKeyClicked));
        }

        public VirtualKeyboard()
        {
            Focusable = false;
            IsTabStop = false;

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var source = (HwndSource)PresentationSource.FromVisual(this);
            if (source != null)
            {
                _handle = source.Handle;
            }
        }

        protected virtual void OnOkKeyClicked()
        {
            var handle = OkKeyClicked;
            if (handle != null)
            {
                handle(this, EventArgs.Empty);
            }
        }

        private void ExecuteKeyPress(object obj)
        {
            var key = (Key)obj;
            PostMessage(_handle, WmKeyDown, KeyInterop.VirtualKeyFromKey(key), 0);
            if (key == Key.Enter)
            {
                OnOkKeyClicked();
            }
        }

        public void SetParentVisual(Visual parent)
        {
            var source = (HwndSource)PresentationSource.FromVisual(parent);
            if (source != null)
            {
                this._handle = source.Handle;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                _contentControl = GetTemplateChild(ContentPresenterPart) as ContentPresenter;
            }
        }

        private static void OnShiftModified(object sender, ModifierChangedRoutedEventArgs e)
        {
            var keyboard = (VirtualKeyboard)sender;
            keyboard.IsShifted = e.Applied;
            keyboard.IsShiftLocked = e.Locked;
            e.Handled = true;
        }

        private static void OnCapsModified(object sender, ModifierChangedRoutedEventArgs e)
        {
            var keyboard = (VirtualKeyboard)sender;
            keyboard.IsCapsLocked = e.Applied;
            e.Handled = true;
        }

        private static void OnKeyClicked(object sender, RoutedEventArgs e)
        {
            // we will only react to changing shift behavior if the key pressed
            // was a keyboard key
            if (e.OriginalSource is KeyBase)
            {
                var key = (KeyBase)e.OriginalSource;
                var keyboard = (VirtualKeyboard)sender;

                if (key.CommandParameter != null)
                {
                    var parm = (Key)key.CommandParameter;
                    if (parm != null && parm == Key.Back)
                    {
                        keyboard.ExecuteKeyPress(parm);
                    }
                }


                if (!keyboard.IsShiftLocked && keyboard.IsShifted)
                { keyboard.IsShifted = false; }
            }
        }

        private static void OnIsShiftedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var isShifted = (bool)e.NewValue;
            var allKeys = FindVisualChildren<KeyBase>(obj);
            foreach (var key in allKeys)
            { key.IsShifted = isShifted; }
        }

        private static void OnIsCapsLockedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var isCapsLocked = (bool)e.NewValue;
            var allKeys = FindVisualChildren<KeyBase>(obj);
            foreach (var key in allKeys)
            { key.IsCapsLocked = isCapsLocked; }
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    var child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    { yield return (T)child; }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    { yield return childOfChild; }
                }
            }
        }
    }
}
