namespace UnsortedOrderer.Contracts.Categories;

public interface INonSplittableDirectoryCategory : ICategory
{
    bool IsNonSplittableDirectory(string path);

    string GetDirectoryDestination(string destinationRoot, string directoryPath);

    string GetFileDestination(string destinationRoot, string filePath);
}
