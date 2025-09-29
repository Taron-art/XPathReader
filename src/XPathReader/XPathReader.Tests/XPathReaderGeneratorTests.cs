namespace XPathReader.Tests
{
    [TestFixture("public partial class", "public")]
    [TestFixture("internal partial record class", "public static")]
    [TestFixture("public partial record", "private")]
    [TestFixture("public partial struct", "protected internal")]
    [TestFixture("internal ref partial struct", "public")]
    [TestFixture("partial interface", "internal static")]
    [TestFixture("public readonly ref partial struct", "private protected static")]
    [TestOf(typeof(XPathReaderGenerator))]

    public class XPathReaderGeneratorTests : XPathReaderGeneratorTestsBase
    {
        private readonly string _parentModifiers;
        private readonly string _methodModifiers;

        public XPathReaderGeneratorTests(string parentModifiers, string methodModifiers)
        {
            _parentModifiers = parentModifiers;
            _methodModifiers = methodModifiers;
        }

        private static IEnumerable<TestCaseData<string>> GetDataForGeneratesXPathReaderCorrectly()
        {
            yield return (TestCaseData<string>)new TestCaseData<string>("[GeneratedXPathReader(\"/root/child1|/root/child|/root/child.important|/root/child-3/grandchild1\")]").SetArgDisplayNames("Normal_method");
            yield return (TestCaseData<string>)new TestCaseData<string>(
                """"
                [GeneratedXPathReader("""
                /root/child1
                /root/child
                /root/child.important
                /root/child-3/grandchild1
                """
                )]
                """"
                ).SetArgDisplayNames("Normal_method_with_new_line");

            yield return (TestCaseData<string>)new TestCaseData<string>("[GeneratedXPathReader(\"/root/child1[1]|/root/child[ready()]|/root/child.important[1]|/root/child-3[@attribute]/grandchild1[name]\")]").SetArgDisplayNames("Normal_method_predicates");
            yield return (TestCaseData<string>)new TestCaseData<string>("[GeneratedXPathReader(\"/root/child[1]|/root/child[1]\")]").SetArgDisplayNames("Normal_method_duplicates");
        }

        [TestCaseSource(nameof(GetDataForGeneratesXPathReaderCorrectly))]
        public async Task GeneratesXPathReader_NoErrors(string attribute)
        {
            string source =
                $$"""
                namespace XPathReader.TestInterface
                {
                    using System;
                    using XPathReader.Common;

                    {{_parentModifiers}} TestClass
                    {
                        {{attribute}}
                        {{_methodModifiers}} partial XPathReader CreateReader();
                    }
                }
                """;

            VerifySettings settings = CreateSettingsForSourceTests();
            await Verify(source, settings);
        }

        [TestCaseSource(nameof(GetDataForGeneratesXPathReaderCorrectly))]
        public async Task GeneratesXPathReader_ForProperty_NoErrors(string attribute)
        {
            string source =
                $$"""
                namespace XPathReader.TestInterface
                {
                    using System;
                    using XPathReader.Common;

                    {{_parentModifiers}} TestClass
                    {
                        {{attribute}}
                        {{_methodModifiers}} partial XPathReader CreateReader {get;};
                    }
                }
                """;

            VerifySettings settings = CreateSettingsForSourceTests();
            await Verify(source, settings);
        }

        private static IEnumerable<TestCaseData<string>> GetDataForNotGeneratesXPathReader_InvalidArgument()
        {
            yield return (TestCaseData<string>)new TestCaseData<string>("[GeneratedXPathReader(null)]").SetArgDisplayNames("Null_argument");
            yield return (TestCaseData<string>)new TestCaseData<string>("[GeneratedXPathReader(\"\")]").SetArgDisplayNames("Empty_argument");
            yield return (TestCaseData<string>)new TestCaseData<string>("[GeneratedXPathReader(\"/root/child1[1]/\")]").SetArgDisplayNames("Ends_with_solidus");
            // We need to escape two backslashes (one for this string, other so a compiled code still have this backslash) + escape the ": 2+2+1.
            yield return (TestCaseData<string>)new TestCaseData<string>($"[GeneratedXPathReader(\"/root/child1[1]\\\\\")]").SetArgDisplayNames("Ends_with_reverse_solidus");
            yield return (TestCaseData<string>)new TestCaseData<string>($"[GeneratedXPathReader(\"/root/child1 1\")]").SetArgDisplayNames("Invalid_name");
        }


        [TestCaseSource(nameof(GetDataForNotGeneratesXPathReader_InvalidArgument))]
        public async Task NotGeneratesXPathReader_InvalidArgument(string attribute)
        {
            string source =
                $$"""
                namespace XPathReader.TestInterface
                {
                    using System;
                    using XPathReader.Common;

                    {{_parentModifiers}} TestClass
                    {
                        {{attribute}}
                        {{_methodModifiers}} partial XPathReader CreateReader();
                    }
                }
                """;

            var settings = CreateSettingsForSourceTests();
            await Verify(source, settings);
        }

        private VerifySettings CreateSettingsForSourceTests()
        {
            var settings = new VerifySettings();
            settings.UseMethodName(_parentModifiers.Replace(' ', '_') + _methodModifiers.Replace(' ', '_') + '_' + TestContext.CurrentContext.Test.Name);
            settings.UseTextForParameters("_");
            return settings;
        }
    }
}