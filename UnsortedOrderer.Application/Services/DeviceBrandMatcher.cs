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

        foreach (var pattern in patterns)
        {
            if (pattern.Pattern.IsMatch(fileName))
            {
                return pattern.Brand;
            }
        }

        return null;
    }
}
