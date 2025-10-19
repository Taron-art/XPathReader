using System.Xml;
using System.Xml.Linq;
using ARTX.XPath;
using FakeItEasy;

namespace ARTX.Common.Tests
{
    [TestFixture]
    [TestOf(typeof(ReadResult))]
    internal class ReadResultTests
    {
        private const string ElementLocalName = "child";

        [Test]
        public void ToPersistedResult_CopiesData()
        {
            string xml = """
                <root>
                    <child>value</child>
                </root>
                """;
            using XmlReader reader = XmlReader.Create(new StringReader(xml), new XmlReaderSettings { Async = false });
            reader.MoveToContent();
            Assert.That(reader.ReadToDescendant(ElementLocalName));
            IXPathBuilder actualXPath = A.Fake<IXPathBuilder>();
            A.CallTo(() => actualXPath.GetXPath()).Returns("/root/child[1]");
            string requestedXPath = "/root/child";

            var subReader = reader.ReadSubtree();
            subReader.MoveToContent();

            ReadResult readResult = new(actualXPath, subReader, requestedXPath, ElementLocalName);
            PersistedReadResult persistedResult = readResult.ToPersistedResult();
            Assert.That(persistedResult.ActualXPath, Is.EqualTo(actualXPath.GetXPath()));
            Assert.That(persistedResult.RequestedXPath, Is.SameAs(requestedXPath));
            Assert.That(persistedResult.ElementLocalName, Is.SameAs(ElementLocalName));
            Assert.That(persistedResult.Node.ToString(SaveOptions.DisableFormatting), Is.EqualTo("<child>value</child>"));
        }

        [Test]
        public async Task ToPersistedResultAsync_CopiesData()
        {
            string xml = """
                <root>
                    <child>value</child>
                </root>
                """;
            using XmlReader reader = XmlReader.Create(new StringReader(xml), new XmlReaderSettings { Async = true });
            await reader.MoveToContentAsync();
            Assert.That(reader.ReadToDescendant(ElementLocalName));
            IXPathBuilder actualXPath = A.Fake<IXPathBuilder>();
            A.CallTo(() => actualXPath.GetXPath()).Returns("/root/child[1]");
            string requestedXPath = "/root/child";

            var subReader = reader.ReadSubtree();
            await subReader.MoveToContentAsync();

            ReadResult readResult = new(actualXPath, subReader, requestedXPath, ElementLocalName);
            PersistedReadResult persistedResult = await readResult.ToPersistedResultAsync();
            Assert.That(persistedResult.ActualXPath, Is.EqualTo(actualXPath.GetXPath()));
            Assert.That(persistedResult.RequestedXPath, Is.SameAs(requestedXPath));
            Assert.That(persistedResult.ElementLocalName, Is.SameAs(ElementLocalName));
            Assert.That(persistedResult.Node.ToString(SaveOptions.DisableFormatting), Is.EqualTo("<child>value</child>"));
        }
    }
}
