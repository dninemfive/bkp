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
        public static IoResult TryMove(string oldFilePath, string newFilePath)
        {
            IoResult result = TryCopy(oldFilePath, newFilePath);
            if (result is not IoResult.Success) return result;
            return TryDelete(oldFilePath);
        }
    }
}
