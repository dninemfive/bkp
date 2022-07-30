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
    public static class Indexer
    {
        const string BACKUP_SOURCE_FILE = "bkp.sources", DESTINATION = "bkp.destination";
        public static long Size { get; private set; }
        public static IEnumerable<string> BackupSources => File.ReadAllLines(BACKUP_SOURCE_FILE);
        private static StreamWriter Bkp, Manifest;
        public static Task RetroactivelyIndex(string path)
        {
            Size = Utils.CalculateSizeOf(path);
            string parentFolder = Directory.GetParent(path).FullName;
            string bkpFile = Path.Join(parentFolder, $"{path.FolderName()}.bkp");            
            //File.WriteAllText(bkpFile, "");
            Bkp = File.AppendText(bkpFile);
            try
            {
                string indexFolder = Path.Join(parentFolder, "_index");
                Utils.Log($"parentFolder = {parentFolder}\nbkpFile = {bkpFile}\nindexFolder = {indexFolder}");
                foreach (string filePath in path.AllFilesRecursive())
                {
                    //Utils.Log($"\t{filePath}");
                    string hash = Index(filePath);
                    if(Utils.Copy(filePath, Path.Join(indexFolder, hash))) Utils.TryDelete(filePath);
                }
            } finally
            {
                Bkp.Flush();
                Bkp.Close();
            }            
            return Task.CompletedTask;
        }
        public static Task IndexAll()
        {
            File.WriteAllText("example.bkp", "");
            Bkp = File.AppendText("example.bkp");
            foreach(string sourceFolder in BackupSources)
            {
                foreach (string filePath in sourceFolder.AllFilesRecursive()) Index(filePath);
            }
            return Task.CompletedTask;
        }
        public static string Index(string path)
        {
            FileRecord fr = new(path);
            string line = JsonSerializer.Serialize(fr);
            Bkp.WriteLine(line);
            Utils.PrintLine(line, new SolidColorBrush(Colors.Orange));
            MainWindow.ForceUpdate();
            return fr.Hash;
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
