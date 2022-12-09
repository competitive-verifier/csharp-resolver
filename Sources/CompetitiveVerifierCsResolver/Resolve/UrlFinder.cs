using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace CompetitiveVerifierCsResolver.Resolve;
internal partial class UrlFinder : CSharpSyntaxWalker
{
    private readonly CancellationToken cancellationToken;

    private readonly ImmutableHashSet<string>.Builder urlsBuilder = ImmutableHashSet.CreateBuilder<string>();
    public ImmutableHashSet<string> Urls => urlsBuilder.ToImmutable();

    public UrlFinder(CancellationToken cancellationToken) : base(SyntaxWalkerDepth.Trivia)
    {
        this.cancellationToken = cancellationToken;
    }

    public override void VisitTrivia(SyntaxTrivia trivia)
    {
        var text = trivia.SyntaxTree?.GetText(cancellationToken).ToString(trivia.Span);
        if (text is null) return;
        foreach (var url in EnumerateEmbeddedUrls(text))
        {
            urlsBuilder.Add(url);
        }
    }


    [GeneratedRegex(@"['""`]?https?://\S*")]
    private static partial Regex EmbeddedUrlsRegex();
    static IEnumerable<string> EnumerateEmbeddedUrls(string content)
    {
        var trimChars = new char[] { '\'', '"', '`' };
        var regex = EmbeddedUrlsRegex();
        foreach (var m in regex.Matches(content).AsEnumerable())
        {
            var url = m.ValueSpan.Trim(trimChars);
            yield return new string(url);
        }
    }
}
