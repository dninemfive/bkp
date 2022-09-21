using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bkp
{
    public enum IoResult { Success, Failure, Existence, InProgress, Other }
    public static class IO
    {
        public static ConcurrentQueue<IoOperation> Queue { get; } = new();
        public static IoResult TryCopy(string oldFilePath, string newFilePath)
        {
            IoResult result;
            long size = new FileInfo(oldFilePath).Length;
            if (File.Exists(newFilePath))
            {
                result = IoResult.Existence;
                MainWindow.Instance.UpdateProgress(oldFilePath, result, size);
                return result;
            }
            Directory.CreateDirectory(Path.GetDirectoryName(newFilePath));
            try
            {
                File.Copy(oldFilePath, newFilePath);
                result = IoResult.Success;
                MainWindow.Instance.UpdateProgress(oldFilePath, result, size);
                return result;
            }
            catch (Exception e)
            {
                Utils.Log(e);
                result = IoResult.Failure;
                MainWindow.Instance.UpdateProgress(oldFilePath, result, size);
                return result;
            }
        }
        public static IoResult TryDelete(string path)
        {
            if(!File.Exists(path))
            {
                Utils.Log($"Tried to delete {path}, but it did not exist!");
                return IoResult.Existence;
            }
            try
            {
                // for some reason git objects are flagged as read-only :unamused:
                // https://stackoverflow.com/a/8081331
                File.SetAttributes(path, File.GetAttributes(path) & ~FileAttributes.ReadOnly);
                File.Delete(path);
                return IoResult.Success;
            }
            catch (Exception e)
            {
                Utils.Log(e);
                return IoResult.Failure;
            }
        }
        public static void Run()
        {
            while(Queue.Any() && Indexer.AnythingLeftToQueue)
            {
                if (Queue.TryDequeue(out IoOperation op)) Utils.PrintLine(op.OldPath, op.Execute());
            }
        }
    }
    public abstract class IoOperation
    {
        public virtual string OldPath { get; private set; }
        public virtual string NewPath { get; private set; }
        public IoOperation(string oldPath, string newPath)
        {
            OldPath = oldPath;
            NewPath = newPath;
        }
        public abstract IoResult Execute();
    }
    public class MoveOperation : IoOperation
    {
        public MoveOperation(string oldPath, string newPath) : base(oldPath, newPath) { }
        public override IoResult Execute()
        {
            IoResult result = IO.TryCopy(OldPath, NewPath);
            if (result is not IoResult.Success) return result;
            return IO.TryDelete(OldPath);
        }
    }
    public class CopyOperation : IoOperation
    {
        public CopyOperation(string oldPath, string newPath) : base(oldPath, newPath) { }
        public override IoResult Execute() => IO.TryCopy(OldPath, NewPath);
    }
    public class DeleteOperation : IoOperation
    {
        public DeleteOperation(string oldPath, string newPath) : base(oldPath, newPath) { }
        public override IoResult Execute() => IO.TryDelete(OldPath);
    }
}
