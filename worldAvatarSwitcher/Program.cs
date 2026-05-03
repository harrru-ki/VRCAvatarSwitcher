using System.Text;
using VRChatTools.Common;

const int PollIntervalSeconds = 30;
const int AvatarSwitchDelaySeconds = 10;

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

var worldAvatarMap = await AvatarFileStore.LoadWorldAvatarMapAsync(AppPaths.WorldAvatarMapPath);
var avatarsById = avatars.ToDictionary(avatar => avatar.Id, StringComparer.Ordinal);

using var http = VrChatApiClient.CreateWithAuthCookie(authToken);
var api = new VrChatApiClient(http);

var currentUser = await api.GetCurrentUserAsync();
if (currentUser is null)
{
    return;
}

var userId = JsonHelpers.GetRequiredStringProperty(currentUser.Value, "id");
var displayName = JsonHelpers.GetOptionalStringProperty(currentUser.Value, "displayName") ?? "(unknown)";

Console.WriteLine($"User: {displayName}");
Console.WriteLine($"User ID: {userId}");
Console.WriteLine($"Loaded avatars: {avatars.Count}");
Console.WriteLine($"Excluded avatars: {avatars.Count - selectableAvatars.Count}");
Console.WriteLine($"Selectable avatars: {selectableAvatars.Count}");
Console.WriteLine($"World avatar map entries: {worldAvatarMap.Count}");
Console.WriteLine($"Checking world every {PollIntervalSeconds} seconds. Press Ctrl+C to stop.");
Console.WriteLine();

string? previousWorldId = null;

while (true)
{
    try
    {
        var user = await api.GetJsonAsync($"users/{Uri.EscapeDataString(userId)}");
        if (user is not null)
        {
            var worldId = VrChatApiClient.GetWorldId(user.Value);
            var now = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss");

            if (string.IsNullOrWhiteSpace(worldId))
            {
                Console.WriteLine($"[{now}] world id was not available.");
            }
            else if (previousWorldId is null)
            {
                previousWorldId = worldId;
                Console.WriteLine($"[{now}] current world: {worldId}");
            }
            else if (!string.Equals(previousWorldId, worldId, StringComparison.Ordinal))
            {
                Console.WriteLine($"[{now}] world changed: {previousWorldId} -> {worldId}");
                previousWorldId = worldId;

                var avatar = SelectAvatarForWorld(worldId, worldAvatarMap, avatarsById, selectableAvatars);

                Console.WriteLine($"Waiting {AvatarSwitchDelaySeconds} seconds before switching avatar...");
                await Task.Delay(TimeSpan.FromSeconds(AvatarSwitchDelaySeconds));

                var succeeded = await api.SelectAvatarAsync(avatar);
                Console.WriteLine(succeeded
                    ? $"Selected avatar: {avatar.Name}"
                    : "Avatar switch failed.");
            }
            else
            {
                Console.WriteLine($"[{now}] world unchanged: {worldId}");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }

    await Task.Delay(TimeSpan.FromSeconds(PollIntervalSeconds));
}

static Avatar SelectAvatarForWorld(
    string worldId,
    IReadOnlyDictionary<string, string> worldAvatarMap,
    IReadOnlyDictionary<string, Avatar> avatarsById,
    IReadOnlyList<Avatar> selectableAvatars)
{
    if (worldAvatarMap.TryGetValue(worldId, out var mappedAvatarId))
    {
        if (avatarsById.TryGetValue(mappedAvatarId, out var mappedAvatar))
        {
            Console.WriteLine($"World map selected avatar: {mappedAvatar.Name} ({mappedAvatar.Id})");
            return mappedAvatar;
        }

        Console.WriteLine($"World map avatar was not found in own avatar list: {mappedAvatarId}");
    }

    var randomAvatar = AvatarFileStore.PickRandom(selectableAvatars);
    Console.WriteLine($"Randomly selected avatar: {randomAvatar.Name} ({randomAvatar.Id})");
    return randomAvatar;
}
