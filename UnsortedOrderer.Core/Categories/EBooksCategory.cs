namespace UnsortedOrderer.Categories;

public sealed class EBooksCategory : FileCategory
{
    private static readonly string[] EBookExtensions =
    [
        ".epub",
        ".fb2",
        ".mobi",
        ".azw3",
        ".djvu",
        ".cbr",
        ".cbz",
        ".ibooks",
        ".kfx"
    ];

    public EBooksCategory(string folderName)
        : base("E-Books", folderName, EBookExtensions)
    {
    }
}
