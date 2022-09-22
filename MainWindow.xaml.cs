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
        public long RunningTotal { get; private set; }
        public int NumFilesWhichExisted { get; private set; } = 0;
        public bool AutoScroll = false;
        public int BufferSize = 71;
        public MainWindow()
        {
            if (Instance is not null) return;
            Instance = this;
            File.WriteAllText(Constants.LOG_PATH, "");
            InitializeComponent();        
        }
        private void UpdateTimer(object sender, PropertyChangedEventArgs e)
        {
            // https://stackoverflow.com/a/9732853
            Utils.InvokeInMainThread(delegate ()
            {
                TimeElapsed.Text = $"{Stopwatch.Elapsed:hh\\:mm\\:ss}";
                Utils.ForceUpdate();
            });
        }
        public void Print(Run r)
        {
            // delete runs starting from the beginning if buffer is full
            for (int i = 0; i <= Output.Inlines.Count - BufferSize; i++) Output.Inlines.Remove(Output.Inlines.FirstInline);
            Output.Inlines.Add(r);
        }
#region UpdateProgress
        public void UpdateProgress(object obj, ResultCategory category, long size)
        {
            Utils.InvokeInMainThread(() => UpdateProgressInternal(obj, category, size), DispatcherPriority.Send);
            Utils.ForceUpdate();
        }
        public void UpdateProgress(IoResult result) => UpdateProgress(result.oldFilePath, result.category, result.size);        
        private void UpdateProgressInternal(object obj, ResultCategory category, long size)
        {
            if(size >= 0)
            {
                RunningTotal += size;
                Progress.Value = RunningTotal;
                ProgressText.Text = $"{RunningTotal.Readable()}/{Config.Size.Readable()} ({(RunningTotal / (double)Config.Size):P1})";
            }
            if(category == ResultCategory.NoChange)
            {
                NumFilesWhichExisted++;
            }
            else
            {
                NumFilesWhichExisted = 0;
            }
            if(NumFilesWhichExisted > 0)
            {
                Console.PrintLine(Console.RunFor($"[{NumFilesWhichExisted} files which already existed]", ResultCategory.NoChange), NumFilesWhichExisted > 1);
            } 
            else
            {
                Console.PrintLine(Console.RunFor(obj, category), false);
            }            
            if(AutoScroll) Scroll.ScrollToBottom();
        }
#endregion UpdateProgress
        private void Button_SelectTargetFolder(object sender, RoutedEventArgs e)
        {

        }
        private void Button_SelectBackupFolders(object sender, RoutedEventArgs e)
        {

        }
        private async void Button_StartBackup(object sender, RoutedEventArgs _)
        {
            Config = new("testing", File.ReadAllLines("bkp.sources"), @"D:\Automatic");

            using Timer timer = new(new TimerCallback((s) => UpdateTimer(this, new PropertyChangedEventArgs(nameof(Stopwatch)))), null, 0, 500);
            ButtonHolder.Visibility = Visibility.Collapsed;
            Console.PrintLineAndLog($"Beginning backup...");
            Progress.IsIndeterminate = true;
            Stopwatch.Reset();
            Stopwatch.Start();
            RunningTotal = 0;
            long size = -1;
            try
            {
                size = await Task.Run(() => Config.CalculateSizeAsync());
                Application.Current.Dispatcher.Invoke(() => Progress.Maximum = size);
            } 
            catch(Exception e)
            {
                Console.Log(e);
            }
            Console.PrintLineAndLog($"Time to calculate size {size} was {Stopwatch.Elapsed:hh\\:mm\\:ss}.");
            Progress.IsIndeterminate = false;
            try
            {
                await Indexer.Backup();
            }
            catch (Exception e)
            {
                Console.Log(e);
            }
            Console.PrintLineAndLog($"Total time to back up was {Stopwatch.Elapsed:hh\\:mm\\:ss}.");             
            Stopwatch.Stop();
            timer.Dispose();            
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
