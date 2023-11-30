using System;
using System.IO;

namespace bkp.Utils
{
    public enum ResultCategory { Success, Failure, NoChange, InProgress, Other }
    public struct IoResult
    {
        public string oldFilePath;
        public ResultCategory category;
        public long size;
        public IoResult(string ofp, ResultCategory c, long s)
        {
            oldFilePath = ofp;
            category = c;
            size = s;
        }
    }
    public static class IO
    {
        public static IoResult TryCopy(string oldFilePath, string newFilePath)
        {
            long size = new FileInfo(oldFilePath).Length;
            if (File.Exists(newFilePath))
            {
                return new(oldFilePath, ResultCategory.NoChange, size);
            }

            _ = Directory.CreateDirectory(Path.GetDirectoryName(newFilePath));
            try
            {
                File.Copy(oldFilePath, newFilePath);
                return new(oldFilePath, ResultCategory.Success, size);
            }
            catch (Exception e)
            {
                Console.Log(e);
                return new(oldFilePath, ResultCategory.Failure, size);
            }
        }
        public static IoResult TryDelete(string path)
        {
            if (!File.Exists(path))
            {
                Console.Log($"Tried to delete {path}, but it did not exist!");
                return new(path, ResultCategory.NoChange, -1);
            }
            try
            {
                // for some reason git objects are flagged as read-only :unamused:
                // https://stackoverflow.com/a/8081331
                File.SetAttributes(path, File.GetAttributes(path) & ~FileAttributes.ReadOnly);
                File.Delete(path);
                return new(path, ResultCategory.Success, -1);
            }
            catch (Exception e)
            {
                Console.Log(e);
                return new(path, ResultCategory.Failure, -1);
            }
        }
        public static IoResult TryMove(string oldFilePath, string newFilePath)
        {
            IoResult result = TryCopy(oldFilePath, newFilePath);
            return result.category is not ResultCategory.Success ? result : TryDelete(oldFilePath);
        }
    }
}
