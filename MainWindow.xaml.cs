using System;
using System.Collections.Generic;
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

namespace bkp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {        
        public static MainWindow Instance { get; private set; } = null;
        public static Stopwatch Stopwatch { get; private set; } = new();
        public MainWindow()
        {
            if (Instance is not null) return;
            Instance = this;
            InitializeComponent();                     
            File.Delete(Utils.LOG_PATH);
        }
        public Task DoStuff()
        {
            Progress<(Run run, long amount)> backupProgress = new(report => UpdateProgress(null, report));
            Progress.Maximum = Backup.Size;
            backupProgress.ProgressChanged += UpdateProgress;
            Utils.Log("\tjust got size");
            Task backupTask = Backup.Start(backupProgress);
            Utils.Log("\tstarted backup");
            while (!backupTask.IsCompleted)
            {
                TimeElapsed.Text = $"{Stopwatch.Elapsed:hh\\:mm\\:ss}";
            }
            Stopwatch.Stop();
            return Task.CompletedTask;
        }
        public void Print(Run r) => Output.Inlines.Add(r);
        public void UpdateProgress(object sender, (Run run, long amount) e)
        {
            Utils.Log($"{e.run.Text} {e.amount}");
            (Run run, long amount) = e;
            Progress.Value += amount;
            Backup.RunningTotal += amount;
            ProgressText.Text = $"{Backup.RunningTotal}/{Backup.Size} ({(double)(Backup.RunningTotal/Backup.Size):P1})";
            Utils.PrintLine(run);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            StartButton.Visibility = Visibility.Hidden;
            await DoStuff();
        }
    }
}
