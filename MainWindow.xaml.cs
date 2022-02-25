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
            BackgroundWorker stopwatchWorker = StartStopwatch();
            stopwatchWorker.RunWorkerAsync();
            Progress.Maximum = Backup.Size;
            Backup.DoBackup();
            stopwatchWorker.CancelAsync();
        }
        public void Print(Run r) => Output.Inlines.Add(r);
        public void UpdateProgress(long amount)
        {
            Progress.Value += amount;
            ProgressText.Text = $"{Backup.RunningTotal}/{Backup.Size} ({(double)(Backup.RunningTotal/Backup.Size):P1})";
        }        
        private BackgroundWorker StartStopwatch()
        {            
            Stopwatch.Start();
            // https://stackoverflow.com/questions/5483565/how-to-use-wpf-background-worker
            BackgroundWorker worker = new();
            worker.DoWork += RunStopwatch;
            worker.RunWorkerCompleted += StopStopwatch;
            return worker;
        }
        // https://stackoverflow.com/questions/46765692/c-sharp-wpf-async-thread-with-interface-to-gui
        private void RunStopwatch(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                TimeElapsed.Text = $"{Stopwatch.Elapsed:hh\\:mm\\:ss}";
            }
        }
        private void StopStopwatch(object sender, RunWorkerCompletedEventArgs rca)
        {
            Stopwatch.Stop();
            Utils.Log($"Stopwatch ended at {Stopwatch.Elapsed:hh\\:mm\\:ss}: {rca}");
        }
    }
}
