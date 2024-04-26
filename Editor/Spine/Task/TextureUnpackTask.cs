using System.IO;
using System.Text;
using UnityEngine;

namespace EBA.Ebunieditor.Editor.Spine.Task
{
    [Task("Spine纹理解包的任务")]
    public class TextureUnpackTask : ITask
    {
        public void Run(TaskContext taskContext)
        {
            var sb = new StringBuilder();
            sb.Append("spine ");
            var atlasList = taskContext.lstSpineAtlass;
            foreach (var atlas in atlasList)
            {
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(atlas));
                var destPngName = $"{name}.png";
                var dir = Path.GetDirectoryName(atlas);
                var f = Path.Combine(taskContext.TextureFolderPath, destPngName);
                //这里处理有的纹理是放在全部的Texture里面，有的和spine的数据文件夹一个目录
                if (!File.Exists(f))
                {
                    f = Path.Combine(dir, destPngName);
                    if (!File.Exists(f))
                        f = Path.Combine(dir, $"{name}.jpg");
                }
                
                var destEntryDir = Path.Combine(taskContext.OutputFolderPath, name);
                var destSrcPicDir = Path.Combine(destEntryDir, "src");
                if (File.Exists(f))
                {
                    //https://zh.esotericsoftware.com/spine-command-line-interface
                    File.Copy(f, Path.Combine(destEntryDir, destPngName), true);
                    sb.AppendLine(
                        $"--update {taskContext.NewSpineVersion} -i {destEntryDir} -o {destSrcPicDir} -c {atlas} ^");
                }
            }

            Debug.Log(sb.ToString());
            
            File.WriteAllText(TaskContext.s_sUnpackTaskPath, sb.ToString());
            
            TaskHelper.RunBat(TaskContext.s_sUnpackTaskPath);
            // var code = 0;
            // var isSuc = TaskHelper.ExecuteCmd(TaskContext.SPINE_EXE, sb.ToString(), out code);
        }

        public void Finish()
        {
            Debug.Log("TextureUnpackTask ====================================== Finish");
        }
    }
}