using System.Text;
using VRChatTools.Common;

const string OutputPath = "own_avatars.txt";

Console.OutputEncoding = Encoding.UTF8;

var authToken = await AuthTokenStore.TryReadAuthTokenAsync(AppPaths.AuthTokenPath);
if (authToken is null)
{
    return;
}

using var http = VrChatApiClient.CreateWithAuthCookie(authToken);
var api = new VrChatApiClient(http);

var currentUserJson = await api.GetCurrentUserAsync();
if (currentUserJson is null)
{
    return;
}

var currentUser = currentUserJson.Value;
var userId = JsonHelpers.GetRequiredStringProperty(currentUser, "id");
var displayName = JsonHelpers.GetOptionalStringProperty(currentUser, "displayName") ?? "(unknown)";

Console.WriteLine($"User: {displayName}");
Console.WriteLine($"User ID: {userId}");
Console.WriteLine();

var avatars = await api.GetOwnAvatarsAsync(userId);
if (avatars.Count == 0)
{
    Console.WriteLine("No avatars were returned.");
    return;
}

var lines = new List<string>();
foreach (var avatar in avatars)
{
    var id = JsonHelpers.GetOptionalStringProperty(avatar, "id") ?? "";
    var name = JsonHelpers.GetOptionalStringProperty(avatar, "name") ?? "(no name)";
    var releaseStatus = JsonHelpers.GetOptionalStringProperty(avatar, "releaseStatus") ?? "(unknown)";

    Console.WriteLine($"{id}, {name} [{releaseStatus}]");
    lines.Add($"{id},{name}");
}

await File.WriteAllLinesAsync(OutputPath, lines, Encoding.UTF8);

Console.WriteLine();
Console.WriteLine($"Avatar count: {avatars.Count}");
Console.WriteLine($"Saved to: {Path.GetFullPath(OutputPath)}");
