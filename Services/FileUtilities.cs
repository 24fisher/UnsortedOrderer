namespace UnsortedOrderer.Services;

public static class FileUtilities
{
    public static string GetUniqueFilePath(string path)
    {
        if (!File.Exists(path))
        {
            return path;
        }

        var directory = Path.GetDirectoryName(path) ?? string.Empty;
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
        var extension = Path.GetExtension(path);
        var counter = 1;

        string candidate;
        do
        {
            candidate = Path.Combine(directory, $"{fileNameWithoutExtension}({counter}){extension}");
            counter++;
        } while (File.Exists(candidate));

        return candidate;
    }

    public static string GetUniqueDirectoryPath(string root, string directoryName)
    {
        var destinationPath = Path.Combine(root, directoryName);
        if (!Directory.Exists(destinationPath) && !File.Exists(destinationPath))
        {
            return destinationPath;
        }

        var counter = 1;
        string candidate;
        do
        {
            candidate = Path.Combine(root, $"{directoryName}({counter})");
            counter++;
        } while (Directory.Exists(candidate) || File.Exists(candidate));

        return candidate;
    }

    public static string MoveFile(string sourcePath, string destinationDirectory)
    {
        Directory.CreateDirectory(destinationDirectory);
        var destinationPath = Path.Combine(destinationDirectory, Path.GetFileName(sourcePath));
        destinationPath = GetUniqueFilePath(destinationPath);
        File.Move(sourcePath, destinationPath);
        return destinationPath;
    }
}
