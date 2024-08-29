using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VirtualKeys.Controls
{
    [TypeConverter(typeof(EnumConverter))]
    [Serializable]
    [Flags]
    public enum VirtualMode
    {
        Disabled = 0,
        Touch = 1,
        Mouse = 2,
        TouchAndMouse = Touch | Mouse
    }
}
