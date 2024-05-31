using System.IO;
using System.Text;
using EBA.LitJson;
using UnityEngine;

namespace EBA.Ebunieditor.Editor.Spine.Task
{
    [Task("Spine修改工程里面的图片路径任务")]
    public class ModifyPicResTask : ITask
    {
        public void Run(TaskContext taskContext)
        {
            var atlasFiles = Directory.GetFiles(taskContext.OutputFolderPath, taskContext.SpineAtlasSearchPattern, SearchOption.AllDirectories);
            foreach (var atlas in atlasFiles)
            {
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(atlas));
                var destEntryDir = Path.Combine(taskContext.OutputFolderPath, name);
                var destData = Path.Combine(destEntryDir, $"{name}{taskContext.SpineDataExtension}");
                if (File.Exists(destData))
                {
                    var jsonObj = JsonMapper.ToObject(File.ReadAllText(destData));
                    jsonObj["skeleton"]["images"] = "./src/";
                    jsonObj["skeleton"]["audio"] = "";
                    File.WriteAllText(destData, jsonObj.ToJson());
                }
            }
        }

        public void Finish()
        {
            Debug.Log("ModifyPicResTask ====================================== Finish");
        }
    }
}