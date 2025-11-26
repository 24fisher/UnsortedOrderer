using System.Text.RegularExpressions;
using UnsortedOrderer.Models;
using UnsortedOrderer.Services;
using Xunit;

namespace UnsortedOrderer.Tests;

public class DeviceBrandMatcherTests
{
    [Fact]
    public void GetBrandByFileName_prefers_honor_date_pattern_over_other_matches()
    {
        var patterns = new[]
        {
            new DeviceBrandPattern("Honor", new Regex("^VID_\\d{8}_\\d{6}$", RegexOptions.IgnoreCase)),
            new DeviceBrandPattern("iPhone", new Regex("^(?:IMG|VID|RPReplay_Final)_?\\d+", RegexOptions.IgnoreCase)),
            new DeviceBrandPattern("Xiaomi", new Regex("^VID_\\d{8}_\\d{6}", RegexOptions.IgnoreCase))
        };

        var brand = DeviceBrandMatcher.GetBrandByFileName("VID_20250910_200444.mp4", patterns);

        Assert.Equal("Honor", brand);
    }

    [Fact]
    public void GetBrandByFileName_continues_matching_when_honor_date_pattern_is_not_applicable()
    {
        var patterns = new[]
        {
            new DeviceBrandPattern("Honor", new Regex("^VID_\\d{8}_\\d{6}$", RegexOptions.IgnoreCase)),
            new DeviceBrandPattern("iPhone", new Regex("^(?:IMG|VID|RPReplay_Final)_?\\d+", RegexOptions.IgnoreCase))
        };

        var brand = DeviceBrandMatcher.GetBrandByFileName("VID_0001.MOV", patterns);

        Assert.Equal("iPhone", brand);
    }
}
