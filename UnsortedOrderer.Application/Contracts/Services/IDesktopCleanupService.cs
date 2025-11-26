namespace UnsortedOrderer.Contracts.Services;

public interface IDesktopCleanupService
{
    void CleanIfRunningFromDesktop(string destinationPath);
}
