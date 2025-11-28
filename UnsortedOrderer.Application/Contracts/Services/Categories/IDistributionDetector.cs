namespace UnsortedOrderer.Application.Contracts.Services.Categories;

public interface IDistributionDetector
{
    bool IsDistributionDirectory(string path);
}
