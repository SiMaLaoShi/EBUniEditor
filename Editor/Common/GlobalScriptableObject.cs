namespace EBA.Ebunieditor.Editor.Common
{
    public class GlobalScriptableObject : BaseScriptable<GlobalScriptableObject>
    {
        public string strNotePad = @"notepad.exe";
        public string strNotePadPpPath = @"C:\Program Files (x86)\Notepad++\notepad++.exe";
        public string strSublimePath = @"C:\Program Files\Sublime Text 3\sublime_text.exe";
        public bool isUseAni = false;
        public bool isHookApplication = false;
        public bool isHookStreamingAssetsPath = false;
        public string strRemoteStreamingAssetsPath = "";
        public bool isHookPersistentDataPath = false;
        public string strRemotePersistentDataPath = "";
    }
}