namespace UnsortedOrderer.Application.Contracts.Services.Categories.Photo;

public interface IPhotoCameraMetadataService
{
    string? GetCameraFolder(string filePath);
}
