namespace EBA.Ebunieditor.Editor.Spine
{
    public interface ITask
    {
        void Run(TaskContext taskContext);

        void Finish();
    }
}