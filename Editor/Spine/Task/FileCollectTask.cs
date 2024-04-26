using System.IO;
using UnityEngine;

namespace EBA.Ebunieditor.Editor.Spine.Task
{
    [Task("文件搜集Task")]
    public class FileCollectTask : ITask
    {
        public void Run(TaskContext taskContext)
        {
            TaskHelper.SpineMkdir(taskContext.OutputFolderPath, true);
            var spineFiles = Directory.GetFiles(taskContext.SearchFolderPath, taskContext.SpineDataSearchPattern, SearchOption.AllDirectories);
            taskContext.lstSpineDatas.AddRange(spineFiles);
            var spineAtlasFiles = Directory.GetFiles(taskContext.SearchFolderPath, taskContext.SpineAtlasSearchPattern, SearchOption.AllDirectories);
            taskContext.lstSpineAtlass.AddRange(spineAtlasFiles);
            // 创建以文件名命名的文件夹，并移动文件
            TaskHelper.MoveFilesToGroupFolders(taskContext.lstSpineDatas, taskContext.OutputFolderPath);
            TaskHelper.MoveFilesToGroupFolders(taskContext.lstSpineAtlass, taskContext.OutputFolderPath);
        }

        public void Finish()
        {
            Debug.Log("FileCollectTask ====================================== Finish");
        }
    }
}