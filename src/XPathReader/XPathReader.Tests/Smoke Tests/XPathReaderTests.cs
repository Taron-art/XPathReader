using ARTX.XPath;
using ARTX.XPath.Internal;

namespace ARTX.XPathReader.Tests.Smoke_Tests
{
    [TestFixture]
    [TestOf(typeof(XPath.XPathReader))]
    internal partial class XPathReaderTests
    {

        [GeneratedXPathReader("/ukraine/geography/regions/region/name|/ukraine/economy/sectors/sector/companies/company|/ukraine/culture/languages/language")]
        private static partial XPath.XPathReader UkraineXmlReader();

        [GeneratedXPathReader("/company/name")]
        private static partial XPath.XPathReader CompanyNameReader { get; }

        [Test]
        public void UkraineXmlReader_ReturnsDifferentData()
        {
            Stream testFile = File.OpenRead("Smoke Tests/TestFile.xml");

            var firstInstance = UkraineXmlReader();
            var secondInstance = UkraineXmlReader();

            Assert.That(firstInstance, Is.SameAs(secondInstance));
            List<string> regions = [];
            List<string> companies = [];
            List<PersistedReadResult> languages = [];
            foreach (ReadResult result in UkraineXmlReader().Read(testFile))
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
                        companies.Add(CompanyNameReader.ReadFromSubtree(result.NodeReader).Select(result => result.NodeReader.ReadOuterXml()).Single());
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
        public async Task UkraineXmlReader_ReturnsDifferentDataAsync()
        {
            Stream testFile = File.OpenRead("Smoke Tests/TestFile.xml");

            List<string> regions = [];
            List<string> companies = [];
            List<PersistedReadResult> languages = [];
            await foreach (ReadResult result in UkraineXmlReader().ReadAsync(testFile))
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
    }
}
