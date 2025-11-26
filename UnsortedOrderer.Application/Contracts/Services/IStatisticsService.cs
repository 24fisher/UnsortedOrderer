using UnsortedOrderer.Contracts.Categories;

namespace UnsortedOrderer.Contracts.Services;

public interface IStatisticsService
{
    void RecordMovedFile(string destinationPath, string category);
    void RecordMovedFiles(int count, string category);
    void RecordMovedNonSplittableDirectory(INonSplittableDirectoryCategory category, string destinationPath, int fileCount);
    void RecordDeletedDirectory(string directory);
    void RecordUnknownFile(string extension);
    void RecordDeletedFile(string extension);
    void PrintStatistics(string sourceDirectory);
}
