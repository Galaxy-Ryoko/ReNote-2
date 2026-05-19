using System;
using System.Runtime.InteropServices;
using Windows.System;

namespace Renote_2.Helpers
{
    /// <summary>
    /// Sends key chords to the focused control (e.g. editor undo/redo).
    /// </summary>
    internal static class KeyboardShortcutHelper
    {
        private const uint KeyEventKeyUp = 0x0002;

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        public static void SendCtrlChord(VirtualKey key)
        {
            byte ctrl = (byte)VirtualKey.Control;
            byte vk = (byte)key;

            keybd_event(ctrl, 0, 0, UIntPtr.Zero);
            keybd_event(vk, 0, 0, UIntPtr.Zero);
            keybd_event(vk, 0, KeyEventKeyUp, UIntPtr.Zero);
            keybd_event(ctrl, 0, KeyEventKeyUp, UIntPtr.Zero);
        }
    }
}
