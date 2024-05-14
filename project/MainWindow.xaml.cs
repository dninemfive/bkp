using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
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
        public int BufferSize = 1024;
        public MainWindow()
        {
            if (Instance is not null) return;
            Instance = this;
            File.WriteAllText(Constants.LOG_PATH, "");
            InitializeComponent();
            Config = new("testing", File.ReadAllLines("bkp.sources"), @"D:\Automatic");
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
        public void Print(Block b)
        {
            // delete runs starting from the beginning if buffer is full
            for (int i = 0; i <= Output.Blocks.Count - BufferSize; i++) Output.Blocks.Remove(Output.Blocks.FirstBlock);
            Output.Blocks.Add(b);
        }
#region UpdateProgress
        public void UpdateProgress(object obj, ResultCategory category, long size, long? overrideMax = null)
        {
            Utils.InvokeInMainThread(() => UpdateProgressInternal(obj, category, size, overrideMax), DispatcherPriority.Send);
            Utils.ForceUpdate();
        }
        public void UpdateProgress(IoResult result) => UpdateProgress(result.oldFilePath, result.category, result.size);        
        private void UpdateProgressInternal(object obj, ResultCategory category, long size, long? overrideMax = null)
        {
            if(size >= 0)
            {
                Progress.IsIndeterminate = false;
                RunningTotal += size;
                Progress.Value = RunningTotal;
                if(overrideMax is not null)
                {
                    ProgressText.Text = $"{RunningTotal}/{overrideMax} ({(RunningTotal / (double)overrideMax):P1})";
                }
                else
                {
                    ProgressText.Text = $"{RunningTotal.Readable()}/{Config.Size.Readable()} ({(RunningTotal / (double)Config.Size):P1})";
                }
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
                Console.Print($"[{NumFilesWhichExisted} files which already existed]", ResultCategory.NoChange, NumFilesWhichExisted > 1);
            } 
            else
            {
                Console.Print(obj, category);
            }
            if (AutoScroll) Scroll.ScrollToBottom();
        }
#endregion UpdateProgress
        private void Button_Settings(object sender, RoutedEventArgs e)
        {

        }
        private Timer HideButtonsAndStartTimer()
        {
            Timer result = new(new TimerCallback((s) => UpdateTimer(this, new PropertyChangedEventArgs(nameof(Stopwatch)))), null, 0, 500);
            ButtonHolder.Visibility = Visibility.Collapsed;
            Stopwatch.Reset();
            Stopwatch.Start();
            return result;
        }
        private async void Button_CleanUp(object sender, RoutedEventArgs _)
        {
            using Timer timer = HideButtonsAndStartTimer();
            string bkpFile = System.IO.Path.Join(Config.DestinationFolder, $"{Console.DateToday}.bkp");
            Console.PrintAndLog($"Cleaning up {bkpFile}...");                     
            Progress.IsIndeterminate = true;
            try
            {
                await BkpGenerator.CleanUp(bkpFile);
            } 
            catch(Exception ex)
            {
                Console.Log(ex);
            }
            Console.PrintAndLog($"Total time to clean up was {Stopwatch.Elapsed:hh\\:mm\\:ss}.");
            Stopwatch.Stop();
            timer.Dispose();
        }
        private async void Button_StartBackup(object sender, RoutedEventArgs _)
        {
            using Timer timer = HideButtonsAndStartTimer();
            Console.PrintAndLog($"Beginning backup...");
            Progress.IsIndeterminate = true;            
            RunningTotal = 0;
            long size = -1;
            try
            {
                size = await Task.Run(() => Config.CalculateSizeAsync());
                Utils.InvokeInMainThread(() => Progress.Maximum = size);
            } 
            catch(Exception e)
            {
                Console.Log(e);
            }
            Console.PrintAndLog($"Time to calculate size {size.Readable()} was {Stopwatch.Elapsed:hh\\:mm\\:ss}.");
            Progress.IsIndeterminate = false;
            try
            {
                await BkpGenerator.Backup();
            }
            catch (Exception e)
            {
                Console.Log(e);
            }
            Console.PrintAndLog($"Total time to back up was {Stopwatch.Elapsed:hh\\:mm\\:ss}.");             
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
        protected override void OnClosed(EventArgs e)
        {
            if (BkpGenerator.TempFilePath is not null && File.Exists(BkpGenerator.TempFilePath)) File.Delete(BkpGenerator.TempFilePath);
        }

        private async void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            Progress<(string path, bool applies)> progress = new((x) => UpdateProgress(x.path, x.applies ? ResultCategory.Success : ResultCategory.NoChange, 1));
            using Timer timer = HideButtonsAndStartTimer();
            Progress.IsIndeterminate = true;
            Console.Print($"asdf");
            int ct = (await File.ReadAllLinesAsync(@"D:\Automatic\2024.5.14.bkp")).Length;
            Console.Print($"jkl;");
            Utils.InvokeInMainThread(() => Progress.Maximum = ct);
            Console.Print($"ewt;");
            Progress.IsIndeterminate = false;
            Console.Print($"asefrie4whot");
            await Restore.RestoreStuff(progress);
            Console.Print("done");
        }
    }
}
