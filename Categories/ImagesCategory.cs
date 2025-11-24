namespace UnsortedOrderer.Categories;

public sealed class ImagesCategory : FileCategory
{
    private static readonly string[] ImageExtensions =
    [
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".tif", ".raw", ".cr2", ".nef", ".arw", ".orf", ".sr2"
    ];

    public ImagesCategory(string folderName)
        : base("Images", folderName, ImageExtensions)
    {
    }
}
