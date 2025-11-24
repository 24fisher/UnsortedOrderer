namespace UnsortedOrderer.Categories;

public sealed class DriversCategory : FileCategory
{
    public DriversCategory(string folderName)
        : base("Drivers", folderName, SoftCategory.SoftExtensions)
    {
    }
}
