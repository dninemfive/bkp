using d9.bkp.maui;

namespace bkp_maui;

public partial class MainPage : ContentPage
{
    public string Destination { get; private set; } // todo: populate from file picker or Entry
    public IEnumerable<string> SourceFolders { get; private set; } // ditto

    public MainPage()
    {
        InitializeComponent();
    }
    private async void StartButton_Clicked(object sender, EventArgs e)
    {
        using BackupModel model = new(Destination, SourceFolders);
        Progress<IoResult> progress = new((result) =>
        {
            // set progress bar progress
            // update labels
            // log result
        });
        await model.Backup(progress);
    }
}

