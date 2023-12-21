using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;


namespace CompetitiveVerifierCsResolver.Resolve;
internal class TypeFinder : CSharpSyntaxWalker
{
    private readonly SemanticModel model;
    private readonly CancellationToken cancellationToken;

    private readonly ImmutableHashSet<string>.Builder definedTypesBuilder = ImmutableHashSet.CreateBuilder<string>();
    private readonly ImmutableHashSet<string>.Builder usedFilesBuilder = ImmutableHashSet.CreateBuilder<string>();
    public ImmutableHashSet<string> UsedFiles => usedFilesBuilder.ToImmutable();
    public ImmutableHashSet<string> DefinedTypeNames => definedTypesBuilder.ToImmutable();

    public TypeFinder(SemanticModel model, CancellationToken cancellationToken)
    {
        this.model = model;
        this.cancellationToken = cancellationToken;
    }

    /// <summary>
    /// Find Type or skip
    /// </summary>
    /// <returns>if <see langword="false"/>, ignore <paramref name="node"/></returns>
    private bool FindDeclaredType(MemberDeclarationSyntax node)
    {
        if (model.GetDeclaredSymbol(node, cancellationToken) is not ITypeSymbol symbol)
            return false;

        var typeName = symbol?.ToDisplayString();
        if (typeName is not null)
        {
            definedTypesBuilder.Add(typeName);
            return true;
        }
        return false;
    }

    public override void Visit(SyntaxNode? node)
    {
        if (node is null or UsingDirectiveSyntax)
            return;

        if (node is BaseTypeDeclarationSyntax typeDeclaration)
        {
            FindDeclaredType(typeDeclaration);
        }
        else if (model.GetSymbolInfo(node, cancellationToken).Symbol is { } symbol and not INamespaceSymbol)
        {
            usedFilesBuilder.UnionWith(symbol.Locations
                    .Where(l => l.Kind == LocationKind.SourceFile)
                    .Select(l => l.SourceTree?.FilePath)
                    .OfType<string>());
        }

        base.Visit(node);
    }
}

