using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;


namespace CompetitiveVerifierCsResolver;
internal class TypeFinder : CSharpSyntaxWalker
{
    private bool visited;
    private readonly SemanticModel model;
    private readonly CancellationToken cancellationToken;

    private readonly ImmutableHashSet<string>.Builder definedTypesBuilder = ImmutableHashSet.CreateBuilder<string>();
    private readonly ImmutableHashSet<string>.Builder usedFilesBuilder = ImmutableHashSet.CreateBuilder<string>();
    public ImmutableHashSet<string> UsedFiles => usedFilesBuilder.ToImmutable();
    public ImmutableHashSet<string> DefinedTypeNames => definedTypesBuilder.ToImmutable();

    private void ThrowIfNotVisited()
    {
        if (!visited)
            throw new InvalidOperationException("Not Visited");
    }
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
    public override void VisitCompilationUnit(CompilationUnitSyntax node)
    {
        base.VisitCompilationUnit(node);
        visited = true;
    }

    public override void Visit(SyntaxNode? node)
    {
        if (node == null)
            return;
        if (node is BaseTypeDeclarationSyntax typeDeclaration)
        {
            FindDeclaredType(typeDeclaration);
        }
        //else if (node is DelegateDeclarationSyntax delegateDeclaration)
        //{
        //    FindDeclaredType(delegateDeclaration);
        //}
        else if (node is UsingDirectiveSyntax) return;
        else if (model.GetSymbolInfo(node, cancellationToken).Symbol is ITypeSymbol nodeSymbol)
        {
            usedFilesBuilder.UnionWith(nodeSymbol.Locations
                    .Where(l => l.Kind == LocationKind.SourceFile)
                    .Select(l => l.SourceTree?.FilePath)
                    .OfType<string>());
        }

        base.Visit(node);
    }
}

