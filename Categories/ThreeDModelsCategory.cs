namespace UnsortedOrderer.Categories;

public sealed class ThreeDModelsCategory : FileCategory
{
    private static readonly string[] ModelExtensions =
    [
        ".obj", ".fbx", ".stl", ".3ds", ".dae", ".ply"
    ];

    public ThreeDModelsCategory()
        : base("3DModels", "3DModels", ModelExtensions)
    {
    }
}
