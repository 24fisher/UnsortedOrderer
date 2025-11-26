using System.Collections.Generic;
using UnsortedOrderer.Contracts.Services;

using Path = System.IO.Path;

namespace UnsortedOrderer.Services;

public sealed class MessengerPathService : IMessengerPathService
{
    private static readonly IReadOnlyDictionary<string, string> MessengerFolders = new Dictionary<string, string>(
        StringComparer.OrdinalIgnoreCase)
    {
        { "telegram", "_Telegram" },
        { "watsapp", "_Watsapp" },
        { "whatsapp", "_Watsapp" }
    };

    public string? GetMessengerFolder(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);

        while (!string.IsNullOrWhiteSpace(directory))
        {
            var folderName = Path.GetFileName(directory);
            if (!string.IsNullOrWhiteSpace(folderName))
            {
                foreach (var messengerName in MessengerFolders.Keys)
                {
                    if (folderName.Contains(messengerName, StringComparison.OrdinalIgnoreCase))
                    {
                        return MessengerFolders[messengerName];
                    }
                }
            }

            directory = Path.GetDirectoryName(directory);
        }

        return null;
    }
}
