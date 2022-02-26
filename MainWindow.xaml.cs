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
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public static MainWindow Instance { get; private set; } = null;
        public Stopwatch Stopwatch { get; private set; } = new();
        public event PropertyChangedEventHandler PropertyChanged;
        // to avoid garbage collection per https://docs.microsoft.com/en-us/dotnet/api/system.threading.timer
        private Timer _timer;
        public MainWindow()
        {
            if (Instance is not null) return;
            Instance = this;
            Stopwatch.Start();
            //PropertyChanged = new(TimerPropertyChanged);
            _timer = new Timer(new TimerCallback((s) => UpdateTimer(this, new PropertyChangedEventArgs(nameof(Stopwatch)))), null, 0, 500);
            //PropertyChanged += UpdateTimer;
            InitializeComponent();                     
            // File.Delete(Utils.LOG_PATH);
            // Progress.Maximum = Backup.Size;
            StartButton.Visibility = Visibility.Visible;
        }
        // https://stackoverflow.com/questions/8302590/running-stopwatch-in-textblock/8302652#8302652
        private void TimerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged(sender, e);
        }
        private void UpdateTimer(object sender, PropertyChangedEventArgs e)
        {
            // https://stackoverflow.com/a/9732853
            Application.Current.Dispatcher.Invoke(delegate()
            {
                TimeElapsed.Text = $"{Stopwatch.Elapsed:hh\\:mm\\:ss}";
            }, DispatcherPriority.ContextIdle);
            //Dispatcher.Invoke(() =>{
                
            //});
            
        }
        public void DoStuff(object sender, PropertyChangedEventArgs e)
        {            
            while (true)
            {
                ProgressText.Text = $"{Stopwatch.Elapsed:hh\\:mm\\:ss}";
            }
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ProgressText.Text = Stopwatch.Elapsed.ToString();
            //StartButton.Visibility = Visibility.Hidden;
            //await Task.Run(() => DoOtherStuff());
        }
        public void DoOtherStuff()
        {
            Stopwatch.Start();
            Backup.DoBackup();
            Stopwatch.Stop();
        }
    }
}
