using System;
using System.IO;
using System.Reflection;
using UnsortedOrderer.Services;
using Xunit;

namespace UnsortedOrderer.Tests;

public class DesktopCleanupServiceTests
{
    [Theory]
    [InlineData(".lnk")]
    [InlineData(".url")]
    public void IsShortcut_ReturnsTrue_ForKnownExtensions(string extension)
    {
        var isShortcutMethod = typeof(DesktopCleanupService)
            .GetMethod("IsShortcut", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("IsShortcut method not found.");

        var filePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + extension);
        File.WriteAllText(filePath, string.Empty);

        try
        {
            var result = (bool)isShortcutMethod.Invoke(null, new object[] { new FileInfo(filePath) })!;

            Assert.True(result);
        }
        finally
        {
            File.Delete(filePath);
        }
    }
}
