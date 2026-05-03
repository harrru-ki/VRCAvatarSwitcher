using System.Text;

namespace VRChatTools.Common;

public static class ConsoleInput
{
    public static string ReadRequired(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var value = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }
    }

    public static string ReadPassword(string prompt)
    {
        Console.Write(prompt);
        var password = new StringBuilder();

        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                return password.ToString();
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0)
                {
                    password.Length--;
                    Console.Write("\b \b");
                }

                continue;
            }

            password.Append(key.KeyChar);
            Console.Write('*');
        }
    }
}
