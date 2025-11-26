namespace UnsortedOrderer.Contracts.Services;

public interface IMessengerPathService
{
    string? GetMessengerFolder(string filePath);
}
