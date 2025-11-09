using System.IO;
using ARTX.XPath;
using ARTX.XPath.Internal;

namespace ARTX.XPathReader.Tests.Smoke_Tests
{
    [TestFixture]
    [TestOf(typeof(XPath.XPathReader))]
    public partial class XPathReaderTests
    {

        [GeneratedXPathReader("/ukraine/geography/regions/region/name|/ukraine/economy/sectors/sector/companies/company|/ukraine/culture/languages/language")]
        private static partial XPath.XPathReader UkraineXmlReader();

        [GeneratedXPathReader("/company/name|/company/name[1]")]
        private static partial XPath.XPathReader CompanyNameReader { get; }

        [GeneratedXPathReader("/a/b/c")]
        private static partial XPath.XPathReader EmptyLeafElementReader();

        [Test]
        public void UkraineXmlReader_ReturnsDifferentData([Values] bool useTextReader)
        {
            TextReader textReader = null!;
            Stream stream = null!;
            if (useTextReader)
            {
                Stream testFileStream = File.OpenRead("Smoke Tests/TestFile.xml");
                textReader = new StreamReader(testFileStream);
            }
            else
            {
                stream = File.OpenRead("Smoke Tests/TestFile.xml");
            }

            XPath.XPathReader firstInstance = UkraineXmlReader();
            XPath.XPathReader secondInstance = UkraineXmlReader();

            Assert.That(firstInstance, Is.SameAs(secondInstance));
            List<string> regions = [];
            List<string> companies = [];
            List<PersistedReadResult> languages = [];
            foreach (ReadResult result in (useTextReader ? UkraineXmlReader().Read(textReader) : UkraineXmlReader().Read(stream)))
            {
                Assert.That(result.NodeReader.NameTable, Is.InstanceOf<ThreadSafeNameTable>());
                switch (result.RequestedXPath)
                {
                    case "/ukraine/geography/regions/region/name":
                        Assert.That(result.ElementLocalName, Is.EqualTo("name"));
                        Assert.That(result.NodeReader.LocalName, Is.EqualTo("name"));
                        regions.Add(result.NodeReader.ReadOuterXml());
                        Assert.That(result.ActualXPath.GetXPath(), Is.EqualTo($"/ukraine/geography[1]/regions[1]/region[{regions.Count}]/name[1]"));
                        break;
                    case "/ukraine/economy/sectors/sector/companies/company":
                        Assert.That(result.ElementLocalName, Is.EqualTo("company"));
                        Assert.That(result.NodeReader.LocalName, Is.EqualTo("company"));
                        companies.Add(CompanyNameReader.ReadFromSubtree(result.NodeReader)
                            .Select(result =>
                            {
                                Assert.That(result.RequestedXPaths, Is.EquivalentTo(["/company/name", "/company/name[1]"]));
                                return result.NodeReader.ReadOuterXml();
                            })
                            .Single());
                        Assert.That(result.ActualXPath.GetXPath(), Is.EqualTo($"/ukraine/economy[1]/sectors[1]/sector[2]/companies[1]/company[{companies.Count}]"));
                        break;
                    case "/ukraine/culture/languages/language":
                        Assert.That(result.ElementLocalName, Is.EqualTo("language"));
                        Assert.That(result.NodeReader.LocalName, Is.EqualTo("language"));
                        languages.Add(result.ToPersistedResult());
                        break;
                    default:
                        Assert.Fail("Unexpected XPath: " + result.RequestedXPath);
                        break;
                }
            }

            Assert.That(regions, Has.Count.EqualTo(2));
            Assert.That(regions[0], Is.EqualTo("<name>Lviv Oblast</name>"));
            Assert.That(regions[1], Is.EqualTo("<name>Kyiv Oblast</name>"));

            Assert.That(companies, Has.Count.EqualTo(2));
            Assert.That(companies[0], Is.EqualTo(
                """
                <name>Grammarly</name>
                """).IgnoreWhiteSpace);
            Assert.That(companies[1], Is.EqualTo(
                """
                <name>GitLab</name>
                """).IgnoreWhiteSpace);

            Assert.That(languages, Has.Count.EqualTo(4));
        }

        [Test]
        public async Task UkraineXmlReader_ReturnsDifferentDataAsync([Values] bool useTextReader)
        {
            TextReader textReader = null!;
            Stream stream = null!;
            if (useTextReader)
            {
                Stream testFileStream = File.OpenRead("Smoke Tests/TestFile.xml");
                textReader = new StreamReader(testFileStream);
            }
            else
            {
                stream = File.OpenRead("Smoke Tests/TestFile.xml");
            }

            List<string> regions = [];
            List<string> companies = [];
            List<PersistedReadResult> languages = [];
            await foreach (ReadResult result in (useTextReader ? UkraineXmlReader().ReadAsync(textReader) : UkraineXmlReader().ReadAsync(stream)))
            {
                Assert.That(result.NodeReader.NameTable, Is.InstanceOf<ThreadSafeNameTable>());

                switch (result.RequestedXPath)
                {
                    case "/ukraine/geography/regions/region/name":
                        Assert.That(result.ElementLocalName, Is.EqualTo("name"));
                        Assert.That(result.NodeReader.LocalName, Is.EqualTo("name"));
                        regions.Add(await result.NodeReader.ReadOuterXmlAsync());
                        Assert.That(result.ActualXPath.GetXPath(), Is.EqualTo($"/ukraine/geography[1]/regions[1]/region[{regions.Count}]/name[1]"));
                        break;
                    case "/ukraine/economy/sectors/sector/companies/company":
                        Assert.That(result.ElementLocalName, Is.EqualTo("company"));
                        Assert.That(result.NodeReader.LocalName, Is.EqualTo("company"));
                        await foreach (ReadResult companyResult in CompanyNameReader.ReadFromSubtreeAsync(result.NodeReader))
                        {
                            companies.Add(await companyResult.NodeReader.ReadOuterXmlAsync());
                            Assert.That(companyResult.RequestedXPaths, Is.EquivalentTo(["/company/name", "/company/name[1]"]));
                        }
                        Assert.That(result.ActualXPath.GetXPath(), Is.EqualTo($"/ukraine/economy[1]/sectors[1]/sector[2]/companies[1]/company[{companies.Count}]"));
                        break;
                    case "/ukraine/culture/languages/language":
                        Assert.That(result.ElementLocalName, Is.EqualTo("language"));
                        Assert.That(result.NodeReader.LocalName, Is.EqualTo("language"));
                        languages.Add(await result.ToPersistedResultAsync());
                        break;
                    default:
                        Assert.Fail("Unexpected XPath: " + result.RequestedXPath);
                        break;
                }
            }

            Assert.That(regions, Has.Count.EqualTo(2));
            Assert.That(regions[0], Is.EqualTo("<name>Lviv Oblast</name>"));
            Assert.That(regions[1], Is.EqualTo("<name>Kyiv Oblast</name>"));

            Assert.That(companies, Has.Count.EqualTo(2));
            Assert.That(companies[0], Is.EqualTo(
                """
                <name>Grammarly</name>
                """).IgnoreWhiteSpace);
            Assert.That(companies[1], Is.EqualTo(
                """
                <name>GitLab</name>
                """).IgnoreWhiteSpace);

            Assert.That(languages, Has.Count.EqualTo(4));
        }

        [Test]
        [CancelAfter(100)]
        public void EmptyElementReader_ReturnsEmptyNode_WhenPresent(CancellationToken testCancellationToken)
        {
            StringReader testFile = new(
                """
                <a>
                    <b/>
                    <b>
                        <c/>
                        <c/>
                    </b>
                    <b/>
                </a>
                """);
            List<string> elements = [];
            foreach (ReadResult result in EmptyLeafElementReader().Read(testFile))
            {
                testCancellationToken.ThrowIfCancellationRequested();
                elements.Add(result.NodeReader.ReadOuterXml());

                Assert.That(result.ActualXPath.GetXPath(), Is.EqualTo($"/a/b[2]/c[{elements.Count}]"));
            }
            Assert.That(elements, Has.Count.EqualTo(2));
            Assert.That(elements[0], Is.EqualTo("<c />"));
            Assert.That(elements[1], Is.EqualTo("<c />"));
        }
    }
}
