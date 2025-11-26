using UnsortedOrderer.Services;

namespace UnsortedOrderer.Services;

public sealed class ConsoleMessageWriter : IMessageWriter
{
    public void WriteLine(string message)
    {
        Console.WriteLine(message);
    }
}
