using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace WebApi.Services;

public class ImageService(IWebHostEnvironment environment)
{
    private readonly IWebHostEnvironment _environment = environment;

    public string CreateImageUrl(IFormFile image)
    {
        if (image == null || image.Length == 0) return null!;

        var directoryPath = Path.Combine(_environment.WebRootPath, "Images");

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        using var sha256 = SHA256.Create();
        using var stream = image.OpenReadStream();
        var hash = sha256.ComputeHash(stream);
        var hashString = BitConverter.ToString(hash).Replace("-", "").ToLower();

        var fileName = $"{hashString}{Path.GetExtension(image.FileName)}";
        var filePath = Path.Combine(directoryPath, fileName);

        if(File.Exists(filePath))
            return $"/Images/{fileName}";

        stream.Position = 0;
        using var fileStream = new FileStream(filePath, FileMode.Create);
        stream.CopyTo(fileStream);

        return $"/Images/{fileName}";
    }
}
