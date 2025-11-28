using UnsortedOrderer.Categories;
using UnsortedOrderer.Contracts.Categories;
using UnsortedOrderer.Contracts.Services;

namespace UnsortedOrderer.Services;

public sealed class RepositoryDetector : IFileCategoryParsingService
{
    private const int DefaultMinimumCodeFiles = 3;

    private static readonly string[] RepositoryMarkers =
    {
        ".git",
        ".hg",
        ".svn"
    };

    private static readonly string[] RepositoryManifestFiles =
    {
        "package.json",
        "pnpm-lock.yaml",
        "yarn.lock",
        "package-lock.json",
        "tsconfig.json",
        "pyproject.toml",
        "requirements.txt",
        "Pipfile",
        "Gemfile",
        "go.mod",
        "Cargo.toml",
        "composer.json",
        "Makefile",
        "CMakeLists.txt"
    };

    private readonly HashSet<string> _codeExtensions;
    private readonly int _minimumCodeFiles;

    public RepositoryDetector(IEnumerable<string> codeExtensions, int minimumCodeFiles = DefaultMinimumCodeFiles)
    {
        _codeExtensions = codeExtensions
            .Select(NormalizeExtension)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        _minimumCodeFiles = Math.Max(1, minimumCodeFiles);
    }

    public bool IsRepositoryDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            return false;
        }

        if (RepositoryMarkers.Any(marker => Directory.Exists(Path.Combine(path, marker))))
        {
            return true;
        }

        if (ContainsManifest(path))
        {
            return true;
        }

        return ContainsEnoughCodeFiles(path);
    }

    public string? FindRepositoryRoot(string path)
    {
        if (!Directory.Exists(path))
        {
            return null;
        }

        var current = path;
        string? repositoryPath = null;

        while (true)
        {
            if (IsRepositoryDirectory(current))
            {
                repositoryPath = current;
            }

            var subDirectories = Directory.GetDirectories(current);
            var hasTopLevelFiles = Directory.EnumerateFiles(current, "*", SearchOption.TopDirectoryOnly).Any();

            if (subDirectories.Length != 1 || hasTopLevelFiles)
            {
                break;
            }

            current = subDirectories[0];
        }

        return repositoryPath;
    }

    public bool IsFileOfCategory<TCategory>(string filePath)
        where TCategory : ICategory
    {
        if (typeof(TCategory) != typeof(RepositoriesCategory))
        {
            return false;
        }

        var fileName = Path.GetFileName(filePath);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        if (RepositoryManifestFiles.Any(manifest =>
                fileName.Equals(manifest, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        var extension = Path.GetExtension(filePath);
        return !string.IsNullOrWhiteSpace(extension) && _codeExtensions.Contains(extension.ToLowerInvariant());
    }

    public bool IsFolderOfCategory<TCategory>(string folderPath)
        where TCategory : ICategory
    {
        return typeof(TCategory) == typeof(RepositoriesCategory) && IsRepositoryDirectory(folderPath);
    }

    private static string NormalizeExtension(string extension)
    {
        return extension.StartsWith('.')
            ? extension.ToLowerInvariant()
            : $".{extension.ToLowerInvariant()}";
    }

    private bool ContainsManifest(string path)
    {
        var files = Directory.EnumerateFiles(path, "*", SearchOption.TopDirectoryOnly);
        return files.Any(file => RepositoryManifestFiles.Any(
            manifest => string.Equals(
                Path.GetFileName(file),
                manifest,
                StringComparison.OrdinalIgnoreCase)));
    }

    private bool ContainsEnoughCodeFiles(string path)
    {
        var count = 0;

        foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
        {
            var extension = Path.GetExtension(file);
            if (string.IsNullOrWhiteSpace(extension))
            {
                continue;
            }

            if (!_codeExtensions.Contains(extension.ToLowerInvariant()))
            {
                continue;
            }

            count++;
            if (count >= _minimumCodeFiles)
            {
                return true;
            }
        }

        return false;
    }
}
