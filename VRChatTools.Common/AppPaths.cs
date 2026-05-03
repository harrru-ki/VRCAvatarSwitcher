namespace VRChatTools.Common;

public static class AppPaths
{
    private static readonly string AppDirectory = AppContext.BaseDirectory;

    public static readonly string AuthTokenPath = Path.Combine(AppDirectory, "auth_token.txt");
    public static readonly string OwnAvatarsPath = Path.Combine(AppDirectory, "own_avatars.txt");
    public static readonly string UnusedAvatarsPath = Path.Combine(AppDirectory, "unused_avatars.txt");
    public static readonly string WorldAvatarMapPath = Path.Combine(AppDirectory, "world_avatar_map.txt");
}
