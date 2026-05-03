using System.Text;

namespace VRChatTools.Common;

public static class AvatarFileStore
{
    public static async Task<List<Avatar>> LoadAvatarsAsync(string path)
    {
        var avatars = new List<Avatar>();

        foreach (var line in await File.ReadAllLinesAsync(path, Encoding.UTF8))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var items = line.Split(',', 2, StringSplitOptions.TrimEntries);
            var id = items[0];
            var name = items.Length > 1 && !string.IsNullOrWhiteSpace(items[1])
                ? items[1]
                : id;

            if (!id.StartsWith("avtr_", StringComparison.Ordinal))
            {
                Console.WriteLine($"Skipped invalid avatar line: {line}");
                continue;
            }

            avatars.Add(new Avatar(id, name));
        }

        return avatars;
    }

    public static async Task<HashSet<string>> LoadUnusedAvatarIdsAsync(string path)
    {
        var avatarIds = new HashSet<string>(StringComparer.Ordinal);

        if (!File.Exists(path))
        {
            Console.WriteLine($"unused avatar list file was not found, so no avatars will be excluded: {path}");
            return avatarIds;
        }

        foreach (var line in await File.ReadAllLinesAsync(path, Encoding.UTF8))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var items = line.Split(',', 2, StringSplitOptions.TrimEntries);
            var id = items[0];

            if (!id.StartsWith("avtr_", StringComparison.Ordinal))
            {
                Console.WriteLine($"Skipped invalid unused avatar line: {line}");
                continue;
            }

            avatarIds.Add(id);
        }

        return avatarIds;
    }

    public static async Task<Dictionary<string, string>> LoadWorldAvatarMapAsync(string path)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);

        if (!File.Exists(path))
        {
            return map;
        }

        foreach (var line in await File.ReadAllLinesAsync(path, Encoding.UTF8))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var items = line.Split(',', 2, StringSplitOptions.TrimEntries);
            if (items.Length != 2)
            {
                Console.WriteLine($"Skipped invalid world avatar map line: {line}");
                continue;
            }

            var worldId = items[0];
            var avatarId = items[1];

            if (!worldId.StartsWith("wrld_", StringComparison.Ordinal)
                || !avatarId.StartsWith("avtr_", StringComparison.Ordinal))
            {
                Console.WriteLine($"Skipped invalid world avatar map line: {line}");
                continue;
            }

            map[worldId] = avatarId;
        }

        return map;
    }

    public static List<Avatar> ExcludeUnusedAvatars(IEnumerable<Avatar> avatars, ISet<string> unusedAvatarIds)
    {
        return avatars
            .Where(avatar => !unusedAvatarIds.Contains(avatar.Id))
            .ToList();
    }

    public static Avatar PickRandom(IReadOnlyList<Avatar> avatars)
    {
        if (avatars.Count == 0)
        {
            throw new InvalidOperationException("No avatars are available.");
        }

        return avatars[Random.Shared.Next(avatars.Count)];
    }
}
