﻿using UnityEngine;

namespace EBA.Ebunieditor.Editor.Common
{
    public class GlobalScriptableObject : BaseScriptable<GlobalScriptableObject>
    {
        [Header("第三方程序路径相关")]
        public string strNotePad = @"notepad.exe";
        public string strNotePadPpPath = @"C:\Program Files (x86)\Notepad++\notepad++.exe";
        public string strSublimePath = @"C:\Program Files\Sublime Text 3\sublime_text.exe";

        [Header("Hook相关")]
        public bool isHookApplication = false;
        public bool isHookStreamingAssetsPath = false;
        public string strRemoteStreamingAssetsPath = "";
        public bool isHookPersistentDataPath = false;
        public string strRemotePersistentDataPath = "";

        [Header("UGUI相关")]
        //UGUI相关
        public Font font;
        public int fontSize = 30;
        public string textDefault = "New Text";
        public Color textDefaultColor = Color.white;
    }
}