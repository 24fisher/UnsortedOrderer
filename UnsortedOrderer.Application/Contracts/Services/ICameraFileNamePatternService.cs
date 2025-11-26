namespace UnsortedOrderer.Contracts.Services;

public interface ICameraFileNamePatternService
{
    string? GetBrandByFileName(string? fileName);
}
