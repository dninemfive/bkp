using System.Security.Cryptography;

namespace d9.bkp.maui;
public static class Files
{
    public static async Task<string> HashFileAsync(this string path)
    {
        return await Task.Run(() =>
        {
            // https://stackoverflow.com/a/51966515
            using SHA512 sha512 = SHA512.Create();
            using FileStream fs = File.OpenRead(path);
            return BitConverter.ToString(sha512.ComputeHash(fs)).Replace("-", "");
        });
    }
    // https://stackoverflow.com/a/64662525
    // https://devblogs.microsoft.com/dotnet/understanding-the-whys-whats-and-whens-of-valuetask/
    public static async Task<long> TotalSizeAsync(this string path)
    {
        List<Task<long?>> tasks = new();
        foreach (string filePath in path.AllFilesRecursive())
        {
            tasks.Add(filePath.FileSizeAsync());
        }
        long?[] results = await Task.WhenAll(tasks);
        return results.Where(x => x.HasValue).Select(x => x.Value).Sum();
    }
    public static async Task<long?> FileSizeAsync(this string filePath)
    {
        long? result = null;
        try
        {
            result = await Task.Run(() => new FileInfo(filePath).Length);
        }
        catch (Exception e)
        {
            Console.Log(e);
            return result;
        }
        return result;
    }
    public static void DeleteEmptySubfolders(this string path)
    {
        foreach (string subfolder in Directory.EnumerateDirectories(path))
        {
            subfolder.DeleteEmptySubfolders();
        }

        int numFiles = Directory.GetFiles(path).Length, numFolders = Directory.GetDirectories(path).Length;
        if (numFiles + numFolders == 0)
        {
            try
            {
                Directory.Delete(path);
            }
            catch (Exception e)
            {
                Console.Log(e);
            }
        }
    }
    public static IEnumerable<string> AllFilesRecursive(this string path)
    {
        if (!Directory.Exists(path))
        {
            yield break;
        }

        foreach (string subfolder in Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories).EnumerateSafe())
        {
            foreach (string file in Directory.EnumerateFiles(subfolder).EnumerateSafe())
            {
                yield return file;
            }
        }
        foreach (string file in Directory.EnumerateFiles(path).EnumerateSafe())
        {
            yield return file;
        }
    }
}
