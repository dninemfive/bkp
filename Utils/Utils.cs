using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bkp
{
    public static class Utils
    {               
        public static IEnumerable<T> EnumerateSafe<T>(this IEnumerable<T> enumerable)
        {
            // https://stackoverflow.com/questions/3835633/wrap-an-ienumerable-and-catch-exceptions/34745417
            using var enumerator = enumerable.GetEnumerator();
            bool next = true;
            while (next)
            {
                try
                {
                    next = enumerator.MoveNext();
                }
                catch (Exception e)
                {
                    Output.Log(e);
                }
                if (next)
                {
                    yield return enumerator.Current;
                }
            }
        }
    }
}
