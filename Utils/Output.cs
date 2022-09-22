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
    public static class Output
    {        
        public static SolidColorBrush Color(this ResultCategory lt) => lt switch
        {
            // colors tested with https://www.color-blindness.com/coblis-color-blindness-simulator/ and seem fine for all except maybe protanopia
            ResultCategory.Success    => new(Colors.LimeGreen),
            ResultCategory.Failure    => new(Colors.Red),
            ResultCategory.NoChange  => new(Colors.Cyan),
            ResultCategory.InProgress => new(Colors.Yellow),
            _                   => new(Colors.White)
        };
        public static void Log(object obj) => File.AppendAllText(Constants.LOG_PATH, $"{obj}\n");
        public static void Print(object obj) => MainWindow.Instance.Print(RunFor(obj));
        public static void PrintLine(object obj) => Print($"{obj}\n");
        public static void PrintLine(Run r, bool replaceLast)
        {
            r.Text += "\n";
            if(replaceLast && MainWindow.Instance.Output.Inlines.Any())
            {
                MainWindow.Instance.Output.Inlines.Remove(MainWindow.Instance.Output.Inlines.LastInline);
            } 
            MainWindow.Instance.Print(r);
        }
        public static void PrintLineAndLog(object obj)
        {
            PrintLine(obj);
            Log(obj);
        }
        public static Run RunFor(object obj, ResultCategory type = ResultCategory.Other) => new Run(obj.ToString()) { Foreground = type.Color() };
        #region readability
        // cached to avoid issues when running near midnight
        private static DateTime? _today = null;
        public static string DateToday
        {
            get
            {
                if (_today is null) _today = DateTime.Now;
                return _today.Value.ToString(Constants.DATE_FORMAT);
            }
        }
        public static string Readable(this long bytes)
        {
            int digits = bytes.Digits();
            return $"{(bytes / (double)digits.Divisor()):F3} {digits.Suffix()}";
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
        #endregion readability
    }
}
