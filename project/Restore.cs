using d9.utl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace bkp
{
    public class Restore
    {
        public static async Task RestoreStuff(Progress<(string path, bool applies)> progress)
        {
            string basePath = "C:/Users/dninemfive/Music/Files", backupPath = @"D:\Automatic\2024.5.14.bkp", indexPath = @"D:\Automatic\_index";
            await foreach(FileRecord fr in GetFileRecords(backupPath))
            {
                string filePath = fr.Path;
                // ((IProgress<(string, bool)>)progress).Report((filePath, filePath.IsInFolder(basePath) && !File.Exists(filePath)));
                MainWindow.Instance.UpdateProgress(filePath,
                    (filePath.IsInFolder(basePath) && !File.Exists(filePath)) ? ResultCategory.Success : ResultCategory.NoChange, 1);
            }
        }
        public static async IAsyncEnumerable<FileRecord> GetFileRecords(string path)
        {
            foreach(string s in await File.ReadAllLinesAsync(path))
            {
                yield return await Task.Run(() => JsonSerializer.Deserialize<FileRecord>(s));
            }
        }
    }
}
