using XPathReader.Common;
using XPathReader.Common.Internal;

namespace XPathReader.Tests.Smoke_Tests
{
    [TestFixture]
    [TestOf(typeof(Common.XPathReader))]
    internal partial class XPathReaderTests
    {

        [GeneratedXPathReader("/ukraine/geography/regions/region/name|/ukraine/economy/sectors/sector/companies/company|/ukraine/culture/languages/language")]
        private static partial Common.XPathReader UkraineXmlReader();

        [Test]
        public void UkraineXmlReader_ReturnsDifferentData()
        {
            Stream testFile = File.OpenRead("Smoke Tests/TestFile.xml");

            var firstInstance = UkraineXmlReader();
            var secondInstance = UkraineXmlReader();

            Assert.That(firstInstance, Is.SameAs(secondInstance));
            List<string> regions = [];
            List<string> companies = [];
            byte languagesCount = 0;
            foreach (var result in UkraineXmlReader().Read(testFile))
            {
                switch (result.RequestedXPath)
                {
                    case "/ukraine/geography/regions/region/name":
                        regions.Add(result.NodeReader.ReadOuterXml());
                        Assert.That(result.ActualXPath.GetXPath(), Is.EqualTo($"/ukraine/geography[1]/regions[1]/region[{regions.Count}]/name[1]"));
                        break;
                    case "/ukraine/economy/sectors/sector/companies/company":
                        companies.Add(result.NodeReader.ReadOuterXml());
                        Assert.That(result.ActualXPath.GetXPath(), Is.EqualTo($"/ukraine/economy[1]/sectors[1]/sector[2]/companies[1]/company[{companies.Count}]"));
                        break;
                    case "/ukraine/culture/languages/language":
                        languagesCount++;
                        break;
                    default:
                        Assert.Fail("Unexpected XPath: " + result.RequestedXPath);
                        break;
                }

                Assert.That(result.NodeReader.NameTable, Is.InstanceOf<ThreadSafeNameTable>());
            }

            Assert.That(regions, Has.Count.EqualTo(2));
            Assert.That(regions[0], Is.EqualTo("<name>Lviv Oblast</name>"));
            Assert.That(regions[1], Is.EqualTo("<name>Kyiv Oblast</name>"));

            Assert.That(companies, Has.Count.EqualTo(2));
            Assert.That(companies[0], Is.EqualTo(
                """
                <company>
                    <name>Grammarly</name>
                    <location>Kyiv</location>
                    <employees>600</employees>
                </company>
                """).IgnoreWhiteSpace);
            Assert.That(companies[1], Is.EqualTo(
                """
                <company>
                    <name>GitLab</name>
                    <location>Kharkiv</location>
                    <employees>300</employees>
                </company>
                """).IgnoreWhiteSpace);

            Assert.That(languagesCount, Is.EqualTo(4));
        }

        [Test]
        public async Task UkraineXmlReader_ReturnsDifferentDataAsync()
        {
            Stream testFile = File.OpenRead("Smoke Tests/TestFile.xml");

            List<string> regions = [];
            List<string> companies = [];
            byte languagesCount = 0;
            await foreach (var result in UkraineXmlReader().ReadAsync(testFile))
            {
                switch (result.RequestedXPath)
                {
                    case "/ukraine/geography/regions/region/name":
                        regions.Add(await result.NodeReader.ReadOuterXmlAsync());
                        Assert.That(result.ActualXPath.GetXPath(), Is.EqualTo($"/ukraine/geography[1]/regions[1]/region[{regions.Count}]/name[1]"));
                        break;
                    case "/ukraine/economy/sectors/sector/companies/company":
                        companies.Add(await result.NodeReader.ReadOuterXmlAsync());
                        Assert.That(result.ActualXPath.GetXPath(), Is.EqualTo($"/ukraine/economy[1]/sectors[1]/sector[2]/companies[1]/company[{companies.Count}]"));
                        break;
                    case "/ukraine/culture/languages/language":
                        languagesCount++;
                        break;
                    default:
                        Assert.Fail("Unexpected XPath: " + result.RequestedXPath);
                        break;
                }

                Assert.That(result.NodeReader.NameTable, Is.InstanceOf<ThreadSafeNameTable>());
            }

            Assert.That(regions, Has.Count.EqualTo(2));
            Assert.That(regions[0], Is.EqualTo("<name>Lviv Oblast</name>"));
            Assert.That(regions[1], Is.EqualTo("<name>Kyiv Oblast</name>"));

            Assert.That(companies, Has.Count.EqualTo(2));
            Assert.That(companies[0], Is.EqualTo(
                """
                <company>
                    <name>Grammarly</name>
                    <location>Kyiv</location>
                    <employees>600</employees>
                </company>
                """).IgnoreWhiteSpace);
            Assert.That(companies[1], Is.EqualTo(
                """
                <company>
                    <name>GitLab</name>
                    <location>Kharkiv</location>
                    <employees>300</employees>
                </company>
                """).IgnoreWhiteSpace);

            Assert.That(languagesCount, Is.EqualTo(4));
        }
    }
}
