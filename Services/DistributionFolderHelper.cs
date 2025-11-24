using System.Text.RegularExpressions;

namespace UnsortedOrderer.Services;

public static class DistributionFolderHelper
{
    private static readonly Regex VersionSuffixRegex = new(
        "^(?<name>.+?)(?:[\\s._-]*v?(?:\\d+)(?:[\\._-]\\d+)*)(?:[\\s._-]*(setup|installer|x86|x64|win\\d+)?)?$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static string? TryGetProgramFolderName(string filePath)
    {
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath) ?? string.Empty;
        var match = VersionSuffixRegex.Match(fileNameWithoutExtension);
        if (!match.Success)
        {
            return null;
        }

        var programName = match.Groups["name"].Value.Trim().Trim('-', '_', '.', ' ');
        return string.IsNullOrWhiteSpace(programName) ? null : programName;
    }
}
