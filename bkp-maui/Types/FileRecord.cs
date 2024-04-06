namespace d9.bkp.maui;
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