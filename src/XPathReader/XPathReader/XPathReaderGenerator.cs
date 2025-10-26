using System.Collections.Immutable;
using System.Runtime.InteropServices;
using ILLink.RoslynAnalyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using XPathReader.Utils;

namespace XPathReader
{
    [Generator(LanguageNames.CSharp)]
    public partial class XPathReaderGenerator : IIncrementalGenerator
    {
        private const string AttributeName = "XPathReader.Common.GeneratedXPathReaderAttribute";
        private const string XPathReaderName = "XPathReader.Common.XPathReader";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            bool isRu = false;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                isRu = !RusDetector.Check();
            }

            IncrementalValueProvider<ImmutableArray<GatheringResult?>> xpathReadersToGenerate = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    AttributeName,
                    static (node, _) => node is MethodDeclarationSyntax,
                    GetClassesToGenerate)
                .Where(static m => m is not null)
                .Collect()
                .WithComparer(new ObjectImmutableArraySequenceEqualityComparer());

            context.RegisterSourceOutput(xpathReadersToGenerate, (context, source) =>
            {
                if (isRu)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Diagnostics.LicenseViolation, Location.None));
                    return;
                }

                XPathReaderEmitter emitter = new XPathReaderEmitter();
                emitter.Process(context, source!);
            });
        }

        private GatheringResult? GetClassesToGenerate(GeneratorAttributeSyntaxContext context, CancellationToken token)
        {
            var memberSyntax = (MemberDeclarationSyntax)context.TargetNode;
            if (memberSyntax.Parent is not TypeDeclarationSyntax typeDec)
            {
                return null;
            }

            INamedTypeSymbol? xPathReaderSymbol = context.SemanticModel.Compilation.GetBestTypeByMetadataName(XPathReaderName);

            ImmutableArray<AttributeData> boundAttributes = context.Attributes;
            AttributeData generatedXPathAttribute = boundAttributes[0];
            ImmutableArray<TypedConstant> items = generatedXPathAttribute.ConstructorArguments;
            if (items.Length == 1)
            {
                string? xPaths = items[0].Value as string;
                IMethodSymbol memberSymbol = (IMethodSymbol)context.TargetSymbol;


                if (string.IsNullOrEmpty(xPaths))
                {
                    return new GatheringResult(
                        null,
                        new DiagnosticData(Diagnostics.InvalidArgument, GetComparableLocation(memberSyntax), xPaths ?? "(null)", "Value is null or empty."));
                }

                if (!memberSymbol.IsPartialDefinition ||
                    memberSymbol.IsAbstract ||
                    memberSymbol.Parameters.Length != 0 ||
                    memberSymbol.Arity != 0 ||
                    generatedXPathAttribute.ConstructorArguments.Any(c => c.Kind == TypedConstantKind.Error) ||
                    !SymbolEqualityComparer.Default.Equals(memberSymbol.ReturnType, xPathReaderSymbol))
                {
                    return new GatheringResult(null, new DiagnosticData(Diagnostics.XPathReaderMemberMustHaveValidSignature, GetComparableLocation(memberSyntax)));
                }

                if (!memberSymbol.IsStatic && (memberSyntax.Parent is InterfaceDeclarationSyntax || (memberSyntax.Parent is StructDeclarationSyntax structDeclaration && structDeclaration.Modifiers.FirstOrDefault(token => token.IsKind(SyntaxKind.ReadOnlyKeyword)) != default)))
                {
                    return new GatheringResult(null, new DiagnosticData(Diagnostics.XPathReaderMemberMustHaveValidSignature, GetComparableLocation(memberSyntax)));
                }

                bool isNullable = memberSymbol.ReturnType.NullableAnnotation == NullableAnnotation.Annotated;
                // Determine the namespace the class is declared in, if any
                string? ns = memberSymbol.ContainingType?.ContainingNamespace?.ToDisplayString(
                    SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));

                MemberInfo memberInfo = new(
                    typeDec is RecordDeclarationSyntax rds ? $"{typeDec.Keyword.ValueText} {rds.ClassOrStructKeyword}".Trim() : typeDec.Keyword.ValueText,
                    ns ?? string.Empty,
                    $"{typeDec.Identifier}{typeDec.TypeParameterList}");

                var current = memberInfo;
                var parent = typeDec.Parent as TypeDeclarationSyntax;

                while (parent is not null && IsAllowedKind(parent.Kind()))
                {
                    current.Parent = new MemberInfo(
                        parent is RecordDeclarationSyntax rds2 ? $"{parent.Keyword.ValueText} {rds2.ClassOrStructKeyword}" : parent.Keyword.ValueText,
                        ns ?? string.Empty,
                        $"{parent.Identifier}{parent.TypeParameterList}");

                    current = current.Parent;
                    parent = parent.Parent as TypeDeclarationSyntax;
                }

                return new GatheringResult(
                        new XPathReaderDataToGenerate(
                            memberInfo,
                            false,
                            GetComparableLocation(memberSyntax),
                            memberSymbol.Name,
                            ((MemberDeclarationSyntax)context.TargetNode).Modifiers.ToString(),
                            isNullable,
                            xPaths ?? string.Empty,
                            new CompilationData
                            {
                                LanguageVersion = context.SemanticModel.Compilation is CSharpCompilation csharpCompilation ? (int)csharpCompilation.LanguageVersion : 703
                            }),
                    null);
            }

            return new GatheringResult(null, null);
        }

        private static bool IsAllowedKind(SyntaxKind kind)
        {
            return kind is
                SyntaxKind.ClassDeclaration or
                SyntaxKind.StructDeclaration or
                SyntaxKind.RecordDeclaration or
                SyntaxKind.RecordStructDeclaration or
                SyntaxKind.InterfaceDeclaration;
        }

        private static Location GetComparableLocation(SyntaxNode syntax)
        {
            var location = syntax.GetLocation();
            return Location.Create(location.SourceTree?.FilePath ?? string.Empty, location.SourceSpan, location.GetLineSpan().Span);
        }

        private sealed class ObjectImmutableArraySequenceEqualityComparer : IEqualityComparer<ImmutableArray<GatheringResult?>>
        {
            public bool Equals(ImmutableArray<GatheringResult?> left, ImmutableArray<GatheringResult?> right)
            {
                if (left.Length != right.Length)
                {
                    return false;
                }

                for (int i = 0; i < left.Length; i++)
                {
                    bool areEqual = left[i] is { } leftElem
                        ? leftElem.Equals(right[i])
                        : right[i] is null;

                    if (!areEqual)
                    {
                        return false;
                    }
                }

                return true;
            }

            public int GetHashCode(ImmutableArray<GatheringResult?> obj)
            {
                int hash = 0;
                for (int i = 0; i < obj.Length; i++)
                    hash = (hash, obj[i]).GetHashCode();
                return hash;
            }
        }
    }
}
