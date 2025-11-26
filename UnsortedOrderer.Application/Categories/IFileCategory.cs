namespace UnsortedOrderer.Categories;

public interface IFileCategory
{
    string Name { get; }

    string FolderName { get; }

    IReadOnlyCollection<string> Extensions { get; }

    bool Matches(string extension);
}
