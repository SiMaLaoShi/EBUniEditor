using System.Collections.Generic;
using EBA.Ebunieditor.Editor.Spine.Task;
using UnityEditor;

namespace EBA.Ebunieditor.Editor.Spine
{
    public class SpineTaskScheduler
    {
        [MenuItem("Tools/RunSpineTask")]
        public static void RunSpineTask()
        {
            var taskContext = new TaskContext();
            TaskHelper.SpineMkdir(TaskContext.s_sBatRootPath);
            var taskList = new List<ITask>()
            {
                new FileCollectTask(),
                new TextureUnpackTask(),
                new ModifyPicResTask(),
                new ImportDataTask(),
                new UpdateSpineProjectTask(),
                new ExportGifTask(),
                new ExportAtlasTask()
            };
            var cnt = 1;
            foreach (var task in taskList)
            {
                task.Run(taskContext);
                task.Finish();
                EditorUtility.DisplayProgressBar("", $"update[{cnt} - {taskList.Count}]",(float) cnt ++ / taskList.Count);
            }
            EditorUtility.ClearProgressBar();
        }
    }
}