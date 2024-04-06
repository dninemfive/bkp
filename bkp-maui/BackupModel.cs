using System.Text.Json;
namespace d9.bkp.maui;
public class BackupModel : IDisposable
{
    public string Destination { get; private set; }
    public IEnumerable<string> SourceFolders { get; private set; }
    public string OutputFilePath { get; private set; } = null;
    private StreamWriter OutputFileWriter { get; set; }
    public BackupModel(string destination, IEnumerable<string> sourceFolders)
    {
        Destination = destination;
        OutputFilePath = Path.Join(Destination, $"{Utils.DateToday}.bkp.temp");
        SourceFolders = sourceFolders;
        OutputFileWriter = File.AppendText(OutputFilePath);
    }
    public IEnumerable<string> AllTargetFiles
    {
        get
        {
            foreach (string folder in SourceFolders)
                foreach (string file in folder.AllFilesRecursive())
                    yield return file;
        }
    }
    public async Task<long> TotalSizeAsync()
    {
        List<Task<long>> tasks = new();
        foreach (string folder in SourceFolders)
            tasks.Add(folder.TotalSizeAsync());
        return (await Task.WhenAll(tasks)).Sum();
    }
    public async Task BackupAsync(IProgress<IoResult> progress)
    {
        string indexFolder = Path.Join(Destination, "_index");
        List<Task> tasks = new();
        try
        {
            foreach (string file in AllTargetFiles)
                tasks.Add(IndexAndCopyAsync(file, indexFolder, progress));
            await Task.WhenAll(tasks);
        }
        finally
        {
            ConsoleUtils.Log("Flushing...");
            OutputFileWriter.Flush();
            OutputFileWriter.Close();
        }
        File.Move(OutputFilePath, OutputFilePath.Replace(".temp", ""));
    }
    private async Task<string> IndexAsync(string path)
    {
        FileRecord fr = await FileRecord.For(path);
        string line = JsonSerializer.Serialize(fr);
        OutputFileWriter.WriteLine(line);
        return fr.Hash;
    }
    private async Task IndexAndCopyAsync(string filePath, string indexFolder, IProgress<IoResult> progress)
    {
        IoResult result;
        try
        {
            string hash = await IndexAsync(filePath);
            result = await filePath.TryCopyToAsync(Path.Join(indexFolder, hash));
        }
        catch
        {
            result = new(filePath, ResultCategory.Failure, -1);
        }
        progress.Report(result);
    }
    #region IDisposable
    private bool _disposed = false;
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            if (disposing)
                OutputFileWriter.Dispose();
            _disposed = true;
        }
    }
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion IDisposable
}