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
            CancellationTokenSource stopwatchToken = new();
            _ = StartStopwatch(stopwatchToken.Token);
            Progress.Maximum = Backup.Size;
            /*
            Backup.DoBackup();
            */
            stopwatchToken.Cancel();
        }
        public void Print(Run r) => Output.Inlines.Add(r);
        public void UpdateProgress(long amount)
        {
            Progress.Value += amount;
            ProgressText.Text = $"{Backup.RunningTotal}/{Backup.Size} ({(double)(Backup.RunningTotal/Backup.Size):P1})";
        }
        public Task StartStopwatch(CancellationToken ct)
        {
            Stopwatch.Start();
            return Task.Factory.StartNew(delegate() 
                {
                    while(true)
                    {
                        try
                        {
                            TimeElapsed.Text = $"{Stopwatch.Elapsed:hh\\:mm\\:ss}";
                        }
                        catch(Exception e)
                        {
                            Stopwatch.Stop();
                            Utils.Log($"Stopwatch ended at {Stopwatch.Elapsed:hh\\:mm\\:ss}: {e.Message}");
                        }
                    }                
                }, ct);
        }
    }
}
