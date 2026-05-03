using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace VRChatTools.Common;

public sealed class VrChatApiClient(HttpClient http)
{
    public const string BaseUrl = "https://api.vrchat.cloud/api/1/";
    public const string DefaultUserAgent = "VRCAvatarSwitcher/1.0 your@email.com";

    public static HttpClient CreateWithAuthCookie(string authToken, string userAgent = DefaultUserAgent)
    {
        var http = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl)
        };

        ConfigureDefaultHeaders(http, userAgent);
        http.DefaultRequestHeaders.TryAddWithoutValidation("Cookie", $"auth={authToken}");
        return http;
    }

    public static HttpClient CreateWithCookieContainer(CookieContainer cookieContainer, out Uri baseUri, string? userAgent = null)
    {
        baseUri = new Uri(BaseUrl);
        var handler = new HttpClientHandler
        {
            CookieContainer = cookieContainer,
            UseCookies = true
        };

        var http = new HttpClient(handler)
        {
            BaseAddress = baseUri
        };

        ConfigureDefaultHeaders(http, userAgent ?? DefaultUserAgent);
        return http;
    }

    public static void ConfigureDefaultHeaders(HttpClient http, string userAgent)
    {
        http.DefaultRequestHeaders.Accept.Clear();
        http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        http.DefaultRequestHeaders.UserAgent.Clear();
        http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", userAgent);
    }

    public async Task<JsonElement?> GetJsonAsync(string requestUri)
    {
        using var response = await http.GetAsync(requestUri);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            PrintFailure($"Request failed: GET {requestUri}", response, body, prettyJson: true);
            return null;
        }

        using var document = JsonDocument.Parse(body);
        return document.RootElement.Clone();
    }

    public async Task<HttpResponseMessage> GetCurrentUserWithBasicAuthAsync(string username, string password)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "auth/user");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", CreateBasicToken(username, password));
        return await http.SendAsync(request);
    }

    public async Task<JsonElement?> GetCurrentUserAsync()
    {
        return await GetJsonAsync("auth/user");
    }

    public async Task<List<JsonElement>> GetOwnAvatarsAsync(string userId, int pageSize = 100)
    {
        var avatars = new List<JsonElement>();

        for (var offset = 0; ; offset += pageSize)
        {
            var url = "avatars"
                + $"?userId={WebUtility.UrlEncode(userId)}"
                + "&releaseStatus=all"
                + $"&n={pageSize}"
                + $"&offset={offset}"
                + "&sort=updated"
                + "&order=descending";

            var json = await GetJsonAsync(url);
            if (json is null)
            {
                break;
            }

            if (json.Value.ValueKind != JsonValueKind.Array)
            {
                Console.WriteLine("Unexpected avatar response. Expected a JSON array.");
                break;
            }

            var page = json.Value.EnumerateArray()
                .Select(avatar => avatar.Clone())
                .ToList();

            avatars.AddRange(page);

            if (page.Count < pageSize)
            {
                break;
            }
        }

        return avatars;
    }

    public async Task<bool> SelectAvatarAsync(Avatar avatar)
    {
        using var response = await http.PutAsync($"avatars/{Uri.EscapeDataString(avatar.Id)}/select", content: null);
        var body = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return true;
        }

        PrintFailure($"Failed to select avatar: {avatar.Name} ({avatar.Id})", response, body, prettyJson: true);
        return false;
    }

    public async Task<bool> LogoutAsync()
    {
        using var response = await http.PutAsync("logout", content: null);
        var body = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return true;
        }

        PrintFailure("Logout failed.", response, body, prettyJson: true);
        return false;
    }

    public async Task<HttpResponseMessage> VerifyTwoFactorAsync(string method, string code)
    {
        var endpoint = method.ToLowerInvariant() switch
        {
            "emailotp" or "email" => "auth/twofactorauth/emailotp/verify",
            "otp" or "recovery" => "auth/twofactorauth/otp/verify",
            _ => "auth/twofactorauth/totp/verify"
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(CreateCodePayload(code), Encoding.UTF8, "application/json")
        };

        return await http.SendAsync(request);
    }

    private static string CreateCodePayload(string code)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("code", code);
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    public static string? GetWorldId(JsonElement user)
    {
        var worldId = JsonHelpers.GetOptionalStringProperty(user, "worldId");
        if (IsWorldId(worldId))
        {
            return worldId;
        }

        var location = JsonHelpers.GetOptionalStringProperty(user, "location");
        if (!string.IsNullOrWhiteSpace(location))
        {
            var match = Regex.Match(location, @"wrld_[0-9a-fA-F-]+");
            if (match.Success)
            {
                return match.Value;
            }
        }

        var travelingToWorld = JsonHelpers.GetOptionalStringProperty(user, "travelingToWorld");
        return IsWorldId(travelingToWorld) ? travelingToWorld : null;
    }

    public static bool RequiresTwoFactorAuth(string json, out string[] methods)
    {
        methods = [];

        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("requiresTwoFactorAuth", out var property))
        {
            return false;
        }

        methods = property.ValueKind switch
        {
            JsonValueKind.Array => property.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.String)
                .Select(item => item.GetString())
                .Where(method => !string.IsNullOrWhiteSpace(method))
                .Select(method => method!)
                .ToArray(),
            JsonValueKind.String => [property.GetString() ?? string.Empty],
            _ => []
        };

        return methods.Length > 0;
    }

    public static bool IsTwoFactorVerified(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.TryGetProperty("verified", out var verified)
            && verified.ValueKind == JsonValueKind.True;
    }

    public static Cookie? GetAuthCookie(CookieContainer cookies, Uri baseUri)
    {
        return cookies.GetCookies(baseUri)
            .Cast<Cookie>()
            .FirstOrDefault(cookie => cookie.Name == "auth");
    }

    public static void PrintFailure(string title, HttpResponseMessage response, string responseBody, bool prettyJson = false)
    {
        Console.WriteLine(title);
        Console.WriteLine($"Status: {(int)response.StatusCode} {response.ReasonPhrase}");

        if (!string.IsNullOrWhiteSpace(responseBody))
        {
            Console.WriteLine(prettyJson
                ? JsonHelpers.FormatJsonIfPossible(responseBody)
                : responseBody);
        }
    }

    private static string CreateBasicToken(string username, string password)
    {
        var encodedUsername = Uri.EscapeDataString(username);
        var encodedPassword = Uri.EscapeDataString(password);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{encodedUsername}:{encodedPassword}"));
    }

    private static bool IsWorldId(string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && value.StartsWith("wrld_", StringComparison.Ordinal);
    }
}
