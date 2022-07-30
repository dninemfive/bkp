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
        private static long? _size = null;
        public static IEnumerable<string> BackupSources => File.ReadAllLines(BACKUP_SOURCE_FILE);
        private static StreamWriter Bkp, Manifest;
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
        public static void Index(string path)
        {
            string line = JsonSerializer.Serialize(new FileRecord(path));
            Bkp.WriteLine(line);
            Utils.PrintLine(line, new SolidColorBrush(Colors.Orange));
            MainWindow.ForceUpdate();
        }
    }
    public class FileRecord
    {
        public string Path { get; private set; }
        public string Hash => Path.FileHash();
        public FileRecord(string path)
        {
            Path = path;
        }
    }
}
