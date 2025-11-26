using UnsortedOrderer.Contracts.Categories;
using UnsortedOrderer.Services;

namespace UnsortedOrderer.Categories;

public sealed class RepositoriesCategory : FileCategory, INonSplittableDirectoryCategory
{
    private static readonly string[] CodeExtensions =
    {
        ".cs",
        ".fs",
        ".vb",
        ".csproj",
        ".sln",
        ".js",
        ".ts",
        ".jsx",
        ".tsx",
        ".py",
        ".rb",
        ".go",
        ".rs",
        ".java",
        ".kt",
        ".kts",
        ".cpp",
        ".cxx",
        ".cc",
        ".c",
        ".h",
        ".hpp",
        ".swift",
        ".php",
        ".scala"
    };

    private readonly RepositoryDetector _repositoryDetector = new(CodeExtensions);

    public RepositoriesCategory(string folderName)
        : base("Repositories", folderName, CodeExtensions)
    {
    }

    public bool IsNonSplittableDirectory(string path)
    {
        return GetRepositoryRoot(path) is not null;
    }

    public string? GetRepositoryRoot(string path)
    {
        return _repositoryDetector.FindRepositoryRoot(path);
    }

    public string GetDirectoryDestination(string destinationRoot, string directoryPath)
    {
        var repositoryRoot = GetRepositoryRoot(directoryPath) ?? directoryPath;
        var categoryRoot = Path.Combine(destinationRoot, FolderName);
        Directory.CreateDirectory(categoryRoot);

        var directoryName = Path.GetFileName(repositoryRoot) ?? "Repository";
        return FileUtilities.GetUniqueDirectoryPath(categoryRoot, directoryName);
    }

    public string GetFileDestination(string destinationRoot, string filePath)
    {
        return Path.Combine(destinationRoot, FolderName);
    }
}
