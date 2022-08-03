using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;

namespace bkp
{
    public static class Utils
    {
        public const string LOG_PATH = "bkp.log";
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
        public static SolidColorBrush Color(this IoResult lt) => lt switch
        {
            // colors tested with https://www.color-blindness.com/coblis-color-blindness-simulator/ and seem fine for all except maybe protanopia
            IoResult.Success    => new(Colors.LimeGreen),
            IoResult.Failure    => new(Colors.Red),
            IoResult.Existence  => new(Colors.Cyan),
            IoResult.InProgress => new(Colors.Yellow),
            _                   => new(Colors.White)
        };
        public static bool Exists(this string path) => Directory.Exists(path);
        public static void Log(object obj) => File.AppendAllText(LOG_PATH, $"{obj}\n");
        public static void Print(object obj) => MainWindow.Instance.Print(new Run(obj.ToString()));
        public static void Print(object obj, SolidColorBrush color) => MainWindow.Instance.Print(new Run(obj.ToString()) { Foreground = color });
        public static void PrintLine(object obj) => Print($"{obj}\n");
        public static void PrintLine(object obj, SolidColorBrush color) => Print($"{obj}\n", color);
        public static void PrintLine(Run r, bool replaceLast)
        {
            r.Text += "\n";
            if(replaceLast && MainWindow.Instance.Output.Inlines.Any())
            {
                MainWindow.Instance.Output.Inlines.Remove(MainWindow.Instance.Output.Inlines.LastInline);
            } 
            MainWindow.Instance.Print(r);
        }
        public static void PrintLine(object obj, IoResult type = IoResult.Other) => Print(obj, type.Color());
        public static Run RunFor(object obj, IoResult type) => new Run(obj.ToString()) { Foreground = type.Color() };
        public static IEnumerable<string> AllFilesRecursive(this string path)
        {
            if (!Directory.Exists(path)) yield break;
            foreach (string subfolder in (Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories)).EnumerateSafe())
            {
                foreach (string file in Directory.EnumerateFiles(subfolder).EnumerateSafe())
                {
                    yield return file;
                }
            }
            foreach (string file in Directory.EnumerateFiles(path).EnumerateSafe())
            {
                yield return file;
            }
        }
        public static IEnumerable<T> EnumerateSafe<T>(this IEnumerable<T> enumerable)
        {
            // https://stackoverflow.com/questions/3835633/wrap-an-ienumerable-and-catch-exceptions/34745417
            using var enumerator = enumerable.GetEnumerator();
            bool next = true;
            while(next)
            {
                try
                {                    
                    next = enumerator.MoveNext();
                }
                catch(Exception e)
                {
                    Log(e);
                }
                if (next)
                {
                    yield return enumerator.Current;
                }
            }
        }
        public static string Readable(this long bytes)
        {
            int digits = bytes.Digits();
            return $"{(bytes / (double)digits.Divisor()):F3} {digits.Suffix()}";
        }
        public static int Digits(this long l)
        {
            if (l < 0) l = -l;
            int ct = 0;
            while (l > 9)
            {
                l /= 10;
                ct++;
            }
            return ct;
        }
        public static int Divisor(this int digits)
        {
            int result = 1;
            for(int i = 0; i < digits / 3; i++)
            {
                result *= 1000;
            }
            return result;
        }
        public static string Suffix(this int digits) => digits switch
        {
            < 3  => "bytes",
            < 6  => "KB",
            < 9  => "MB",
            < 12 => "GB",
            < 15 => "TB",
            < 18 => "PB",
            < 21 => "EB",
            < 24 => "ZB",
            < 27 => "YB",
            _ => "unknown"
        };
        public static string FileHash(this string path)
        {
            // https://stackoverflow.com/a/51966515
            using SHA512 sha512 = SHA512.Create();
            using FileStream fs = File.OpenRead(path);
            return BitConverter.ToString(sha512.ComputeHash(fs)).Replace("-", "");
        }
        public static string FileName(this string path) => Path.GetFileName(path);
        // https://stackoverflow.com/a/27019172
        public static string FolderName(this string path) => new DirectoryInfo(path).Name;
        
        public static long CalculateSizeOf(string path)
        {
            long result = 0;
            foreach (string filePath in path.AllFilesRecursive())
            {
                try
                {
                    long l = new FileInfo(filePath).Length;
                    result += l;
                }
                catch (Exception e)
                {
                    Log(e);
                }
            }
            return result;
        }
        public static void DeleteEmptySubfolders(this string path)
        {
            foreach (string subfolder in Directory.EnumerateDirectories(path)) subfolder.DeleteEmptySubfolders();
            int numFiles = Directory.GetFiles(path).Length, numFolders = Directory.GetDirectories(path).Length;       
            if (numFiles + numFolders == 0)
            {
                try
                {
                    Directory.Delete(path);
                } 
                catch (Exception e)
                {
                    Log(e);
                }
            }                      
        }
        public static void PrintLineAndLog(object obj)
        {
            PrintLine(obj);
            Log(obj);
        }        
    }     
}
