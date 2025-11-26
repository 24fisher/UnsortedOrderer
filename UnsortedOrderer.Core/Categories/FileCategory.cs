namespace UnsortedOrderer.Categories;

public abstract class FileCategory : IFileCategory
{
    protected FileCategory(string name, string folderName, IEnumerable<string> extensions)
    {
        Name = name;
        FolderName = folderName;
        Extensions = extensions
            .Select(e => e.StartsWith('.') ? e : $".{e}")
            .Select(e => e.ToLowerInvariant())
            .ToArray();
    }

    public string Name { get; }

    public string FolderName { get; }

    public IReadOnlyCollection<string> Extensions { get; }

    public bool Matches(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return false;
        }

        var normalized = extension.StartsWith('.') ? extension.ToLowerInvariant() : $".{extension.ToLowerInvariant()}";
        return Extensions.Contains(normalized);
    }
}
