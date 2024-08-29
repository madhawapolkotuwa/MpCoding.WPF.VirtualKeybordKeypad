using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Interop;
using VirtualKeys.Commands;

namespace VirtualKeys.Controls
{
    public partial class VirtualKeyPad : Control
    {
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

        #region ctor

        static VirtualKeyPad()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VirtualKeyPad), new FrameworkPropertyMetadata(typeof(VirtualKeyPad)));
        }

        public VirtualKeyPad()
        {
            Loaded += OnLoaded;
        }

        #endregion

        #region methods

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

        #endregion
    }
}
