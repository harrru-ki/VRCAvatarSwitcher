using System.Text;

namespace VRChatTools.Common;

public static class AuthTokenStore
{
    public static async Task<string?> TryReadAuthTokenAsync(string path)
    {
        if (!File.Exists(path))
        {
            Console.WriteLine($"auth token file was not found: {path}");
            return null;
        }

        var authToken = (await File.ReadAllTextAsync(path, Encoding.UTF8)).Trim();
        if (string.IsNullOrWhiteSpace(authToken))
        {
            Console.WriteLine($"auth token file is empty: {path}");
            return null;
        }

        return authToken;
    }
}
