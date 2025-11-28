using UnsortedOrderer.Models;

namespace UnsortedOrderer.Application.Contracts.Services.Categories.Photo;

public interface IPhotoService
{
    bool IsPhoto(string filePath);

    string MovePhoto(
        string filePath,
        string destinationRoot,
        string photosFolderName,
        string? messengerFolderName = null);
}
