using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;

namespace bkp
{
    public static class Utils
    {
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
                    Console.Log(e);
                }
                if (next)
                {
                    yield return enumerator.Current;
                }
            }
        }
        public static void InvokeInMainThread(this Action action, DispatcherPriority priority = DispatcherPriority.Background)
        {
            Application.Current.Dispatcher.Invoke(action, priority);
        }

        // https://stackoverflow.com/a/616676
        public static void ForceUpdate(DispatcherPriority priority = DispatcherPriority.Background)
        {
            DispatcherFrame frame = new();
            _ = Dispatcher.CurrentDispatcher.BeginInvoke(priority, new DispatcherOperationCallback(delegate (object _)
            {
                frame.Continue = false;
                return null;
            }), null);
            Dispatcher.PushFrame(frame);
        }
    }
}
