using EBA.Ebunieditor.Editor.Common;

namespace EBA.Ebunieditor.Editor.Analysis.Crash
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    public enum PathType
    {
        unitySoPath = 1,
        il2cppSoPath,
        addr2Line
    }

    public class AnalysisCrashWindow : EditorWindow
    {
        bool groupEnabled;

        //path
        string addr2linePath = string.Empty; //ndk解析工具路径
        string il2cppdebugsoPath = string.Empty; //android 符号表路径
        string unitydebugsoPath = string.Empty; //unity  符合表路径

        string MyCashPath = string.Empty;

        //flag
        string il2cppflag = "libil2cpp";
        string unityflag = "libunity";
        string crashEndFlag = "libunity.so";

        string unityPath =
            @"\Data\PlaybackEngines\AndroidPlayer\Variations\mono\Release\Symbols\armeabi-v7a\libunity.sym.so";

        bool isAnalysis = false; //文件是否解析

        private void OnEnable()
        {
            GetPathByMemory();
        }

        [MenuItem("Window/EBAnalysis/AnalyzeCrashWindow")]
        static void Init()
        {
            AnalysisCrashWindow window = (AnalysisCrashWindow) EditorWindow.GetWindow(typeof(AnalysisCrashWindow));
            window.Show();
        }

        void OnGUI()
        {
            groupEnabled = EditorGUILayout.BeginToggleGroup("基础设置", groupEnabled);
            addr2linePath = EditorGUILayout.TextField("NDK工具路径（addr2linePath）", addr2linePath, GUILayout.Width(400));
            unitydebugsoPath =
                EditorGUILayout.TextField("unity符号表（unitydebugsoPath）", unitydebugsoPath, GUILayout.MaxWidth(400));
            il2cppdebugsoPath = EditorGUILayout.TextField("ill2cpp符合表（il2cppdebugsoPath）", il2cppdebugsoPath,
                GUILayout.MaxWidth(400));
            MyCashPath = EditorGUILayout.TextField("崩溃日志文件路径", MyCashPath, GUILayout.MaxWidth(400));

            EditorGUILayout.EndToggleGroup();
            GUILayout.Label("检索内容", EditorStyles.boldLabel);
            GetCrashByPath(MyCashPath);
        }

        /// <summary>
        /// 从内存中获取存储的路径
        /// </summary>
        void GetPathByMemory()
        {
            addr2linePath = EditorPrefs.GetString("addr2linePath");
            il2cppdebugsoPath = EditorPrefs.GetString("il2cppdebugsoPath");
            unitydebugsoPath = EditorPrefs.GetString("unitydebugsoPath");
            if (string.IsNullOrEmpty(unitydebugsoPath))
            {
                unitydebugsoPath = string.Concat(System.AppDomain.CurrentDomain.BaseDirectory, unityPath);
                JudgePath(PathType.unitySoPath, unitydebugsoPath);
            }

            MyCashPath = EditorPrefs.GetString("MyCashPath", MyCashPath);
        }

        /// <summary>
        /// 路径判断
        /// </summary>
        /// <param name="type">路径类型</param>
        /// <param name="path"></param>
        bool JudgePath(PathType type, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            bool temp = true;
            if ((int) type == 1)
            {
                if (!path.EndsWith("libunity.sym.so"))
                {
                    path = string.Empty;
                    Debug.LogError("自动添加unity符合表路径出错，请手动添加");
                    temp = false;
                }
                else
                {
                    if (!File.Exists(path))
                    {
                        temp = false;
                        Debug.LogErrorFormat("当前路径{0}unity符号表不存在", path);
                    }
                }
            }
            else if ((int) type == 2)
            {
                if (!path.EndsWith("libil2cpp.dbg.so"))
                {
                    temp = false;
                }
                else
                {
                    if (!File.Exists(path))
                    {
                        temp = false;
                    }
                }
            }
            else
            {
                if (!path.EndsWith("aarch64-linux-android-addr2line.exe"))
                {
                    temp = false;
                }
                else
                {
                    if (!File.Exists(path))
                    {
                        temp = false;
                    }
                }
            }

            return temp;
        }

        /// <summary>
        /// 创建Button
        /// </summary>
        /// <param name="name"></param>
        /// <param name="path"></param>
        void CreatorButton(string name, string path)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField("名称", name, GUILayout.MaxWidth(400));
            GUILayout.Space(10);
            if (GUILayout.Button("解析", GUILayout.Width(50)))
            {
                if (!JudgePath(PathType.addr2Line, addr2linePath))
                {
                    Debug.LogError("Ndk解析路径出错");
                    return;
                }

                if (!JudgePath(PathType.unitySoPath, unitydebugsoPath) &&
                    !JudgePath(PathType.il2cppSoPath, il2cppdebugsoPath))
                {
                    Debug.LogError("unity与il2cppSoPanth符合表路径出错");
                    return;
                }

                if (!JudgePath(PathType.il2cppSoPath, il2cppdebugsoPath))
                {
                    Debug.LogError("il2cppSoPanth符合表路径出错");
                }

                OutCrash(name, path);
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 根据获取Crash文件的文件创建Button与显示框
        /// </summary>
        /// <param name="path"></param>
        void GetCrashByPath(string path)
        {
            if (Directory.Exists(path))
            {
                var dirctory = new DirectoryInfo(path);
                var files = dirctory.GetFiles("*", SearchOption.AllDirectories);
                foreach (var fi in files)
                {
                    CreatorButton(fi.Name, path);
                }
            }
        }

        /// <summary>
        /// 打开Crash
        /// </summary>
        void OutCrash(string filename, string path)
        {
            isAnalysis = false;
            string filePath = string.Join("/", path, filename);
            var allLines = File.ReadAllLines(filePath);
            // using (StreamReader sr = new StreamReader(filePath))
            // {
            //     while (!sr.EndOfStream)
            //     {
            //         
            //     }
            // }

            for (int i = 0; i < allLines.Length; i++)
            {
                OutCmd(allLines[i]);
                CommonUtility.UpdateProgress(i, allLines.Length, allLines[i]);
            }
            
            CommonUtility.UpdateProgress(allLines.Length, allLines.Length, "解析完成");
            EditorUtility.ClearProgressBar();

            if (!isAnalysis)
            {
                Debug.LogError("无法解析当前cash文件，请检查文件是否为设备崩溃日志");
            }
        }

        /// <summary>
        /// 解析Crash
        /// </summary>
        void OutCmd(string log)
        {
            if (log == null)
            {
                return;
            }

            if (log.EndsWith(crashEndFlag)) //找以libunity.so结尾的崩溃日志
            {
                if (log.Contains("pc"))
                {
                    int startIndex = log.IndexOf("pc") + 3;
                    if (log.Contains("/data/"))
                    {
                        int endIndex = log.IndexOf("/data/");
                        string addStr = log.Substring(startIndex, endIndex - startIndex - 1);
                        string tempUnitySoPath = string.Format("\"{0}\"", unitydebugsoPath);
                        ExecuteCmd(tempUnitySoPath, addStr);
                    }
                }
            }
            else //找 il2cpp和libunity 崩溃日志
            {
                if (log.Contains(il2cppflag) && JudgePath(PathType.il2cppSoPath, il2cppdebugsoPath))
                {
                    string tempill2cppSoPath = string.Format("\"{0}\"", il2cppdebugsoPath);
                    FindMiddleCrash(log, il2cppflag, tempill2cppSoPath);
                }
                else if (log.Contains(unityflag))
                {
                    string tempUnitySoPath = string.Format("\"{0}\"", unitydebugsoPath);
                    FindMiddleCrash(log, unityflag, tempUnitySoPath);
                }
            }
        }

        /// <summary>
        /// 找 il2cpp和libunity 崩溃日志
        /// </summary>
        /// <param name="log"></param>
        /// <param name="debugFlag">标志元素</param>
        /// <param name="SoPath">符号表路径</param>
        void FindMiddleCrash(string log, string debugFlag, string SoPath)
        {
            if (!string.IsNullOrEmpty(SoPath))
            {
                int startIndex = log.IndexOf(debugFlag);
                startIndex = startIndex + debugFlag.Length + 1;
                if (log.Contains("("))
                {
                    int endIndex = log.IndexOf("(");
                    if (endIndex > 0)
                    {
                        if (endIndex - startIndex > 0)
                        {
                            string addStr = log.Substring(startIndex, endIndex - startIndex);
                            ExecuteCmd(SoPath, addStr);
                        }
                    }
                }
            }
            else
            {
                Debug.LogErrorFormat("{0}的符号表路径为空", debugFlag);
            }
        }


        /// <summary>
        /// 执行CMD命令
        /// </summary>
        /// <param name="SoPath">符号表路径</param>
        /// <param name="addStr">崩溃代码地址</param>
        void ExecuteCmd(string soPath, string addStr)
        {
            string cmdStr = string.Join(" ", addr2linePath, "-f", "-C", "-e", soPath, addStr);
            CmdHandler.RunCmd(cmdStr, (str) =>
            {
                Debug.Log(string.Format("解析后{0}", ResultStr(str, addStr)));
                isAnalysis = true;
            });
        }

        /// <summary>
        /// 对解析结果进行分析
        /// </summary>
        /// <param name="str"></param>
        /// <param name="addStr"></param>
        /// <returns></returns>
        string ResultStr(string str, string addStr)
        {
            string tempStr = string.Empty;
            if (!string.IsNullOrEmpty(str))
            {
                if (str.Contains("exit"))
                {
                    int startIndex = str.IndexOf("exit");
                    if (startIndex < str.Length)
                    {
                        tempStr = str.Substring(startIndex);
                        if (tempStr.Contains(")"))
                        {
                            startIndex = tempStr.IndexOf("t") + 1;
                            int endIndex = tempStr.LastIndexOf(")");
                            tempStr = tempStr.Substring(startIndex, endIndex - startIndex + 1);
                            tempStr = string.Format("<color=red>[{0}]</color> :<color=yellow>{1}</color>", addStr,
                                tempStr);
                        }
                        else
                        {
                            startIndex = tempStr.IndexOf("t") + 1;
                            tempStr = tempStr.Substring(startIndex);
                            tempStr = string.Format("<color=red>[{0}]</color> :<color=yellow>{1}</color>", addStr,
                                tempStr);
                        }
                    }
                }
                else
                {
                    Debug.LogErrorFormat("当前结果未执行cmd命令", str);
                }
            }
            else
            {
                Debug.LogErrorFormat("执行cmd:{0}命令，返回值为空", str);
            }

            return tempStr;
        }

        private void OnDestroy()
        {
            EditorPrefs.SetString("addr2linePath", addr2linePath);
            EditorPrefs.SetString("il2cppdebugsoPath", il2cppdebugsoPath);
            EditorPrefs.SetString("unitydebugsoPath", unitydebugsoPath);
            EditorPrefs.SetString("MyCashPath", MyCashPath);
        }
    }
}