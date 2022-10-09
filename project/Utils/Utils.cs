using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

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
                    Console.Log(e);
                }
                if (next)
                {
                    yield return enumerator.Current;
                }
            }
        }
        public static void InvokeInMainThread(this Action action, DispatcherPriority priority = DispatcherPriority.Background) 
            => Application.Current.Dispatcher.Invoke(action, priority);
        // https://stackoverflow.com/a/616676
        public static void ForceUpdate()
        {
            DispatcherFrame frame = new();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(delegate (object parameter)
            {
                frame.Continue = false;
                return null;
            }), null);
            Dispatcher.PushFrame(frame);
        }
    }
}
