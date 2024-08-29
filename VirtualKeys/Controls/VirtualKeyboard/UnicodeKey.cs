using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using WindowsInput;

namespace VirtualKeys.Controls
{
    public class UnicodeKey : KeyBase
    {
        public static readonly DependencyProperty UnshiftedTextProperty =
          DependencyProperty.RegisterAttached("UnshiftedText", typeof(string), typeof(UnicodeKey));

        public static readonly DependencyProperty ShiftedUnicodeTextProperty =
            DependencyProperty.RegisterAttached("ShiftedUnicodeText", typeof(string), typeof(UnicodeKey));

        public string UnshiftedText
        {
            get { return (string)GetValue(UnshiftedTextProperty); }
            set { SetValue(UnshiftedTextProperty, value); }
        }

        public string ShiftedUnicodeText
        {
            get { return (string)GetValue(ShiftedUnicodeTextProperty); }
            set { SetValue(ShiftedUnicodeTextProperty, value); }
        }

        static UnicodeKey()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(UnicodeKey), new FrameworkPropertyMetadata(typeof(UnicodeKey)));
            ShiftOnCapsLockProperty.OverrideMetadata(typeof(UnicodeKey), new FrameworkPropertyMetadata(true));
        }

        protected override void OnClick()
        {
            var capsMatters = ShiftOnCapsLock && IsCapsLocked;
            var setShiftMode = IsShifted ^ capsMatters;

            var sim = new InputSimulator();
            if (setShiftMode && !string.IsNullOrEmpty(ShiftedUnicodeText))
            { sim.Keyboard.TextEntry(ShiftedUnicodeText); }
            else if (!string.IsNullOrEmpty(UnshiftedText))
            { sim.Keyboard.TextEntry(UnshiftedText); }

            base.OnClick();
        }
    }
}
