using System;
using System.IO;

namespace CompetitiveVerifierResolverTestLogger;
internal class TeeStream : IDisposable
{
    private readonly StreamWriter? sw;

    public TeeStream(StreamWriter? sw)
    {
        this.sw = sw;
    }

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
