using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace bkp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; } = null;
        public Stopwatch Stopwatch { get; private set; } = new();
        public bool AutoScroll = true;
        // to avoid garbage collection per https://docs.microsoft.com/en-us/dotnet/api/system.threading.timer
        public MainWindow()
        {
            if (Instance is not null) return;
            Instance = this;            
            InitializeComponent();
            File.Delete(Utils.LOG_PATH);            
        }
        private void UpdateTimer(object sender, PropertyChangedEventArgs e)
        {
            // https://stackoverflow.com/a/9732853
            Application.Current.Dispatcher.Invoke(delegate()
            {
                TimeElapsed.Text = $"{Stopwatch.Elapsed:hh\\:mm\\:ss}";
                ForceUpdate();
            }, DispatcherPriority.Render);
        }
        public void Print(Run r) => Output.Inlines.Add(r);
        public void UpdateProgress(Run run, long amount)
        {
            Application.Current.Dispatcher.Invoke(() => UpdateProgressInternal(run, amount));
            ForceUpdate();
        }
        private void UpdateProgressInternal(Run run, long amount)
        {
            if(amount >= 0)
            {
                Progress.Value += amount;
                ProgressText.Text = $"{Backup.RunningTotal}/{Backup.Size} ({(Backup.RunningTotal / (double)Backup.Size):P1})";
            }            
            Utils.PrintLine(run, amount >= 0);
            if(AutoScroll) Scroll.ScrollToBottom();
        }
        private void Button_SelectTargetFolder(object sender, RoutedEventArgs e)
        {

        }
        private void Button_SelectBackupFolders(object sender, RoutedEventArgs e)
        {

        }
        private async void Button_StartBackup(object sender, RoutedEventArgs e)
        {
            using Timer timer = new(new TimerCallback((s) => UpdateTimer(this, new PropertyChangedEventArgs(nameof(Stopwatch)))), null, 0, 500);
            ToggleScroll.Visibility = Visibility.Visible;
            ButtonHolder.Visibility = Visibility.Collapsed;
            Utils.PrintLine("Started backup...");
            Stopwatch.Start();
            Progress.IsIndeterminate = true;
            await Task.Run(() => _ = Backup.Size); // load backup.size for the first time in a thread so the loading bar works properly
            Progress.Maximum = Backup.Size;
            Progress.IsIndeterminate = false;
            await Backup.DoBackup();
            Stopwatch.Stop();
            timer.Dispose();
            Utils.PrintLine($"Final stopwatch time was {Stopwatch.Elapsed:hh\\:mm\\:ss}");
            ToggleScroll.Visibility = Visibility.Collapsed;
        }
        // https://stackoverflow.com/a/616676
        public static void ForceUpdate()
        {
            DispatcherFrame frame = new();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Render, new DispatcherOperationCallback(delegate (object parameter)
            {
                frame.Continue = false;
                return null;
            }), null);
            Dispatcher.PushFrame(frame);
        }

        private void Button_ToggleScroll(object sender, RoutedEventArgs e)
        {
            AutoScroll = !AutoScroll;
            ToggleScroll.Content = (AutoScroll ? "AUTO" : "MANUAL") + " Scrolling";
        }
    }
}
