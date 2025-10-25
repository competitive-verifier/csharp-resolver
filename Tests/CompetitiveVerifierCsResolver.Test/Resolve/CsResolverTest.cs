using CompetitiveVerifierCsResolver.Models;
using CompetitiveVerifierCsResolver.Resolve;
using CompetitiveVerifierCsResolver.Verifier;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Moq;
using System.CommandLine;

namespace CompetitiveVerifierCsResolver;
public class CsResolverTest
{
    private static MetadataReference? _Mscorlib;
    private readonly Mock<TextWriter> stdout, stderr;
    private readonly Mock<IPathResolver> pathResolver;

    private static MetadataReference Mscorlib => _Mscorlib ??= MetadataReference.CreateFromFile(typeof(object).Assembly.Location);

    public CsResolverTest()
    {
        stdout = new Mock<TextWriter>();
        stderr = new Mock<TextWriter>();
        pathResolver = new Mock<IPathResolver>();
        pathResolver.Setup(p => p.RelativePath(It.IsAny<string>()))
                .Returns(new Func<string, string>(s => Path.GetRelativePath("/foo/bar", s).Replace('\\', '/')));
    }

    public static Solution CreateSolution()
    {
        var project1Id = ProjectId.CreateNewId();
        var project2Id = ProjectId.CreateNewId();

        var project1Doc1Id = DocumentId.CreateNewId(project1Id);
        var project1Doc2Id = DocumentId.CreateNewId(project1Id);

        var project2Doc1Id = DocumentId.CreateNewId(project2Id);
        return new AdhocWorkspace().CurrentSolution
            .AddProject(project1Id, "Project1", "P1", LanguageNames.CSharp)
            .AddDocument(project1Doc1Id, "R", """
            //competitive-verifier: document_title RN
            public record R(int Num);
            """, filePath: "/foo/bar/P1/R.cs")
            .AddDocument(project1Doc2Id, "P", """
            namespace Pn
            { 
                using System;
                public static class P
                {
                    public static void Write() => Console.WriteLine(new R(3));
                }
            }
            """, filePath: "/foo/bar/P1/P.cs")
            .AddMetadataReference(project1Id, Mscorlib)
            .WithProjectCompilationOptions(project1Id, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .AddProject(project2Id, "Project2", "P2", LanguageNames.CSharp)
            .AddDocument(project2Doc1Id, "P", """
            using Pn;
            namespace Test;
            // @for http://example.com/comment
            public static class Solve
            {
                public string Url => "http://example.com/comment";
                public static void Main() => P.Write();
            }
            """, filePath: "/foo/bar/P2/Solve.cs")
            .AddMetadataReference(project2Id, Mscorlib)
            .AddProjectReference(project2Id, new(project1Id))
            .WithProjectCompilationOptions(project2Id, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    [Fact]
    public async Task Empty()
    {
        Assert.Equal("P1/a.cs", pathResolver.Object.RelativePath("/foo/bar/P1/a.cs"));

        var solution = CreateSolution();
        var verifications = await new CsResolver(stdout.Object, stderr.Object).ResolveImplAsync(solution, pathResolver.Object
            , new Dictionary<string, UnitTestResult>()
            , new Dictionary<string, ProblemVerification[]>()
            , TestContext.Current.CancellationToken);
        var expected = """
            {"files":{"P1/P.cs":{"dependencies":["P1/R.cs"],"document_attributes":{},"verification":[]},"P1/R.cs":{"dependencies":[],"document_attributes":{"document_title":"RN"},"verification":[]},"P2/Solve.cs":{"dependencies":["P1/P.cs"],"document_attributes":{"links":["http://example.com/comment"]},"verification":[]}}}
            """;
        Assert.Equivalent(expected, verifications.ToJson());
    }

    [Fact]
    public async Task TestResult()
    {
        Assert.Equal("P1/a.cs", pathResolver.Object.RelativePath("/foo/bar/P1/a.cs"));

        var solution = CreateSolution();
        var verifications = await new CsResolver(stdout.Object, stderr.Object).ResolveImplAsync(
            solution,
            pathResolver.Object,
            new Dictionary<string, UnitTestResult>
            {
                { "Test.Solve", new("Test.Solve",3,2,1) },
            },
            new Dictionary<string, ProblemVerification[]>
            {
            }
            , TestContext.Current.CancellationToken);
        var expected = """
            {"files":{"P1/P.cs":{"dependencies":["P1/R.cs"],"document_attributes":{},"verification":[]},"P1/R.cs":{"dependencies":[],"document_attributes":{"document_title":"RN"},"verification":[]},"P2/Solve.cs":{"dependencies":["P1/P.cs"],"document_attributes":{"links":["http://example.com/comment"]},"verification":[{"type":"const","status":"success"},{"type":"const","status":"success"},{"type":"const","status":"success"},{"type":"const","status":"skipped"},{"type":"const","status":"skipped"},{"type":"const","status":"failure"}]}}}
            """;
        Assert.Equivalent(expected, verifications.ToJson());
    }

    [Fact]
    public async Task Problem()
    {
        Assert.Equal("P1/a.cs", pathResolver.Object.RelativePath("/foo/bar/P1/a.cs"));

        var solution = CreateSolution();
        var verifications = await new CsResolver(stdout.Object, stderr.Object).ResolveImplAsync(
            solution,
            pathResolver.Object,
            new Dictionary<string, UnitTestResult>
            {
            },
            new Dictionary<string, ProblemVerification[]>
            {
                {
                    "Test.Solve",
                    new ProblemVerification[]
                    {
                        new(Url:"http://example.com/solve",Command:"dontet sol"),
                        new(Url:"http://example.com/solve",Command:"dontet sol err", Error:1e-8,Name:"C#(sol-err)"),
                    }
                },
            }
            , TestContext.Current.CancellationToken);
        var expected = """
            {"files":{"P1/P.cs":{"dependencies":["P1/R.cs"],"document_attributes":{},"verification":[]},"P1/R.cs":{"dependencies":[],"document_attributes":{"document_title":"RN"},"verification":[]},"P2/Solve.cs":{"dependencies":["P1/P.cs"],"document_attributes":{"links":["http://example.com/comment"]},"verification":[{"type":"problem","problem":"http://example.com/solve","command":"dontet sol"},{"type":"problem","name":"C#(sol-err)","problem":"http://example.com/solve","command":"dontet sol err","error":1E-08}]}}}
            """;
        Assert.Equivalent(expected, verifications.ToJson());
    }
}