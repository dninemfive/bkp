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
        // to avoid garbage collection per https://docs.microsoft.com/en-us/dotnet/api/system.threading.timer
        private Timer _timer;
        public MainWindow()
        {
            if (Instance is not null) return;
            Instance = this;
            _timer = new Timer(new TimerCallback((s) => UpdateTimer(this, new PropertyChangedEventArgs(nameof(Stopwatch)))), null, 0, 500);
            InitializeComponent();                     
            File.Delete(Utils.LOG_PATH);
        }
        private void UpdateTimer(object sender, PropertyChangedEventArgs e)
        {
            // https://stackoverflow.com/a/9732853
            Application.Current.Dispatcher.Invoke(delegate()
            {
                TimeElapsed.Text = $"{Stopwatch.Elapsed:hh\\:mm\\:ss}";
            }, DispatcherPriority.ContextIdle);
        }
        public void Print(Run r) => Output.Inlines.Add(r);
        public void UpdateProgress((Run run, long amount) e)
        {
            Utils.Log($"{e.run.Text} {e.amount}");
            (Run run, long amount) = e;
            Progress.Value += amount;
            Backup.RunningTotal += amount;
            ProgressText.Text = $"{Backup.RunningTotal}/{Backup.Size} ({(double)(Backup.RunningTotal/Backup.Size):P1})";
            Utils.PrintLine(run);
        }
        private void Button_SelectTargetFolder(object sender, RoutedEventArgs e)
        {

        }
        private void Button_SelectBackupFolders(object sender, RoutedEventArgs e)
        {

        }
        private void Button_StartBackup(object sender, RoutedEventArgs e)
        {
            StartButton.Visibility = Visibility.Collapsed;
            Stopwatch.Start();
            Progress.IsIndeterminate = true;
            Progress.Maximum = Backup.Size;
            Progress.IsIndeterminate = false;
            Backup.DoBackup();
            Stopwatch.Stop();
        }
    }
}
