namespace UnsortedOrderer.Categories;

public sealed class WebCategory : FileCategory
{
    private static readonly string[] WebExtensions =
    [
        ".html",
        ".htm",
        ".xhtml",
        ".mhtml",
        ".mht",
        ".url",
        ".webloc",
        ".website"
    ];

    public WebCategory(string folderName)
        : base("Web", folderName, WebExtensions)
    {
    }
}
