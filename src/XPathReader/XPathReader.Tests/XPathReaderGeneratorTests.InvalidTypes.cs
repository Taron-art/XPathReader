namespace ARTX.XPathReader.Tests
{
    [TestFixture]
    [TestOf(typeof(XPathReaderGenerator))]
    public class XPathReaderGeneratorInvalidTypesTests : XPathReaderGeneratorTestsBase
    {
        [Test]
        public async Task Generate_NonStaticInInterface_ProducesError()
        {
            const string source =
                """
                namespace XPathReader.TestInterface
                {
                    using System;
                    using ARTX.XPath;

                    interface ITestClass
                    {
                        [GeneratedXPathReader("/root/child1|/root/child|/root/child.important|/root/child-3/grandchild1")]
                        private partial XPathReader CreateReader();
                    }
                }
                """;

            VerifySettings settings = CreateSettingsForSourceTests();
            await Verify(source, settings);
        }

        [Test]
        public async Task Generate_NonStaticInReadonlyStruct_ProducesError()
        {
            const string source =
                """
                namespace XPathReader.TestInterface
                {
                    using System;
                    using ARTX.XPath;

                    public readonly struct ITestClass
                    {
                        [GeneratedXPathReader("/root/child1|/root/child|/root/child.important|/root/child-3/grandchild1")]
                        private partial XPathReader CreateReader();
                    }
                }
                """;

            VerifySettings settings = CreateSettingsForSourceTests();
            await Verify(source, settings);
        }

        private static VerifySettings CreateSettingsForSourceTests()
        {
            VerifySettings settings = new VerifySettings();
            settings.UseMethodName(TestContext.CurrentContext.Test.Name);
            settings.UseTextForParameters("_");
            return settings;
        }
    }
}
