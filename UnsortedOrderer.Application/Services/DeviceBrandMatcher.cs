using System.IO;
using UnsortedOrderer.Models;

namespace UnsortedOrderer.Services;

public static class DeviceBrandMatcher
{
    public static string? GetBrandByFileName(string? fileName, IEnumerable<DeviceBrandPattern> patterns)
    {
        if (string.IsNullOrWhiteSpace(fileName) || patterns is null)
        {
            return null;
        }

        var fileNameWithoutPath = Path.GetFileName(fileName);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileNameWithoutPath);

        foreach (var pattern in patterns)
        {
            if (pattern.Pattern.IsMatch(fileNameWithoutPath) || pattern.Pattern.IsMatch(fileNameWithoutExtension))
            {
                return pattern.Brand;
            }
        }

        return null;
    }
}
