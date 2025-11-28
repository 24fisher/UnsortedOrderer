using UnsortedOrderer.Application.Contracts.Services.Categories.Photo;

namespace UnsortedOrderer.Tests;

public partial class FileOrganizerServiceTests
{
    private sealed class StubPhotoService : IPhotoService
    {
        public StubPhotoService(bool isPhoto)
        {
            _isPhoto = isPhoto;
        }

        private readonly bool _isPhoto;

        public bool IsPhoto(string filePath) => _isPhoto;

        public string MovePhoto(string filePath, string destinationRoot, string photosFolderName)
        {
            throw new NotSupportedException();
        }

        public string MovePhoto(string filePath, string destinationRoot, string photosFolderName, string? messengerFolderName = null)
        {
            throw new NotImplementedException();
        }
    }
}
