using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Media;

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
            Utils.Log("IndexAll()");
            Bkp = File.AppendText("example.bkp");
            Utils.Log("2");
            foreach(string sourceFolder in BackupSources)
            {
                Utils.Log($"\t{sourceFolder}");
                foreach (string filePath in sourceFolder.AllFilesRecursive()) Index(filePath);
            }
            return Task.CompletedTask;
        }
        public static void Index(string path)
        {
            Utils.Log($"\t\tIndex{path}");
            string line = $"{path}: {path.FileHash()}";
            Bkp.WriteLine(line);
            Utils.PrintLine(line, new SolidColorBrush(Colors.Orange));
            MainWindow.ForceUpdate();
        }
    }
}
