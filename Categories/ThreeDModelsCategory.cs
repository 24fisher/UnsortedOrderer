namespace UnsortedOrderer.Categories;

public sealed class ThreeDModelsCategory : FileCategory
{
    private static readonly string[] ModelExtensions =
    [
        ".obj",
        ".fbx",
        ".stl",
        ".3ds",
        ".dae",
        ".ply",
        ".blend",
        ".gltf",
        ".glb",
        ".usdz",
        ".step",
        ".stp",
        ".iges",
        ".igs",
        ".3mf",
        ".gcode",
        ".scad"
    ];

    public ThreeDModelsCategory()
        : base("3DModels", "3DModels", ModelExtensions)
    {
    }
}
