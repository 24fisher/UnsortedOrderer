namespace UnsortedOrderer.Categories;

public sealed class FirmwareCategory : FileCategory
{
    private static readonly string[] FirmwareExtensions =
    [
        ".bin",
        ".dat"
    ];

    public FirmwareCategory(string folderName)
        : base("Firmware", folderName, FirmwareExtensions)
    {
    }
}
