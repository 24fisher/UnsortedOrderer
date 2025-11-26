namespace UnsortedOrderer.Categories;

public sealed class DocumentsCategory : FileCategory
{
    private static readonly string[] DocumentExtensions =
    [
        ".pdf",
        ".doc",
        ".docx",
        ".txt",
        ".rtf",
        ".odt",
        ".xlsx",
        ".xls",
        ".csv",
        ".ppt",
        ".pptx",
        ".ipynb",
        ".names"
    ];

    public DocumentsCategory()
        : base("Documents", "Documents", DocumentExtensions)
    {
    }
}
