using System.IO;
using System.Text;
using EBA.Ebunieditor.Editor.Spine.Bean;
using UnityEngine;

namespace EBA.Ebunieditor.Editor.Spine.Task
{
    public class ExportAtlasTask : ITask
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
                    var exportPath = Path.Combine(destEntryDir, "export");
                    var exportSettingBean = new ExportSettingBean
                    {
                        input = TaskHelper.NormalizePath(spineProject),
                        output = TaskHelper.NormalizePath(exportPath),
                        packAtlas = new PackAtlasBean()
                        {
                            
                        }
                    };
                    var exportJsonPath = Path.Combine(destEntryDir, "export_atlas.json");
                    File.WriteAllText(exportJsonPath, TaskHelper.FormatJsonData(exportSettingBean));
                    sb.AppendLine($"--update {taskContext.NewSpineVersion} -i {spineProject} -m -o {exportSettingBean.output} -e {exportJsonPath} ^");
                }
            }
            
            Debug.Log(sb.ToString());
            
            File.WriteAllText(TaskContext.s_sExportAtlasTaskPath, sb.ToString());
            
            TaskHelper.RunBat(TaskContext.s_sExportAtlasTaskPath);
        }

        public void Finish()
        {
            
        }
    }
}