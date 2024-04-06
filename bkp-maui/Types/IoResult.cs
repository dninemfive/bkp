namespace d9.bkp.maui;
public enum ResultCategory { Success, Failure, NoChange, InProgress, Other }
public struct IoResult(string ofp, ResultCategory c, long s)
{
    public string OldFilePath = ofp;
    public ResultCategory Category = c;
    public long Size = s;
    public readonly void Deconstruct(out string oldFilePath, out ResultCategory category, out long size)
    {
        oldFilePath = OldFilePath;
        category = Category;
        size = Size;
    }
    public static implicit operator IoResult((string oldFilePath, ResultCategory category, long size) tuple)
        => new(tuple.oldFilePath, tuple.category, tuple.size);
}