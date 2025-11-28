namespace UnsortedOrderer.Categories;

public abstract class ImageCategoryBase : FileCategory
{
    internal static readonly string[] ImageExtensions =
    [
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".tif", ".raw", ".cr2", ".cr3", ".nef", ".arw", ".orf", ".sr2",
        ".dng", ".rw2", ".pef", ".raf", ".srw", ".k25", ".webp", ".heic"
    ];

    protected ImageCategoryBase(string name, string folderName)
        : base(name, folderName, ImageExtensions)
    {
    }
}
