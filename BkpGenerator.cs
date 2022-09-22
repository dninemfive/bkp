using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Media;
using System.Text.Json;

namespace bkp
{
    public static class BkpGenerator
    {
        private static StreamWriter Bkp;
        public static Task Backup()
        {
            string dest = MainWindow.Config.DestinationFolder;
            string bkpFile = Path.Join(dest, $"{Console.DateToday}.bkp");
            Bkp = File.AppendText(bkpFile);
            string indexFolder = Path.Join(dest, "_index");
            try
            {
                foreach(string folder in MainWindow.Config.SourceFolders)
                {
                    foreach(string file in folder.AllFilesRecursive())
                    {
                        IndexAndCopy(file, indexFolder);
                    }
                }
            } 
            finally
            {
                Console.Log("Flushing...");
                Bkp.Flush();
                Bkp.Close();
            }
            return Task.CompletedTask;
        }
        public static string Index(string path)
        {
            FileRecord fr = new(path);
            string line = JsonSerializer.Serialize(fr);
            Bkp.WriteLine(line);
            return fr.Hash;
        }
        public static void IndexAndCopy(string filePath, string indexFolder)
        {
            try
            {
                string hash = Index(filePath);
                MainWindow.Instance.UpdateProgress(IO.TryCopy(filePath, Path.Join(indexFolder, hash)));
            }
            catch
            {
                MainWindow.Instance.UpdateProgress(filePath, ResultCategory.Failure, -1);
            }
        }
        public static void CleanUp(string filePath)
        {
            IEnumerable<string> lines = File.ReadAllLines(filePath);
            HashSet<string> records = lines.ToHashSet();
            Console.PrintLineAndLog($"{lines.Count()} unique lines and {records.Count} unique lines to clean up.");
            Queue<string> toWrite = new(records.OrderBy(x => x));
            File.WriteAllText(filePath, "");
            while (toWrite.TryDequeue(out string s))
            {
                File.AppendAllText(filePath, $"{s}\n");
                MainWindow.Instance.UpdateProgress(s, ResultCategory.Success, 1, records.Count);
            }
        }
    }
    public class FileRecord
    {
        public string Path { get; private set; }
        public string Hash { get; private set; }
        public FileRecord(string path)
        {
            Path = path;
            Hash = Path.FileHash();
        }
    }
}
