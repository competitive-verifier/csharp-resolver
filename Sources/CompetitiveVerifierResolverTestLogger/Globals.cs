global using static CompetitiveVerifierResolverTestLogger.Globals;
using System;

namespace CompetitiveVerifierResolverTestLogger;

internal static class Globals
{
    public static void WriteWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($"Warning: {message}");
        Console.ResetColor();
        if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") is not null)
        {
            Console.WriteLine($"::warning ::{message}");
        }
    }
    public static void WriteError(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine($"error: {message}");
        Console.ResetColor();
        if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") is not null)
        {
            Console.WriteLine($"::error ::{message}");
        }
    }
}