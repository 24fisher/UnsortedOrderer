namespace UnsortedOrderer.Categories;

public sealed class MusicCategory : FileCategory
{
    private static readonly string[] MusicExtensions =
    [
        ".mp3", ".wav", ".flac", ".aac", ".m4a", ".ogg", ".wma", ".aiff", ".alac", ".opus"
    ];

    public MusicCategory(string folderName)
        : base("Music", folderName, MusicExtensions)
    {
    }
}
