using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace VirtualKeys.Controls
{
    public class CapsKey : KeyBase
    {
        static CapsKey()
        { DefaultStyleKeyProperty.OverrideMetadata(typeof(CapsKey), new FrameworkPropertyMetadata(typeof(CapsKey))); }

        protected override void OnClick()
        {
            var eventArgs = new ModifierChangedRoutedEventArgs(KeyBase.CapsModifierChangedProperty, this, !this.IsCapsLocked, true);
            RaiseEvent(eventArgs);
        }
    }
}
