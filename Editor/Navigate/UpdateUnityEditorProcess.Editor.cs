namespace EBA.Ebunieditor.Editor.Navigate
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Text;
    using UnityEngine;

    public partial class UpdateUnityEditorProcess
    {
        public IntPtr hwnd = IntPtr.Zero;
        private bool haveMainWindow = false;
        private IntPtr mainWindowHandle = IntPtr.Zero;
        private int processId = 0;
        private IntPtr hwCurr = IntPtr.Zero;
        private static StringBuilder sbtitle = new StringBuilder(255);
        private static string UTitle = System.Environment.CurrentDirectory;
        public static float lasttime = 0;

        private static UpdateUnityEditorProcess _instance;

        public static UpdateUnityEditorProcess Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new UpdateUnityEditorProcess();
                    _instance.hwnd = _instance.GetMainWindowHandle(Process.GetCurrentProcess().Id);
                }

                return _instance;
            }
        }

        public void SetTitle()
        {
            //UnityEngine.Debug.Log(string.Format("{0} - {1}", Time.realtimeSinceStartup, lasttime));
            if (Time.realtimeSinceStartup > lasttime)
            {
                sbtitle.Length = 0;
                lasttime = Time.realtimeSinceStartup + 2f;
                int length = GetWindowTextLength(hwnd);

                GetWindowText(hwnd.ToInt32(), sbtitle, 255);
                string strTitle = sbtitle.ToString();
                string[] ss = strTitle.Split('-');
                if (ss.Length > 0 && !strTitle.Contains(UTitle))
                {
                    // SetWindowText(hwnd.ToInt32(), $"{UTitle} - {strTitle}");
                    SetWindowText(hwnd.ToInt32(), $"{UTitle}");
                    UnityEngine.Debug.Log("Current Unity Title: " + UTitle);
                }
            }
        }

        public IntPtr GetMainWindowHandle(int processId)
        {
            if (!this.haveMainWindow)
            {
                this.mainWindowHandle = IntPtr.Zero;
                this.processId = processId;
                EnumThreadWindowsCallback callback = new EnumThreadWindowsCallback(this.EnumWindowsCallback);
                EnumWindows(callback, IntPtr.Zero);
                GC.KeepAlive(callback);

                this.haveMainWindow = true;
            }

            return this.mainWindowHandle;
        }

        private bool EnumWindowsCallback(IntPtr handle, IntPtr extraParameter)
        {
            int num;
            GetWindowThreadProcessId(new HandleRef(this, handle), out num);
            if ((num == this.processId) && this.IsMainWindow(handle))
            {
                this.mainWindowHandle = handle;
                return false;
            }

            return true;
        }

        private bool IsMainWindow(IntPtr handle)
        {
            return (!(GetWindow(new HandleRef(this, handle), 4) != IntPtr.Zero) &&
                    IsWindowVisible(new HandleRef(this, handle)));
        }
    }
}