namespace UnsortedOrderer.Contracts.Categories;

public interface ICategory
{
    string Name { get; }

    string FolderName { get; }

    IReadOnlyCollection<string> Extensions { get; }

    bool Matches(string extension);
}
