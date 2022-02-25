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
            DoStuff();
        }
        public void DoStuff()
        {
            Progress<(Run run, long amount)> backupProgress = new(report => UpdateProgress(report));
            Progress.Maximum = Backup.Size;
            Task backupTask = Backup.Start(backupProgress);
            while (!backupTask.IsCompleted)
            {
                TimeElapsed.Text = $"{Stopwatch.Elapsed:hh\\:mm\\:ss}";
            }
            Stopwatch.Stop();
        }
        public void Print(Run r) => Output.Inlines.Add(r);
        public void UpdateProgress((Run run, long amount) report)
        {
            (Run run, long amount) = report;
            Progress.Value += amount;
            Backup.RunningTotal += amount;
            ProgressText.Text = $"{Backup.RunningTotal}/{Backup.Size} ({(double)(Backup.RunningTotal/Backup.Size):P1})";
            Utils.PrintLine(run);
        }        
    }
}
