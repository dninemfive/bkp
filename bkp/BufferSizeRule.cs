using System;
using System.Globalization;
using System.Windows.Controls;

namespace bkp
{
    /// <summary>
    /// Validates that a string in a <c>TextBox</c> is a valid buffer size, i.e. an integer greater than 0.
    /// </summary>
    /// <remarks>Largely copied from https://docs.microsoft.com/en-us/dotnet/desktop/wpf/data/how-to-implement-binding-validation?view=netframeworkdesktop-4.8. </remarks>
    public class BufferSizeRule : ValidationRule
    {
        private static readonly BufferSizeRule Inst = new();
        public static ValidationResult Validate(string s)
        {
            return Inst.Validate(s, CultureInfo.InvariantCulture);
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int result = default;
            try
            {
                if (value is string s && s.Length > 0)
                {
                    result = int.Parse(s);
                }
            }
            catch (Exception e)
            {
                return new ValidationResult(false, e.Message);
            }
            return result < 1 ? new ValidationResult(false, "Buffer size must be greater than zero.") : ValidationResult.ValidResult;
        }
    }
}
