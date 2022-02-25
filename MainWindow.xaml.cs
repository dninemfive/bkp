using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
        const string LOG_PATH = "log.txt";
        public static MainWindow Instance { get; private set; } = null;
        public MainWindow()
        {
            if (Instance is not null) return;
            Instance = this;
            InitializeComponent();
            Stopwatch stopwatch = new();
            stopwatch.Start();
            File.Delete(LOG_PATH);
            Backup.DoBackup();
            Utils.PrintLine($"Time elapsed: {stopwatch.Elapsed}");
        }
        public void Print(string s) => Output.Text += s;
    }
}
