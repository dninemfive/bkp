namespace d9.bkp.maui;
public static class Utils
{
    // cached to avoid issues when running near midnight
    private static DateTime? _today = null;
    public static string DateToday
    {
        get
        {
            _today ??= DateTime.Today;
            return _today.Value.ToString(Constants.DATE_FORMAT);
        }
    }
    public static IEnumerable<T> EnumerateSafe<T>(this IEnumerable<T> enumerable)
    {
        // https://stackoverflow.com/questions/3835633/wrap-an-ienumerable-and-catch-exceptions/34745417
        using IEnumerator<T> enumerator = enumerable.GetEnumerator();
        bool next = true;
        while (next)
        {
            try
            {
                next = enumerator.MoveNext();
            }
            catch (Exception e)
            {
                ConsoleUtils.Log(e);
            }
            if (next)
            {
                yield return enumerator.Current;
            }
        }
    }
}
