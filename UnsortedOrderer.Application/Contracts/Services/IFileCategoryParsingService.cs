using UnsortedOrderer.Contracts.Categories;

namespace UnsortedOrderer.Contracts.Services;

public interface IFileCategoryParsingService
{
    bool IsFileOfCategory<TCategory>(string filePath)
        where TCategory : ICategory;

    bool IsFolderOfCategory<TCategory>(string folderPath)
        where TCategory : ICategory;
}
