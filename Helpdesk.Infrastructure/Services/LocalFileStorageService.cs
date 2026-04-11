using Helpdesk.Application.Interfaces;

namespace Helpdesk.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _uploadsDirectory;

    public LocalFileStorageService()
    {
        _uploadsDirectory = Path.Combine(AppContext.BaseDirectory, "Uploads");
        Directory.CreateDirectory(_uploadsDirectory);
    }

    public async Task<string> SaveAsync(Stream content, string extension)
    {
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(_uploadsDirectory, storedFileName);

        await using var fileStream = File.Create(fullPath);
        await content.CopyToAsync(fileStream);

        return storedFileName;
    }

    public async Task<byte[]?> ReadAsync(string storedFileName)
    {
        var fullPath = Path.Combine(_uploadsDirectory, storedFileName);

        if (!File.Exists(fullPath))
            return null;

        return await File.ReadAllBytesAsync(fullPath);
    }

    public Task DeleteAsync(string storedFileName)
    {
        var fullPath = Path.Combine(_uploadsDirectory, storedFileName);

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }
}
