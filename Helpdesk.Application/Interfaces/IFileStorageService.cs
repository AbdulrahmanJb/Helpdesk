namespace Helpdesk.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveAsync(Stream content, string extension);

    Task<byte[]?> ReadAsync(string storedFileName);

    Task DeleteAsync(string storedFileName);
}
