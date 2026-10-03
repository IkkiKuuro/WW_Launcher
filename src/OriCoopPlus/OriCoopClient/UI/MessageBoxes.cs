using System;
using System.Runtime.InteropServices;

namespace MP_Client.UI
{
    public enum MessageBoxButtons
    {
        Ok,
        OkCancel,
        AbortRetryIgnore,
        YesNoCancel,
        YesNo,
        RetryCancel
    }

    public enum MessageBoxDefaultButton
    {
        Button1 = 0,
        Button2 = 256,
        Button3 = 512
    }

    public enum MessageBoxIcon
    {
        None = 0,
        Hand = 16,
        Question = 32,
        Exclamation = 48,
        Asterisk = 64,
        Stop = 16,
        Error = 16,
        Warning = 48,
        Information = 64
    }

    public enum MessageBoxModal
    {
        Application = 0,
        System = 4096,
        Task = 8192
    }

    public enum MessageBoxResult
    {
        None,
        Ok,
        Cancel,
        Abort,
        Retry,
        Ignore,
        Yes,
        No
    }

    public static class WinMessageBox
    {
        [DllImport("user32.dll")]
        private static extern int MessageBoxA(IntPtr hWnd, string lpText, string lpCaption, uint uType);

        public static MessageBoxResult Show(string text, string caption, MessageBoxButtons buttons = MessageBoxButtons.Ok, MessageBoxIcon icon = MessageBoxIcon.None)
        {
            return (MessageBoxResult)MessageBoxA(IntPtr.Zero, text, caption, (uint)buttons | (uint)icon);
        }
    }
}
