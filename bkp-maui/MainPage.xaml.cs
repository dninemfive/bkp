namespace d9.bkp.maui;
public partial class MainPage : ContentPage
{
    private string? Destination { get; set; } // todo: populate from file picker or Entry
    private List<string> SourceFolders { get; set; } = new();
    public MainPage()
    {
        InitializeComponent();
    }
    private async void StartButton_Clicked(object sender, EventArgs e)
    {
        Button? button = sender as Button;
        if (button is not null)
            button.IsEnabled = false;
        if(SourceFolders.Any() && Destination is not null)
        {
            using BackupModel model = new(Destination, SourceFolders);
            long totalSize = await model.TotalSizeAsync();
            long runningTotal = 0;
            Progress<IoResult> progress = new((result) =>
            {
                runningTotal += result.Size;
                double progressPct = runningTotal / (double)totalSize;
                BackupProgressBar.Progress = progressPct;
                ProgressLabel.Text = $"{runningTotal.Readable()}/{totalSize.Readable()} ({progressPct:P1})";
                // log result
            });
            await model.BackupAsync(progress);
        }
        if (button is not null)
            button.IsEnabled = true;
    }
}

