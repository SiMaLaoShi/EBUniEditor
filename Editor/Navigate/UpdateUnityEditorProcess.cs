namespace EBA.Ebunieditor.Editor.Navigate
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
 
    public partial class UpdateUnityEditorProcess
    {
        public delegate bool EnumThreadWindowsCallback(IntPtr hWnd, IntPtr lParam);
 
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool EnumWindows(EnumThreadWindowsCallback callback, IntPtr extraData);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowThreadProcessId(HandleRef handle, out int processId);
 
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetWindow(HandleRef hWnd, int uCmd);
 
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool IsWindowVisible(HandleRef hWnd);
 
        [DllImport("user32.dll")]
        private static extern bool GetWindowText(int hWnd, StringBuilder title, int maxBufSize);
 
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private extern static int GetWindowTextLength(IntPtr hWnd);
 
        [DllImport("user32.dll", EntryPoint = "SetWindowText", CharSet = CharSet.Auto)]
        public extern static int SetWindowText(int hwnd, string lpString);
    }
}