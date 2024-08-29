using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows;

namespace VirtualKeys.Controls
{
    public class FunctionKey : KeyBase
    {
        public static readonly DependencyProperty VirtualKeyProperty =
          DependencyProperty.RegisterAttached("VirtualKey", typeof(Key), typeof(FunctionKey));

        public Key VirtualKey
        {
            get { return (Key)GetValue(VirtualKeyProperty); }
            set { SetValue(VirtualKeyProperty, value); }
        }
        static FunctionKey()
        { 
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FunctionKey), 
                new FrameworkPropertyMetadata(typeof(FunctionKey))); }

    }
}
