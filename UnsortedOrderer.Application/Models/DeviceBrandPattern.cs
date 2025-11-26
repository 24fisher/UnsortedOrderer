using System.Text.RegularExpressions;

namespace UnsortedOrderer.Models;

public sealed class DeviceBrandPattern
{
    public DeviceBrandPattern(string brand, Regex pattern)
    {
        Brand = brand;
        Pattern = pattern;
    }

    public string Brand { get; }

    public Regex Pattern { get; }
}
