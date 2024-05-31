using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using EBA.LitJson;
using Debug = UnityEngine.Debug;

namespace EBA.Ebunieditor.Editor.Spine
{
    public static class TaskHelper
    {
        public static void MoveFilesToGroupFolders(List<string> fileList, string outputFolderPath)
        {
            foreach (string filePath in fileList)
            {
                string fileName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(filePath));
                string folderPath = Path.Combine(outputFolderPath, fileName);

                // 创建以文件名命名的文件夹
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                if (!Directory.Exists(Path.Combine(folderPath, "src")))
                {
                    Directory.CreateDirectory(Path.Combine(folderPath, "src"));
                }

                if (!Directory.Exists(Path.Combine(folderPath, "gif")))
                {
                    Directory.CreateDirectory(Path.Combine(folderPath, "gif"));
                }

                if (!Directory.Exists(Path.Combine(folderPath, "export")))
                {
                    Directory.CreateDirectory(Path.Combine(folderPath, "export"));
                }

                // 移动文件到对应文件夹
                string newFilePath = Path.Combine(folderPath, Path.GetFileName(filePath));
                File.Copy(filePath, newFilePath, true);
                Debug.Log($"update path {filePath} ==> {newFilePath}");
                // 去除 ".prefab" 扩展名
                // string newFileName = Path.ChangeExtension(newFilePath, null);
                // File.Copy(newFilePath, newFileName, true);
                // Debug.Log($"update name {newFilePath} ==> {newFileName}");
            }
        }

        public static string NormalizePath(string path)
        {
            return path.Replace("\\", "/");
        }
        
        public static void SpineMkdir(string path, bool delete = false)
        {
            // 使用System.IO的CreateDirectory方法创建文件夹
            // 注意：如果路径中的文件夹已经存在，该方法不会抛出异常
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
                Directory.CreateDirectory(path);
                UnityEngine.Debug.Log($"已成功创建或确认存在路径: {path}");
            }
            catch (Exception ex)
            {
                // 如果在创建目录过程中遇到任何错误，这里会捕捉到异常
                UnityEngine.Debug.LogError($"创建目录时出现错误: {ex.Message}");
            }
        }
        
        public static string FormatJsonData(JsonData jsonData)
        {
            var writer = new JsonWriter() { PrettyPrint = true, IndentValue = 1 };
            JsonMapper.ToJson(jsonData, writer);
            return writer.ToString();
        }
        
        public static string FormatJsonData(object obj)
        {
            var writer = new JsonWriter() { PrettyPrint = true, IndentValue = 1 };
            JsonMapper.ToJson(obj, writer);
            return writer.ToString();
        }

        public static void RunBat(string batPath, string workingDirectory = null)
        {
            Process pro = new Process();

            // 根据是否提供工作目录来设置ProcessStartInfo的工作目录
            if (string.IsNullOrEmpty(workingDirectory))
            {
                FileInfo file = new FileInfo(batPath);
                pro.StartInfo.WorkingDirectory = file.Directory.FullName;
            }
            else
            {
                pro.StartInfo.WorkingDirectory = workingDirectory;
            }

            pro.StartInfo.FileName = batPath;
            // 为了捕获输出，需要将UseShellExecute设置为false
            pro.StartInfo.UseShellExecute = false;
            // 重定向输出和错误流，这样才能捕获它们
            pro.StartInfo.RedirectStandardOutput = true;
            pro.StartInfo.RedirectStandardError = true;
            // 不创建窗口
            pro.StartInfo.CreateNoWindow = true;

            pro.Start();

            // 读取输出
            string output = pro.StandardOutput.ReadToEnd();
            string error = pro.StandardError.ReadToEnd();

            pro.WaitForExit();

            // 输出到Unity控制台
            if (!string.IsNullOrEmpty(output))
            {
                Debug.Log(output);
            }

            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError(error);
            }
        }

        public static bool ExecuteCmd(string filename, string arguments, out int exitCode, string workdir = null,
            DataReceivedEventHandler recv = null)
        {
            exitCode = 0;
            Debug.Log("ApkTools.ExecuteCmd( " + filename + " , " + arguments + " )");

            try
            {
                Process proc = new Process();
                proc.StartInfo.FileName = filename;
                proc.StartInfo.Arguments = arguments;
                if (string.IsNullOrEmpty(workdir))
                    workdir = System.Environment.CurrentDirectory;
                Debug.Log("Walk Directory:" + workdir);
                proc.StartInfo.WorkingDirectory = workdir;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                if (null == recv)
                    proc.OutputDataReceived += new DataReceivedEventHandler((sender, arg) => { Debug.Log(arg.Data); });
                else
                    proc.OutputDataReceived += recv;
                proc.ErrorDataReceived += new DataReceivedEventHandler((sender, arg) => { Debug.Log(arg.Data); });
                proc.Start();
                proc.BeginErrorReadLine();
                proc.BeginOutputReadLine();

                proc.WaitForExit();

                exitCode = proc.ExitCode;

                if (exitCode != 0)
                    Debug.LogError("Execute Command: " + filename + " " + arguments + " Exit Code=" + exitCode);

                return exitCode == 0;
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }

            return false;
        }
    }
}