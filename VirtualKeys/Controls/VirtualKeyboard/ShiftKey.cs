using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace VirtualKeys.Controls
{
    public class ShiftKey : KeyBase
    {
        public static readonly DependencyProperty IsShiftLockingProperty = 
            DependencyProperty.RegisterAttached("IsShiftLocking", typeof(object), typeof(ShiftKey),
               new PropertyMetadata(false));

        public bool IsShiftLocking
        {
            get { return (bool)GetValue(IsShiftLockingProperty); }
            set { SetValue(IsShiftLockingProperty, value); }
        }

        static ShiftKey()
        { DefaultStyleKeyProperty.OverrideMetadata(typeof(ShiftKey), new FrameworkPropertyMetadata(typeof(ShiftKey))); }

        protected override void OnClick()
        {
            var eventArgs = 
                new ModifierChangedRoutedEventArgs(KeyBase.ShiftModifierChangedProperty, this, !this.IsShifted, IsShiftLocking);
            RaiseEvent(eventArgs);
        }
    }
}
