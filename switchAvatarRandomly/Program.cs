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

var unusedAvatarIds = await AvatarFileStore.LoadUnusedAvatarIdsAsync(AppPaths.UnusedAvatarsPath);
var selectableAvatars = AvatarFileStore.ExcludeUnusedAvatars(avatars, unusedAvatarIds);
if (selectableAvatars.Count == 0)
{
    Console.WriteLine("No selectable avatars remain after excluding unused avatars.");
    Console.WriteLine($"All avatars: {avatars.Count}");
    Console.WriteLine($"Excluded avatars: {unusedAvatarIds.Count}");
    return;
}

var selectedAvatar = AvatarFileStore.PickRandom(selectableAvatars);

using var http = VrChatApiClient.CreateWithAuthCookie(authToken);
var api = new VrChatApiClient(http);

Console.WriteLine($"Loaded avatars: {avatars.Count}");
Console.WriteLine($"Excluded avatars: {avatars.Count - selectableAvatars.Count}");
Console.WriteLine($"Selectable avatars: {selectableAvatars.Count}");
Console.WriteLine($"Randomly selected: {selectedAvatar.Name} ({selectedAvatar.Id})");

var succeeded = await api.SelectAvatarAsync(selectedAvatar);
Console.WriteLine(succeeded
    ? $"Selected avatar: {selectedAvatar.Name}"
    : "Avatar switch failed.");
