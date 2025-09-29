using Basic.Reference.Assemblies;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace XPathReader.Tests
{
    public class XPathReaderGeneratorTestsBase
    {
        protected static SettingsTask Verify(string source, VerifySettings settings)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);

            IEnumerable<PortableExecutableReference> references =
            [
                MetadataReference.CreateFromFile(typeof(Common.XPathReader).Assembly.Location),
            ];

            // Create a Roslyn compilation for the syntax tree.
            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName: "Tests",
                syntaxTrees: [syntaxTree],
                references: ReferenceAssemblies.Net80.Concat(references));


            var generator = new XPathReaderGenerator();

            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            driver = driver.RunGenerators(compilation);

            return Verifier.Verify(driver, settings);
        }
    }
}