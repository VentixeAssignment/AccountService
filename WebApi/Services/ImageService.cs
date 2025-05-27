using Microsoft.IdentityModel.Tokens;

namespace WebApi.Services;

public class ImageService
{
    public string CreateImageUrl(IFormFile image)
    {
        if (image == null) return null!;

        var directoryPath = "/Images";

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var imagePath = Path.Combine(directoryPath, $"{Guid.NewGuid()}_{image.Name}");

        return imagePath;
    }
}
