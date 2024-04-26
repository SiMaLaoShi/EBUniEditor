using System;

namespace EBA.Ebunieditor.Editor.Spine
{
    public class TaskAttribute : Attribute
    {
        /// <summary>
        /// 任务说明
        /// </summary>
        public readonly string TaskDesc;

        public TaskAttribute(string taskDesc)
        {
            TaskDesc = taskDesc;
        }
    }
}