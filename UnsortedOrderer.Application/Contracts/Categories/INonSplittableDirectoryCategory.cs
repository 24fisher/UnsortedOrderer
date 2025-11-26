namespace UnsortedOrderer.Contracts.Categories;

public interface INonSplittableDirectoryCategory : IFileCategory
{
    bool IsNonSplittableDirectory(string path);

    string GetDirectoryDestination(string destinationRoot, string directoryPath);

    string GetFileDestination(string destinationRoot, string filePath);
}
