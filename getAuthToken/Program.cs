using System.Net;
using System.Text;
using VRChatTools.Common;

const string TokenOutputPath = "auth_token.txt";
const string UserAgent = "VRCAvatarSwitcher/1.0 your@email.com";

Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine("VRChat Auth Token Cookie Getter");
Console.WriteLine($"User-Agent: {UserAgent}");

var username = ConsoleInput.ReadRequired("Username: ");
var password = ConsoleInput.ReadPassword("Password: ");

var cookieContainer = new CookieContainer();
using var http = VrChatApiClient.CreateWithCookieContainer(cookieContainer, out var baseUri, UserAgent);
var api = new VrChatApiClient(http);

var currentUserResponse = await api.GetCurrentUserWithBasicAuthAsync(username, password);
var currentUserJson = await currentUserResponse.Content.ReadAsStringAsync();

if (!currentUserResponse.IsSuccessStatusCode)
{
    VrChatApiClient.PrintFailure("Login failed.", currentUserResponse, currentUserJson);
    return;
}

if (VrChatApiClient.RequiresTwoFactorAuth(currentUserJson, out var twoFactorMethods))
{
    Console.WriteLine($"2FA is required: {string.Join(", ", twoFactorMethods)}");
    var twoFactorMethod = ChooseTwoFactorMethod(twoFactorMethods);
    var code = ConsoleInput.ReadRequired("2FA Code: ");

    var verifyResponse = await api.VerifyTwoFactorAsync(twoFactorMethod, code);
    var verifyJson = await verifyResponse.Content.ReadAsStringAsync();

    if (!verifyResponse.IsSuccessStatusCode || !VrChatApiClient.IsTwoFactorVerified(verifyJson))
    {
        VrChatApiClient.PrintFailure("2FA verification failed.", verifyResponse, verifyJson);
        return;
    }

    currentUserResponse = await http.GetAsync("auth/user");
    currentUserJson = await currentUserResponse.Content.ReadAsStringAsync();

    if (!currentUserResponse.IsSuccessStatusCode || VrChatApiClient.RequiresTwoFactorAuth(currentUserJson, out _))
    {
        VrChatApiClient.PrintFailure("Failed to get current user after 2FA.", currentUserResponse, currentUserJson);
        return;
    }
}

var authCookie = VrChatApiClient.GetAuthCookie(cookieContainer, baseUri);
if (authCookie is null)
{
    Console.WriteLine("The auth cookie was not found in the response.");
    Console.WriteLine("VRChat only sends the auth cookie once when creating a login session.");
    return;
}

await File.WriteAllTextAsync(TokenOutputPath, authCookie.Value + Environment.NewLine, Encoding.UTF8);

Console.WriteLine();
Console.WriteLine("Login succeeded.");
Console.WriteLine($"auth cookie: {authCookie.Value}");
Console.WriteLine($"Saved to: {Path.GetFullPath(TokenOutputPath)}");

static string ChooseTwoFactorMethod(string[] methods)
{
    if (methods.Length == 1)
    {
        return methods[0];
    }

    Console.WriteLine("Choose a 2FA method.");
    for (var i = 0; i < methods.Length; i++)
    {
        Console.WriteLine($"{i + 1}: {methods[i]}");
    }

    while (true)
    {
        Console.Write("Number: ");
        var input = Console.ReadLine();
        if (int.TryParse(input, out var index) && index >= 1 && index <= methods.Length)
        {
            return methods[index - 1];
        }
    }
}
