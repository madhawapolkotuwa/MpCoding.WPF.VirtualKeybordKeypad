using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace VirtualKeys.Controls
{
    public class BackspaceKey : KeyBase
    {
        private DispatcherTimer _timer;

        static BackspaceKey()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BackspaceKey),
                new FrameworkPropertyMetadata(typeof(BackspaceKey)));
            ClickModeProperty.OverrideMetadata(typeof(BackspaceKey), new FrameworkPropertyMetadata(ClickMode.Press));
        }

        public static readonly DependencyProperty DelayProperty =
            DependencyProperty.Register("Delay", typeof(int), typeof(BackspaceKey),
                new FrameworkPropertyMetadata(GetKeybordDelay()), IsDelayValid);

        public static readonly DependencyProperty IntervalProperty =
            DependencyProperty.Register("Interval", typeof(int), typeof(BackspaceKey),
                new FrameworkPropertyMetadata(GetKeybordSpeed()), IsIntervalValid);

        [Bindable(true)]
        [Category("Behavior")]
        public int Delay
        {
            get { return (int)GetValue(DelayProperty); }
            set { SetValue(DelayProperty, value); }
        }

        [Bindable(true)]
        [Category("Behavior")]
        public int Interval
        {
            get { return (int)GetValue(IntervalProperty); }
            set { SetValue(IntervalProperty, value); }
        }

        private static bool IsDelayValid(object value)
        {
            return (int)value >= 0;
        }
        private static int GetKeybordDelay()
        {
            var delay = SystemParameters.KeyboardDelay;
            // SPI_GETKEYBOARDDELAY 0,1,2,3 correspond to 250,500,750,1000ms
            if (delay < 0 || delay > 3)
            {
                delay = 0;
            }
            return (delay + 1) * 250;
        }
        private static bool IsIntervalValid(object value)
        {
            return (int)value > 0;
        }
        private static int GetKeybordSpeed()
        {
            var speed = SystemParameters.KeyboardSpeed;
            // SPI_GETKEYBOARDSPEED 0,...,31 correspond to 1000/2.5=400,...,1000/30 ms
            if (speed < 0 || speed > 31)
            {
                speed = 31;
            }
            return (31 - speed) * (400 - 1000 / 30) / 31 + 1000 / 30;
        }

        #region Override methods
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (IsPressed && ClickMode != ClickMode.Hover)
            {
                StartTimer();
            }
        }
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            StopTimer();
        }
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            StopTimer();
        }
        private void StopTimer()
        {
            if (_timer != null && _timer.IsEnabled)
            {
                _timer.Stop();
            }
        }
        private void StartTimer()
        {
            if (_timer == null)
            {
                _timer = new DispatcherTimer();
                _timer.Tick += OnTimeout;
            }
            else if (_timer.IsEnabled)
            {
                return;
            }

            _timer.Interval = TimeSpan.FromMilliseconds(Delay);
            _timer.Start();
        }
        private void OnTimeout(object sender, EventArgs e)
        {
            var interval = TimeSpan.FromMilliseconds(Interval);
            if (_timer.Interval != interval)
            {
                _timer.Interval = interval;
            }
            base.OnClick();
        }
        #endregion
    }
}
