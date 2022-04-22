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
        public static SolidColorBrush Color(this LineType lt) => lt switch
        {
            // colors tested with https://www.color-blindness.com/coblis-color-blindness-simulator/ and seem fine for all except maybe protanopia
            LineType.Success    => new(Colors.LimeGreen),
            LineType.Failure    => new(Colors.Red),
            LineType.Existence  => new(Colors.Cyan),
            LineType.InProgress => new(Colors.Yellow),
            _                   => new(Colors.White)
        };
        public static string BackupLocation(this string path) => path.Replace("C:/", $"{Backup.TargetFolder}{DateToday}/");
        public static bool Exists(this string path) => Directory.Exists(path);
        public static void Log(string s) => File.AppendAllText(LOG_PATH, $"{s}\n");
        public static void Log(Exception e) => File.AppendAllText(LOG_PATH, $"{e}\n");
        public static void Print(object obj) => MainWindow.Instance.Print(new Run(obj.ToString()));
        public static void Print(object obj, SolidColorBrush color) => MainWindow.Instance.Print(new Run(obj.ToString()) { Foreground = color });
        public static void PrintLine(object obj) => Print($"{obj}\n");
        public static void PrintLine(Run r, bool replaceLast = true)
        {
            r.Text += "\n";
            if(replaceLast && MainWindow.Instance.Output.Inlines.Any())
            {
                MainWindow.Instance.Output.Inlines.Remove(MainWindow.Instance.Output.Inlines.LastInline);
            } 
            MainWindow.Instance.Print(r);  
        }
        public static void PrintLine(object obj, LineType type = LineType.Other) => Print(obj, type.Color());
        public static Run RunFor(object obj, LineType type) => new Run(obj.ToString()) { Foreground = type.Color() };
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
            foreach (string file in Directory.EnumerateFiles(path)) yield return file;
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
        public static string Readable(this byte[] arr)
        {
            string ret = "";
            foreach(byte b in arr) ret += b.ToString("X2");
            return ret;
        }
    }
    public enum LineType { Success, Failure, Existence, InProgress, Other } 
}
