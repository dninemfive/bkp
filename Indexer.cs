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
        public static bool AnythingLeftToQueue { get; private set; } = false;
        public static IEnumerable<string> BackupSources => File.ReadAllLines(BACKUP_SOURCE_FILE);
        private static StreamWriter Bkp;
        public static Task RetroactivelyIndex(string path)
        {
            string parentFolder = Directory.GetParent(path).FullName;
            string bkpFile = Path.Join(parentFolder, $"{path.FolderName()}.bkp");
            Bkp = File.AppendText(bkpFile);
            try
            {
                string indexFolder = Path.Join(parentFolder, "_index");
                Utils.Log($"parentFolder = {parentFolder}\nbkpFile = {bkpFile}\nindexFolder = {indexFolder}");
                foreach (string filePath in path.AllFilesRecursive())
                {
                    Task.Run(() => IndexAndMove(filePath, indexFolder));
                }
            } finally
            {
                Utils.Log("Flushing...");
                Bkp.Flush();
                Bkp.Close();
            }            
            return Task.CompletedTask;
        }
        public static Task Backup()
        {
            AnythingLeftToQueue = true;
            string dest = MainWindow.Config.DestinationFolder;
            string bkpFile = Path.Join(dest, Utils.DateToday, ".bkp");
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
                Utils.Log("Flushing...");
                Bkp.Flush();
                Bkp.Close();
            }
            AnythingLeftToQueue = false;
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
            return fr.Hash;
        }
        public static void IndexAndCopy(string filePath, string indexFolder)
        {
            string hash = Index(filePath);
            IO.TryCopy(filePath, Path.Join(indexFolder, hash));
        }
        public static void IndexAndMove(string filePath, string indexFolder)
        {
            string hash = Index(filePath);
            IO.Queue.Enqueue(new MoveOperation(filePath, Path.Join(indexFolder, hash)));
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
