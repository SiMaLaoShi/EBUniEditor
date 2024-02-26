using System;

namespace EBA.Ebunieditor.Editor.Analysis.Crash
{
    using UnityEngine;
    using UnityEditor;
    using System.Diagnostics;
    using System.IO;
    public class Addr2LineToolWindow : EditorWindow
    {
        private string _soFilePath;
        private string _logFilePath;
        private string _addr2LinePath;

        [MenuItem("Window/EBAnalysis/Addr2Line")]
        static void Init()
        {
            Addr2LineToolWindow window = (Addr2LineToolWindow)EditorWindow.GetWindow(typeof(Addr2LineToolWindow));
            window.Show();
        }

        private void OnEnable()
        {
            _soFilePath = EditorPrefs.GetString("Addr2Line_SOFilePath");
            _logFilePath = EditorPrefs.GetString("Addr2Line_LogFilePath");
            _addr2LinePath = EditorPrefs.GetString("Addr2Line_Addr2LinePath");
        }

        private void OnDestroy()
        {
            EditorPrefs.SetString("Addr2Line_SOFilePath", _soFilePath);
            EditorPrefs.SetString("Addr2Line_LogFilePath", _logFilePath);
            EditorPrefs.SetString("Addr2Line_Addr2LinePath", _addr2LinePath);
        }

        void OnGUI()
        {
            GUILayout.Label("Paths for Crash Log Analysis", EditorStyles.boldLabel);

            _soFilePath = EditorGUILayout.TextField("Path to .so file:", _soFilePath);
            _logFilePath = EditorGUILayout.TextField("Path to log file:", _logFilePath);
            _addr2LinePath = EditorGUILayout.TextField("Path to addr2line:", _addr2LinePath);

            if (GUILayout.Button("Analyze Crash Log"))
            {
                AnalyzeCrashLog();
            }
        }

        private void AnalyzeCrashLog()
        {
            // Save paths using EditorPrefs
            EditorPrefs.SetString("Addr2Line_SOFilePath", _soFilePath);
            EditorPrefs.SetString("Addr2Line_LogFilePath", _logFilePath);
            EditorPrefs.SetString("Addr2Line_Addr2LinePath", _addr2LinePath);

            // Analysis logic using Process
            Process process = new Process();
            process.StartInfo.FileName = _addr2LinePath;
            process.StartInfo.Arguments = $"-e \"{_soFilePath}\" -f -p -C -i @{_logFilePath}";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;

            try
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                // Output the results
                UnityEngine.Debug.Log("Crash Log Analysis Result: \n" + output);
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError("Failed to analyze crash log: " + e.Message);
            }
        }
    }
}