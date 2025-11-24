namespace UnsortedOrderer.Categories;

public sealed class SoftCategory : FileCategory
{
    internal static readonly string[] SoftExtensions =
    [
        ".exe", ".msi", ".msix", ".apk", ".dmg", ".pkg", ".deb", ".rpm", ".appimage", ".iso"
    ];

    public SoftCategory(string folderName)
        : base("Soft", folderName, SoftExtensions)
    {
    }
}
