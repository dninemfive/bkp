﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
        public bool AutoScroll = false;
        public int BufferSize = 32;
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
        public void Print(Run r)
        {
            // delete runs starting from the beginning if buffer is full
            for (int i = 0; i <= Output.Inlines.Count - BufferSize; i++) Output.Inlines.Remove(Output.Inlines.FirstInline);
            Output.Inlines.Add(r);
        }
        public void PrintLineFromThread(string line, LineType type = LineType.Other) => UpdateProgress(Utils.RunFor(line, type), -1);
        public void UpdateProgress(Run run, long amount, string summary = null, bool replacePrevious = false)
        {
            Application.Current.Dispatcher.Invoke(() => UpdateProgressInternal(run, amount, summary, replacePrevious));
            ForceUpdate();
        }
        private void UpdateProgressInternal(Run run, long amount, string summary, bool replacePrevious)
        {
            if(amount >= 0)
            {
                Progress.Value += amount;
            }           
            if(summary is not null)
            {
                ProgressText.Text = summary;
            }
            Utils.PrintLine(run, replacePrevious);
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
            Utils.PrintLine($"Final backup duration was {Stopwatch.Elapsed:hh\\:mm\\:ss}");
        }
        // this code is *very* similar - definitely should either make a parent type or shared base function
        private async void Button_StartIndex(object sender, RoutedEventArgs e)
        {
            using Timer timer = new(new TimerCallback((s) => UpdateTimer(this, new PropertyChangedEventArgs(nameof(Stopwatch)))), null, 0, 500);
            ButtonHolder.Visibility = Visibility.Collapsed;
            // todo: set this with a popup or smth
            string path = "C:/Users/dninemfive/Documents";
            Utils.PrintLine($"Started indexing {path}...");
            Stopwatch.Start();
            Progress.IsIndeterminate = true;            
            await Task.Run(() => FileRegistry.SetSize(path)); // load backup.size for the first time in a thread so the loading bar works properly
            Progress.Maximum = FileRegistry.Size;
            Progress.IsIndeterminate = false;
            await FileRegistry.Index(path);
            Stopwatch.Stop();
            timer.Dispose();
            Utils.PrintLine($"Final index duration was {Stopwatch.Elapsed:hh\\:mm\\:ss}");
            Progress.IsIndeterminate = true;
            await Task.Run(() => FileRegistry.Save($"{path}/index.bkp"));
            Progress.Value = 0;
            Progress.IsIndeterminate = false;
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
