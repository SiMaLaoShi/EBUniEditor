namespace EBA.Ebunieditor.Editor.Analysis.Crash
{
#if UNITY_EDITOR
    using UnityEditor;
    using System.Diagnostics;
    using UnityEngine;

    public class NdkStackWindow : EditorWindow
    {
        private string ndkStackPath = "";
        private string crashLogPath = "";
        private string symbolsPath = "";
        private string ndkStackOutput = "";

        [MenuItem("Window/EBAnalysis/Ndk Stack")]
        public static void ShowWindow()
        {
            GetWindow<NdkStackWindow>("Ndk Stack");
        }

        void OnGUI()
        {
            GUILayout.Label("NDK Stack Configuration", EditorStyles.boldLabel);

            ndkStackPath = EditorGUILayout.TextField("NDK Stack Path", ndkStackPath);
            crashLogPath = EditorGUILayout.TextField("Crash Log Path", crashLogPath);
            symbolsPath = EditorGUILayout.TextField("Symbols Path", symbolsPath);

            if (GUILayout.Button("Run Ndk Stack"))
            {
                RunNdkStack();
            }

            GUILayout.Label("NDK Stack Output");
            EditorGUILayout.TextArea(ndkStackOutput, GUILayout.Height(200));
        }

        private void RunNdkStack()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = ndkStackPath,
                Arguments = $"-sym {symbolsPath} -dump {crashLogPath}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(startInfo))
            {
                using (var reader = process.StandardOutput)
                {
                    ndkStackOutput = reader.ReadToEnd();
                }
            }
        }
    }
#endif
}