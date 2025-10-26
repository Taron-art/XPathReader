using System.Reflection;
using Basic.Reference.Assemblies;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace XPathReader.Tests
{
    [TestFixture]
    [TestOf(typeof(XPathReaderGenerator))]
    internal class XPathReaderGeneratorTests
    {

        [Test]
        public async Task GeneratesXPathReaderCorrectly()
        {
            // The source code to test
            var source = /* lang=c#-test */
                """
                namespace XPathReader.TestInterface
                {
                    using System;
                    using XPathReader.Common;

                    public partial class TestClass
                    {
                        [GeneratedXPathReader("/root/child1|/root/child|/root/child/|root/child3/grandchild1")]
                        public partial XPathReader CreateReader();
                    }
                }
                """;
            await Verify(source);
        }

        private static SettingsTask Verify(string source)
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

            return Verifier.Verify(driver);
        }
    }
}