using System.Text.Json;
namespace d9.bkp.maui;
public class BackupModel : IDisposable
{
    private bool _disposed = false;
    public string Destination { get; private set; }
    public IEnumerable<string> SourceFolders { get; private set; }
    public string TempFilePath { get; private set; } = null;
    private StreamWriter StreamWriter { get; set; }
    public BackupModel(string destination, IEnumerable<string> sourceFolders)
    {
        Destination = destination;
        TempFilePath = Path.Join(Destination, $"{Utils.DateToday}.bkp.temp");
        SourceFolders = sourceFolders;
        StreamWriter = File.AppendText(TempFilePath);
    }
    public async Task Backup(Progress<IoResult> progress)
    {
        string indexFolder = Path.Join(Destination, "_index");
        try
        {
            foreach (string folder in SourceFolders)
            {
                foreach (string file in folder.AllFilesRecursive())
                {
                    await IndexAndCopy(file, indexFolder, progress);
                }
            }
        }
        finally
        {
            Console.Log("Flushing...");
            StreamWriter.Flush();
            StreamWriter.Close();
        }
        File.Move(TempFilePath, TempFilePath.Replace(".temp", ""));
    }
    public async Task<string> Index(string path)
    {
        FileRecord fr = await FileRecord.For(path);
        string line = JsonSerializer.Serialize(fr);
        StreamWriter.WriteLine(line);
        return fr.Hash;
    }
    public async Task IndexAndCopy(string filePath, string indexFolder, Progress<IoResult> progress)
    {
        IoResult result;
        try
        {
            string hash = await Index(filePath);
            result = await IO.TryCopyAsync(filePath, Path.Join(indexFolder, hash));
        }
        catch
        {
            result = new(filePath, ResultCategory.Failure, -1);
        }
        ((IProgress<IoResult>)progress).Report(result);
    }
    public Task CleanUp(string filePath)
    {
        throw new NotImplementedException();
        /*
        IEnumerable<string> lines = File.ReadAllLines(filePath);
        HashSet<string> records = lines.ToHashSet();
        Console.PrintAndLog($"{lines.Count()} unique lines and {records.Count} unique lines to clean up.");
        Queue<string> toWrite = new(records.OrderBy(x => x));
        Utils.InvokeInMainThread(() => MainWindow.Instance.Progress.Maximum = records.Count);
        string tempFilePath = $"{filePath}.temp";
        StreamWriter sw = File.AppendText(tempFilePath);
        while (toWrite.TryDequeue(out string s))
        {
            sw.WriteLine(s);
            MainWindow.Instance.UpdateProgress(s, ResultCategory.Success, 1, records.Count);
        }
        sw.Flush();
        sw.Close();
        File.Delete(filePath);
        File.Move(tempFilePath, filePath);
        return Task.CompletedTask;
        */
    }
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            if (disposing)
            {
                StreamWriter.Dispose();
            }
            _disposed = true;
        }
    }
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
public class FileRecord
{
    public string Path { get; private set; }
    public string Hash { get; private set; }
    private FileRecord(string path, string hash)
    {
        Path = path;
        Hash = hash;
    }
    public static async Task<FileRecord> For(string path)
    {
        string hash = await path.HashFileAsync();
        return new(path, hash);
    }
}
