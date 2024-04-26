using System.IO;
using System.Text;
using UnityEngine;

namespace EBA.Ebunieditor.Editor.Spine.Task
{
    [Task("升级Spine工程的Task")]
    public class UpdateSpineProjectTask : ITask
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
                var spineProject = Path.Combine(destEntryDir, $"{name}.spine");
                if (File.Exists(spineProject))
                {
                    var newSpineProjectPath = spineProject.Replace(".spine", $".{taskContext.NewSpineVersion}.spine");
                    sb.AppendLine($"--update {taskContext.NewSpineVersion} --input {spineProject} --output {newSpineProjectPath} -r ^");
                }
            }
            
            Debug.Log(sb.ToString());
            
            File.WriteAllText(TaskContext.s_sUpdateProjectTaskPath, sb.ToString());
            
            TaskHelper.RunBat(TaskContext.s_sUpdateProjectTaskPath);
        }

        public void Finish()
        {
            Debug.Log("UpdateSpineProjectTask ====================================== Finish");
        }
    }
}