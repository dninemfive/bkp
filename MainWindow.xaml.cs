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
        public static Config Config { get; private set; }
        public Stopwatch Stopwatch { get; private set; } = new();
        public bool AutoScroll = false;
        public int BufferSize = 32;
        // to avoid garbage collection per https://docs.microsoft.com/en-us/dotnet/api/system.threading.timer
        public MainWindow()
        {
            if (Instance is not null) return;
            Instance = this;
            File.WriteAllText(Utils.LOG_PATH, "");
            InitializeComponent();        
        }
        private void UpdateTimer(object sender, PropertyChangedEventArgs e)
        {
            // https://stackoverflow.com/a/9732853
            Application.Current.Dispatcher.Invoke(delegate()
            {
                TimeElapsed.Text = $"{Stopwatch.Elapsed:hh\\:mm\\:ss}";
                ForceUpdate();
            }, DispatcherPriority.Background);
        }
        public void Print(Run r)
        {
            // delete runs starting from the beginning if buffer is full
            for (int i = 0; i <= Output.Inlines.Count - BufferSize; i++) Output.Inlines.Remove(Output.Inlines.FirstInline);
            Output.Inlines.Add(r);
        }
        public void UpdateProgress(object obj, IoResult type, long amount)
        {
            Application.Current.Dispatcher.Invoke(() => UpdateProgressInternal(obj, type, amount));
            ForceUpdate();
        }
        public long RunningTotal { get; private set; }
        public int NumFilesWhichExisted { get; private set; } = 0;
        private void UpdateProgressInternal(object obj, IoResult type, long amount)
        {
            if(amount >= 0)
            {
                RunningTotal += amount;
                Progress.Value = RunningTotal;
                ProgressText.Text = $"{RunningTotal.Readable()}/{Indexer.Size.Readable()} ({(RunningTotal / (double)Indexer.Size):P1})";
            }
            if(type == IoResult.Existence)
            {
                NumFilesWhichExisted++;
            }
            else
            {
                NumFilesWhichExisted = 0;
            }
            if(NumFilesWhichExisted > 0)
            {
                Utils.PrintLine(Utils.RunFor($"[{NumFilesWhichExisted} files which already existed]", IoResult.Existence), NumFilesWhichExisted > 1);
            } 
            else
            {
                Utils.PrintLine(Utils.RunFor(obj, type), false);
            }            
            if(AutoScroll) Scroll.ScrollToBottom();
        }
        private void Button_SelectTargetFolder(object sender, RoutedEventArgs e)
        {

        }
        private void Button_SelectBackupFolders(object sender, RoutedEventArgs e)
        {

        }
        private async void Button_StartBackup(object sender, RoutedEventArgs _)
        {
            using Timer timer = new(new TimerCallback((s) => UpdateTimer(this, new PropertyChangedEventArgs(nameof(Stopwatch)))), null, 0, 500);
            ButtonHolder.Visibility = Visibility.Collapsed;
            Utils.PrintLine($"Beginning backup...");
            Progress.IsIndeterminate = true;
            Stopwatch.Reset();
            Stopwatch.Start();
            RunningTotal = 0;
            Progress.Maximum = await Task.Run(() => Config.Size);
            Utils.PrintLineAndLog($"Time to calculate size was {Stopwatch.Elapsed:hh\\:mm\\:ss}.");
            Progress.IsIndeterminate = false;
            try
            {
                await Indexer.Backup();
            }
            catch (Exception e)
            {
                Utils.Log(e);
            }
            Utils.PrintLineAndLog($"Total time to back up was {Stopwatch.Elapsed:hh\\:mm\\:ss}.");             
            Stopwatch.Stop();
            timer.Dispose();            
        }
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
        private void BufferSizeBox_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                TextBox box = sender as TextBox;
                ValidationResult validation = BufferSizeRule.Validate(box.Text);
                if (validation.IsValid)
                {
                    box.Foreground = new SolidColorBrush(Colors.Black);
                    BufferSize = int.Parse(box.Text);
                }
                else
                {
                    box.Foreground = new SolidColorBrush(Colors.Red);
                    // todo: box tooltip with error
                }
                BindingExpression binding = BindingOperations.GetBindingExpression(box, TextBox.TextProperty);
                binding?.UpdateSource();
            }            
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            AutoScroll = true;
            AutoscrollCheckbox.IsChecked = AutoScroll;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            AutoScroll = false;
            AutoscrollCheckbox.IsChecked = AutoScroll;
        }
    }
}
