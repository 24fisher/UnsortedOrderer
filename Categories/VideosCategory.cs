namespace UnsortedOrderer.Categories;

public sealed class VideosCategory : FileCategory
{
    private static readonly string[] VideoExtensions =
    [
        ".mp4", ".mov", ".avi", ".mkv", ".wmv", ".flv", ".webm", ".mpeg"
    ];

    public VideosCategory()
        : base("Videos", "Videos", VideoExtensions)
    {
    }
}
