using System;
using System.Collections.Generic;
using System.Diagnostics;
using EBA.Ebunieditor.Editor.Common;
using UnityEditor;
using UnityEngine;

namespace EBA.Ebunieditor.Editor.Subversion
{
    public class Subversion
    {
        /// <summary>
        ///     SVN更新指定的路径
        ///     路径示例：Assets/1.png
        /// </summary>
        /// <param name="assetPaths"></param>
        public static void UpdateAtPath(string assetPath)
        {
            var assetPaths = new List<string>();
            assetPaths.Add(assetPath);
            UpdateAtPaths(assetPaths);
        }

        public static void AddAtPaths(List<string> assetPaths)
        {
            if (assetPaths.Count == 0)
                return;
            var arg = "/command:add /closeonend:0 /path:\"";
            for (var i = 0; i < assetPaths.Count; i++)
            {
                var assetPath = assetPaths[i];
                if (i != 0) arg += "*";

                arg += assetPath;
            }

            arg += "\"";
            SvnCommandRun(arg);
        }

        /// <summary>
        ///     SVN更新指定的路径
        ///     路径示例：Assets/1.png
        /// </summary>
        /// <param name="assetPaths"></param>
        public static void UpdateAtPaths(List<string> assetPaths)
        {
            if (assetPaths.Count == 0) return;

            var arg = "/command:update /closeonend:0 /path:\"";
            for (var i = 0; i < assetPaths.Count; i++)
            {
                var assetPath = assetPaths[i];
                if (i != 0) arg += "*";

                arg += assetPath;
            }

            arg += "\"";
            SvnCommandRun(arg);
        }

        /// <summary>
        ///     SVN提交指定的路径
        ///     路径示例：Assets/1.png
        /// </summary>
        /// <param name="assetPaths"></param>
        public static void CommitAtPaths(List<string> assetPaths, string logmsg = null)
        {
            if (assetPaths.Count == 0) return;

            var arg = "/command:commit /closeonend:0 /path:\"";
            for (var i = 0; i < assetPaths.Count; i++)
            {
                var assetPath = assetPaths[i];
                if (i != 0) arg += "*";

                arg += assetPath;
            }

            arg += "\"";
            if (!string.IsNullOrEmpty(logmsg)) arg += " /logmsg:\"" + logmsg + "\"";

            SvnCommandRun(arg);
        }

        public static void RevertAtPaths(List<string> assetPaths, string logmsg = null)
        {
            if (assetPaths.Count == 0) return;

            var arg = "/command:revert /closeonend:0 /path:\"";
            for (var i = 0; i < assetPaths.Count; i++)
            {
                var assetPath = assetPaths[i];
                if (i != 0) arg += "*";

                arg += assetPath;
            }

            arg += "\"";
            if (!string.IsNullOrEmpty(logmsg)) arg += " /logmsg:\"" + logmsg + "\"";

            SvnCommandRun(arg);
        }

        [MenuItem("Assets/SVN Tool/SVN 更新")]
        private static void SvnToolUpdate()
        {
            var assetPaths = CommonUtility.GetSelectionAssetPaths();
            UpdateAtPaths(assetPaths);
        }

        [MenuItem("Assets/SVN Tool/SVN 提交...")]
        private static void SvnToolCommit()
        {
            var assetPaths = CommonUtility.GetSelectionAssetPaths();
            CommitAtPaths(assetPaths);
        }

        [MenuItem("Assets/SVN Tool/SVN 还原")]
        private static void SvnToolRevert()
        {
            var assetPaths = CommonUtility.GetSelectionAssetPaths();
            RevertAtPaths(assetPaths);
        }

        [MenuItem("Assets/SVN Tool/SVN 增加")]
        private static void SvnToolAdd()
        {
            var assetPaths = CommonUtility.GetSelectionAssetPaths();
            AddAtPaths(assetPaths);
        }

        [MenuItem("Assets/SVN Tool/显示日志")]
        private static void SvnToolLog()
        {
            var assetPaths = CommonUtility.GetSelectionAssetPaths();
            if (assetPaths.Count == 0) return;

            // 显示日志，只能对单一资产
            var arg = "/command:log /closeonend:0 /path:\"";
            arg += assetPaths[0];
            arg += "\"";
            SvnCommandRun(arg);
        }
        
        [MenuItem("Assets/SVN Tool/显示日志（Meta）")]
        private static void SvnToolMetaLog()
        {
            var assetPaths = CommonUtility.GetSelectionAssetPaths();
            if (assetPaths.Count == 0) return;

            // 显示日志，只能对单一资产
            var arg = "/command:log /closeonend:0 /path:\"";
            arg += $"{assetPaths[0]}.meta";
            arg += "\"";
            SvnCommandRun(arg);
        }

        [MenuItem("Assets/SVN Tool/检查修改")]
        private static void SvnCheckModify()
        {
            var assetPaths = CommonUtility.GetSelectionAssetPaths();
            if (assetPaths.Count == 0) return;
            // 显示日志，只能对单一资产
            var arg = "/command:diff /closeonend:0 /path:\"";
            arg += assetPaths[0];
            arg += "\"";
            SvnCommandRun(arg);
        }

        [MenuItem("Assets/SVN Tool/追溯(仅文件)")]
        private static void SvnBlame()
        {
            var assetPaths = CommonUtility.GetSelectionAssetPaths();
            if (assetPaths.Count == 0) return;
            // 显示日志，只能对单一资产
            var arg = "/command:blame /closeonend:0 /path:\"";
            arg += assetPaths[0];
            arg += "\"";
            SvnCommandRun(arg);
        }

        [MenuItem("Assets/SVN Tool/全部更新", false, 1100)]
        private static void SvnToolAllUpdate()
        {
            // 往上两级，包括数据配置文件
            var arg = "/command:update /closeonend:0 /path:\"";
            arg += "..";
            arg += "\"";
            SvnCommandRun(arg);
        }

        [MenuItem("Assets/SVN Tool/全部日志", false, 1101)]
        private static void SvnToolAllLog()
        {
            // 往上两级，包括数据配置文件
            var arg = "/command:log /closeonend:0 /path:\"";
            arg += "..";
            arg += "\"";
            SvnCommandRun(arg);
        }

        /// <summary>
        ///     SVN命令运行
        /// </summary>
        /// <param name="arg"></param>
        public static void SvnCommandRun(string arg)
        {
            var workDirectory =
                Application.dataPath.Remove(Application.dataPath.LastIndexOf("/Assets", StringComparison.Ordinal));
            Process.Start(new ProcessStartInfo
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                FileName = "TortoiseProc",
                Arguments = arg,
                WorkingDirectory = workDirectory
            });
        }
    }
}