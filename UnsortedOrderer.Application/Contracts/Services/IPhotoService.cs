namespace UnsortedOrderer.Contracts.Services;

public interface IPhotoService
{
    bool IsPhoto(string filePath);

    string MovePhoto(string filePath, string destinationRoot, string photosFolderName);
}
