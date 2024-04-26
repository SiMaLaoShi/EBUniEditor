using System.IO;
using System.Text;
using UnityEngine;

namespace EBA.Ebunieditor.Editor.Spine.Task
{
    [Task("Spine导出数据任务")]
    public class ImportDataTask : ITask
    {
        public void Run(TaskContext taskContext)
        {
            var atlasFiles = Directory.GetFiles(taskContext.OutputFolderPath, taskContext.SpineAtlasSearchPattern, SearchOption.AllDirectories);
            var sb = new StringBuilder();
            sb.Append("spine ");
            foreach (var atlas in atlasFiles)
            {
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(atlas));
                var destEntryDir = Path.Combine(taskContext.OutputFolderPath, name);
                var data = Path.Combine(destEntryDir, $"{name}{taskContext.SpineDataExtension}");
                var spineProject = Path.Combine(destEntryDir, $"{name}.spine");
                if (File.Exists(data))
                    sb.AppendLine($"--update {taskContext.OldSpineVersion} -i {data} -o {spineProject} -s 1 -r ^");
            }
            
            Debug.Log(sb.ToString());
            
            File.WriteAllText(TaskContext.s_sImportDataTaskPath, sb.ToString());
            
            TaskHelper.RunBat(TaskContext.s_sImportDataTaskPath);
        }

        public void Finish()
        {
            Debug.Log("ImportDataTask ====================================== Finish");
        }
    }
}