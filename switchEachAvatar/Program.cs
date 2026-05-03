using System.Text;
using VRChatTools.Common;

Console.OutputEncoding = Encoding.UTF8;

if (!File.Exists(AppPaths.OwnAvatarsPath))
{
    Console.WriteLine($"avatar list file was not found: {AppPaths.OwnAvatarsPath}");
    return;
}

var authToken = await AuthTokenStore.TryReadAuthTokenAsync(AppPaths.AuthTokenPath);
if (authToken is null)
{
    return;
}

var avatars = await AvatarFileStore.LoadAvatarsAsync(AppPaths.OwnAvatarsPath);
if (avatars.Count == 0)
{
    Console.WriteLine($"avatar list is empty: {AppPaths.OwnAvatarsPath}");
    return;
}

using var http = VrChatApiClient.CreateWithAuthCookie(authToken);
var api = new VrChatApiClient(http);

Console.WriteLine($"Loaded avatars: {avatars.Count}");
Console.WriteLine("Press Enter to switch to the next avatar. Press Q then Enter to quit.");
Console.WriteLine();

for (var index = 0; ; index = (index + 1) % avatars.Count)
{
    var avatar = avatars[index];
    Console.Write($"Next: {avatar.Name} ({avatar.Id}) > ");

    var input = Console.ReadLine();
    if (string.Equals(input, "q", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    var succeeded = await api.SelectAvatarAsync(avatar);
    if (succeeded)
    {
        Console.WriteLine($"Selected: {avatar.Name}");
    }
}
