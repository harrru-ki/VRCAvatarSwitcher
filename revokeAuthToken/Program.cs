using System.Text;
using VRChatTools.Common;

Console.OutputEncoding = Encoding.UTF8;

var authToken = await AuthTokenStore.TryReadAuthTokenAsync(AppPaths.AuthTokenPath);
if (authToken is null)
{
    return;
}

using var http = VrChatApiClient.CreateWithAuthCookie(authToken);
var api = new VrChatApiClient(http);

Console.WriteLine("Revoking auth token...");

var succeeded = await api.LogoutAsync();
if (!succeeded)
{
    Console.WriteLine("Auth token was not revoked.");
    return;
}

Console.WriteLine("Logout succeeded. The auth token cookie is now invalid.");
