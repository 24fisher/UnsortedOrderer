using System.Drawing;

namespace UnsortedOrderer.Services;

public interface IPhotoService
{
    string MovePhoto(string filePath, string destinationRoot, string imagesFolderName);
}

public sealed class PhotoService : IPhotoService
{
    private const int DateTakenId = 0x9003; // PropertyTagExifDTOrig

    public string MovePhoto(string filePath, string destinationRoot, string imagesFolderName)
    {
        var year = GetPhotoYear(filePath);
        var photoDirectory = Path.Combine(destinationRoot, imagesFolderName, year.ToString());
        return FileUtilities.MoveFile(filePath, photoDirectory);
    }

    private static int GetPhotoYear(string filePath)
    {
        try
        {
            using var image = Image.FromFile(filePath);
            var propertyItem = image.PropertyItems.FirstOrDefault(p => p.Id == DateTakenId);
            if (propertyItem is not null)
            {
                var dateString = System.Text.Encoding.ASCII.GetString(propertyItem.Value).Trim('\0');
                if (DateTime.TryParse(dateString, out var dateTaken))
                {
                    return dateTaken.Year;
                }
            }
        }
        catch
        {
            // fall back to file timestamps
        }

        return File.GetCreationTime(filePath).Year;
    }
}
