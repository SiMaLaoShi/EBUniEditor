using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EBA.Ebunieditor.Editor.AssetPostprocessor
{
    public class Overwrite : UnityEditor.AssetPostprocessor
    {
        class FilePath
        {
            public string Path;
            public string FileName;

            public FilePath(string path)
            {
                Path = path;
                FileName = System.IO.Path.GetFileName(Path);
            }
        }

        class ExistAsset
        {
            public FilePath Source;
            public FilePath Imported;

            public ExistAsset(FilePath source, FilePath imported)
            {
                Source = source;
                Imported = imported;
            }
        }

        const string SourceExistFormat = "因为“{0}”和“{1}”的文件内容完全一致，所以停止替换工具的操作。\n真的要导入吗?";

        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromPath)
        {
            int count = importedAssets.Length;
            if (count == 0 || Event.current == null || Event.current.type != EventType.DragPerform)
            {
                return;
            }

            List<string> dragAndDropPaths = new List<string>(DragAndDrop.paths);
            for (int i = 0; i < dragAndDropPaths.Count;)
            {
                if (dragAndDropPaths[i].EndsWith(".meta"))
                {
                    dragAndDropPaths.RemoveAt(i);
                    continue;
                }

                ++i;
            }

            if (count != dragAndDropPaths.Count)
            {
                return;
            }

            List<FilePath> sourcePaths = new List<FilePath>(count);
            for (int i = 0; i < count; ++i)
            {
                if (dragAndDropPaths[i].EndsWith(".prefab"))
                {
                    continue;
                }

                sourcePaths.Add(new FilePath(dragAndDropPaths[i]));
            }

            List<FilePath> importedPaths = new List<FilePath>(count);
            for (int i = 0; i < count; ++i)
            {
                if (importedAssets[i].EndsWith(".prefab"))
                {
                    continue;
                }

                importedPaths.Add(new FilePath(importedAssets[i]));
            }

            int matchCnt = 0;
            for (; matchCnt < count; ++matchCnt)
            {
                string source = sourcePaths[matchCnt].FileName;
                int j = 0;
                for (; j < count; ++j)
                {
                    if (source.Contains(importedPaths[j].FileName))
                    {
                        break;
                    }
                }

                if (j == count)
                {
                    break;
                }
            }

            if (matchCnt == count)
            {
                return;
            }

            bool isExecutable = true;
            bool isDeleteImportedAssets = false;
            for (int i = 0; i < count; ++i)
            {
                for (int j = i + 1; j < count; ++j)
                {
                    FilePath path1 = sourcePaths[i];
                    FilePath path2 = sourcePaths[j];

                    if (FileCompare(path1.Path, path2.Path))
                    {
                        string message = string.Format(SourceExistFormat, path1.FileName, path2.FileName);
                        isDeleteImportedAssets = !EditorUtility.DisplayDialog(
                            "确定",
                            message,
                            "导入",
                            "中止");
                        isExecutable = false;
                        break;
                    }
                }

                if (!isExecutable)
                {
                    break;
                }
            }

            if (!isExecutable)
            {
                if (isDeleteImportedAssets)
                {
                    for (int i = 0; i < importedAssets.Length; ++i)
                    {
                        AssetDatabase.DeleteAsset(importedAssets[i]);
                    }
                }

                return;
            }

            for (int i = 0; i < sourcePaths.Count;)
            {
                bool isRemoved = false;
                FilePath source = sourcePaths[i];
                for (int j = 0; j < importedPaths.Count; ++j)
                {
                    FilePath imported = importedPaths[j];
                    if (source.FileName != imported.FileName)
                    {
                        continue;
                    }

                    if (!FileCompare(source.Path, imported.Path))
                    {
                        for (int k = 0; k < importedPaths.Count; ++k)
                        {
                            if (j == k)
                            {
                                continue;
                            }

                            if (FileCompare(source.Path, importedPaths[k].Path))
                            {
                                string tempPath = imported.Path + "_temp";
                                FileUtil.CopyFileOrDirectory(imported.Path, tempPath);
                                FileUtil.ReplaceFile(importedPaths[k].Path, imported.Path);
                                FileUtil.ReplaceFile(tempPath, importedPaths[k].Path);
                                FileUtil.DeleteFileOrDirectory(tempPath);
                                AssetDatabase.ImportAsset(imported.Path);
                                AssetDatabase.ImportAsset(importedPaths[k].Path);
                                break;
                            }
                        }
                    }

                    sourcePaths.RemoveAt(i);
                    importedPaths.RemoveAt(j);
                    isRemoved = true;
                    break;
                }

                if (!isRemoved)
                {
                    ++i;
                }
            }

            List<ExistAsset> existAssets = new List<ExistAsset>(sourcePaths.Count);
            for (int i = 0; i < sourcePaths.Count; i++)
            {
                FilePath source = sourcePaths[i];
                for (int j = 0; j < importedPaths.Count; ++j)
                {
                    FilePath imported = importedPaths[j];
                    if (!FileCompare(source.Path, imported.Path))
                    {
                        continue;
                    }

                    existAssets.Add(new ExistAsset(source, imported));
                    importedPaths.RemoveAt(j);
                    break;
                }
            }

            existAssets.Sort((a, b) => a.Source.Path.CompareTo(b.Source.Path));

            var isFirst = true;
            var isSameAction = false;
            var result = 0;

            foreach (var exist in existAssets)
            {
                string importedPath = exist.Imported.Path;
                string importedAssetDirectory = Path.GetDirectoryName(importedPath);
                string existingAssetPath = string.Format("{0}/{1}", importedAssetDirectory, exist.Source.FileName);

                if (!isSameAction)
                {
                    result = EditorUtility.DisplayDialogComplex(
                        existingAssetPath.Replace('\\', '/'),
                        "同名的资产已经存在了。要置换资产吗?",
                        "置换",
                        "中止",
                        "两个都保留");
                }

                if (result == 0)
                {
                    FileUtil.ReplaceFile(importedPath, existingAssetPath);
                    AssetDatabase.DeleteAsset(importedPath);
                    AssetDatabase.ImportAsset(existingAssetPath);
                }
                else if (result == 1)
                {
                    AssetDatabase.DeleteAsset(importedPath);
                }

                if (isFirst)
                {
                    if (existAssets.Count > 2)
                    {
                        isSameAction = EditorUtility.DisplayDialog(
                            "确定",
                            "同样的操作以后都适用吗?",
                            "是",
                            "否");
                    }

                    isFirst = false;
                }
            }
        }

        static bool FileCompare(string file1, string file2)
        {
            if (file1 == file2)
            {
                return true;
            }

            FileStream fs1 = new FileStream(file1, FileMode.Open);
            FileStream fs2 = new FileStream(file2, FileMode.Open);
            int byte1;
            int byte2;
            bool ret = false;

            try
            {
                if (fs1.Length == fs2.Length)
                {
                    do
                    {
                        byte1 = fs1.ReadByte();
                        byte2 = fs2.ReadByte();
                    } while ((byte1 == byte2) && (byte1 != -1));

                    if (byte1 == byte2)
                    {
                        ret = true;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                return false;
            }
            finally
            {
                fs1.Close();
                fs2.Close();
            }

            return ret;
        }
    }
}