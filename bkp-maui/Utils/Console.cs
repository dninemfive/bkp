using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace d9.bkp.maui
{
    public static class Console
    {        
        public static Color Color(this ResultCategory lt) => lt switch
        {
            // colors tested with https://www.color-blindness.com/coblis-color-blindness-simulator/ and seem fine for all except maybe protanopia
            ResultCategory.Success    => Colors.LimeGreen,
            ResultCategory.Failure    => Colors.Red,
            ResultCategory.NoChange   => Colors.Cyan,
            ResultCategory.InProgress => Colors.Yellow,
            _                         => Colors.White
        };
        public static void Log(object obj) => File.AppendAllText(Constants.LOG_PATH, $"{obj}\n");
        public static void Print(object obj, ResultCategory category = ResultCategory.Other, bool replaceLast = false)
        {
            Label label = LabelFor(obj, category);
            List<Label> lines = new(); // todo: cache this each time and do a full update?
            // or: pass in an ObservableCollection?
            if(replaceLast && lines.Any())
            {
                lines.RemoveAt(lines.Count - 1);
            }
            lines.Add(label);
        }
        public static void PrintAndLog(object obj)
        {
            Print(obj);
            Log(obj);
        }
        public static Label LabelFor(object obj, ResultCategory type = ResultCategory.Other)
        {
            return new()
            {
                Margin = new Thickness(0),
                TextColor = type.Color(),
                Text = $"{obj}"
            };
        }
        #region readability
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
            _    => "?B"
        };
        #endregion readability
    }
}
