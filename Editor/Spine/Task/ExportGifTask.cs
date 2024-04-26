using System.IO;
using System.Text;
using EBA.Ebunieditor.Editor.Spine.Bean;
using UnityEngine;

namespace EBA.Ebunieditor.Editor.Spine.Task
{
    [Task("Spine导出Gif的任务相关")]
    public class ExportGifTask : ITask
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
                var spineProject = Path.Combine(destEntryDir, $"{name}.{taskContext.NewSpineVersion}.spine");
                if (File.Exists(spineProject))
                {
                    var exportGifPath = Path.Combine(destEntryDir, "gif");
                    var exportGifSettingBean = new ExportGifSettingBean
                    {
                        input = TaskHelper.NormalizePath(spineProject),
                        output = TaskHelper.NormalizePath(Path.Combine(exportGifPath, $"{name}.gif")),
                        skeleton = name
                    };
                    var exportJsonPath = Path.Combine(destEntryDir, "export_gif.json");
                    File.WriteAllText(exportJsonPath, TaskHelper.FormatJsonData(exportGifSettingBean));
                    sb.AppendLine($"--update {taskContext.NewSpineVersion} -i {spineProject} -m -o {exportGifSettingBean.output} -e {exportJsonPath} ^");
                }
            }
            
            Debug.Log(sb.ToString());
            
            File.WriteAllText(TaskContext.s_sExportGifTaskPath, sb.ToString());
            
            TaskHelper.RunBat(TaskContext.s_sExportGifTaskPath);
        }

        public void Finish()
        {
            Debug.Log("ExportGifTask ====================================== Finish");
        }
    }
}