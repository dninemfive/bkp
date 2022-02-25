using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bkp
{
    public static class Utils
    {
        public const string LOG_PATH = "log.txt";
        const string DATE_FORMAT = "yy.M.d";
        // cached to avoid issues when running near midnight
        private static DateTime? _today = null;
        public static string DateToday
        {
            get
            {
                if (_today is null) _today = DateTime.Now;
                return _today.Value.ToString(DATE_FORMAT);
            }
        }
        public static string BackupLocation(this string path) => path.Replace("C:/", $"D:/Automatic/{DateToday}/");
        public static bool Exists(this string path) => Directory.Exists(path);
        public static void Log(Exception e) => File.AppendAllText(LOG_PATH, e.ToString());
        public static void Print(object obj) => MainWindow.Instance.Print(obj.ToString());
        public static void PrintLine(object obj) => Print($"{obj}\n");
        public static IEnumerable<string> AllFilesRecursive(this string path)
        {
            // https://stackoverflow.com/questions/3835633/wrap-an-ienumerable-and-catch-exceptions/34745417
            using var enumerator = Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories).GetEnumerator();
            bool next = true;
            while (next)
            {
                try
                {
                    next = enumerator.MoveNext();
                }
                catch (Exception e)
                {
                    Log(e);
                }
                if (next)
                {
                    foreach (string file in Directory.EnumerateFiles(enumerator.Current)) yield return file;
                }
            }
        }
        
    }
}
