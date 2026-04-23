using System;
using System.IO;

namespace CompetitiveVerifierResolverTestLogger;

internal class TeeStream(StreamWriter? sw) : IDisposable
{
    public void WriteLine(string line)
    {
        Console.WriteLine(line);
        sw?.WriteLine(line);
    }

    public void Dispose()
    {
        sw?.Dispose();
    }
}
