namespace UnsortedOrderer.Categories;

public sealed class MetadataCategory : FileCategory
{
    private static readonly string[] MetadataExtensions =
    [
        ".hprj", ".xmp", ".dop", ".pp3", ".on1", ".lrtemplate", ".acr", ".drx", ".rmd", ".sidecar"
    ];

    public MetadataCategory(string folderName)
        : base("Metadata", folderName, MetadataExtensions)
    {
    }
}
