namespace EBA.Ebunieditor.Editor.Navigate
{
#if UNITY_EDITOR_WIN
    using UnityEditor;

    [InitializeOnLoad]
    class UpdateUnityEditorTitle
    {
        private static bool isInGame = false;

        static UpdateUnityEditorTitle()
        {
            EditorApplication.delayCall += DoUpdateTitleFunc;

            EditorApplication.playmodeStateChanged += OnPlaymodeStateChanged;
        }

        static void OnPlaymodeStateChanged()
        {
            if (EditorApplication.isPlaying == isInGame) return;
            isInGame = EditorApplication.isPlaying;
            UpdateUnityEditorProcess.lasttime = 0;
            DoUpdateTitleFunc();
        }

        static void DoUpdateTitleFunc()
        {
            //UnityEngine.Debug.Log("DoUpdateTitleFunc");
            UpdateUnityEditorProcess.Instance.SetTitle();
        }
    }
#endif
}