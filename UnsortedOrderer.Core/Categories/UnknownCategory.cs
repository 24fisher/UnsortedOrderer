namespace UnsortedOrderer.Categories;

public sealed class UnknownCategory : IFileCategory
{
    public UnknownCategory(string folderName)
    {
        FolderName = folderName;
    }

    public string Name => "Unknown";

    public string FolderName { get; }

    public IReadOnlyCollection<string> Extensions => Array.Empty<string>();

    public bool Matches(string extension)
    {
        return false;
    }
}
