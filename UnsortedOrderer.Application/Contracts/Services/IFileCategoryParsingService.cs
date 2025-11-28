using UnsortedOrderer.Contracts.Categories;

namespace UnsortedOrderer.Contracts.Services;

public interface IFileCategoryParsingService
{
    bool IsFileOfCategory<TCategory>(string filePath)
        where TCategory : IFileCategory;

    bool IsFolderOfCategory<TCategory>(string folderPath)
        where TCategory : IFileCategory;
}
