using System;
using System.IO;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Media;

namespace bkp
{
    public static class Console
    {
        public static SolidColorBrush Color(this ResultCategory lt)
        {
            return lt switch
            {
                // colors tested with https://www.color-blindness.com/coblis-color-blindness-simulator/ and seem fine for all except maybe protanopia
                ResultCategory.Success => new(Colors.LimeGreen),
                ResultCategory.Failure => new(Colors.Red),
                ResultCategory.NoChange => new(Colors.Cyan),
                ResultCategory.InProgress => new(Colors.Yellow),
                _ => new(Colors.White)
            };
        }

        public static void Log(object obj)
        {
            File.AppendAllText(Constants.LOG_PATH, $"{obj}\n");
        }

        public static void Print(object obj, ResultCategory category = ResultCategory.Other, bool replaceLast = false)
        {
            Block block = BlockFor(obj, category);
            if (replaceLast && MainWindow.Instance.Output.Blocks.Any())
            {
                _ = MainWindow.Instance.Output.Blocks.Remove(MainWindow.Instance.Output.Blocks.LastBlock);
            }
            MainWindow.Instance.Print(block);
        }
        public static void PrintAndLog(object obj)
        {
            Print(obj);
            Log(obj);
        }
        public static Block BlockFor(object obj, ResultCategory type = ResultCategory.Other)
        {
            Run run = new($"{obj}");
            Paragraph result = new()
            {
                Margin = new(0),
                Foreground = type.Color()
            };
            result.Inlines.Add(run);
            return result;
        }
        #region readability
        // cached to avoid issues when running near midnight
        private static DateTime? _today = null;
        public static string DateToday
        {
            get
            {
                _today ??= DateTime.Now;
                return _today.Value.ToString(Constants.DATE_FORMAT);
            }
        }
        public static string Readable(this long bytes)
        {
            int digits = bytes.Digits();
            return $"{bytes / (double)digits.Divisor():F3} {digits.Suffix()}";
        }
        public static string Suffix(this int digits)
        {
            return digits switch
            {
                < 3 => "bytes",
                < 6 => "KB",
                < 9 => "MB",
                < 12 => "GB",
                < 15 => "TB",
                < 18 => "PB",
                < 21 => "EB",
                < 24 => "ZB",
                < 27 => "YB",
                _ => "unknown"
            };
        }
        #endregion readability
    }
}
