using UnityEngine;

namespace EBA.Ebunieditor.Editor.Spine.Task
{
    [Task("Spine导出纹理任务")]
    public class ExportTexturePackTask : ITask
    {
        public void Run(TaskContext taskContext)
        {
            throw new System.NotImplementedException();
        }

        public void Finish()
        {
            Debug.Log("ExportTexturePackTask ====================================== Finish");
        }
    }
}