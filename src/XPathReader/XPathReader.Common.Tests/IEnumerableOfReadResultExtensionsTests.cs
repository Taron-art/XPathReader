using System.Diagnostics.Contracts;
using System.Xml;
using System.Xml.Linq;
using ARTX.XPath;
using FakeItEasy;

namespace ARTX.Common.Tests
{
    [TestFixture]
    [TestOf(typeof(IEnumerableOfReadResultExtensions))]
    public class IEnumerableOfReadResultExtensionsTests
    {
        private const string Xml1 = """
                <root>
                    <child>value</child>
                </root>
                """;
        private const string Xml2 = """
                <root>
                    <child>value1</child>
                </root>
                """;

        [Test]
        public void ToPersisted_ReturnsIEnumerableOfPersistedReadResult()
        {
            ReadResult first = CreateReadResult(Xml1);
            ReadResult second = CreateReadResult(Xml2);

            IEnumerable<ReadResult> collection = [first, second];

            List<PersistedReadResult> results = collection.ToPersisted().ToList();

            Assert.That(results, Has.Count.EqualTo(2));

            Assert.That(results[0].Node.ToString(SaveOptions.DisableFormatting), Is.EqualTo("<child>value</child>"));
            Assert.That(results[1].Node.ToString(SaveOptions.DisableFormatting), Is.EqualTo("<child>value1</child>"));
        }

        [Test]
        public void ToPersisted_ReturnsIAsyncEnumerableOfPersistedReadResult()
        {
            ReadResult first = CreateReadResult(Xml1);
            ReadResult second = CreateReadResult(Xml2);

            IAsyncEnumerable<ReadResult> collection = Create(first, second);

            List<PersistedReadResult> results = collection.ToPersistedAsync().ToBlockingEnumerable().ToList();

            Assert.That(results, Has.Count.EqualTo(2));

            Assert.That(results[0].Node.ToString(SaveOptions.DisableFormatting), Is.EqualTo("<child>value</child>"));
            Assert.That(results[1].Node.ToString(SaveOptions.DisableFormatting), Is.EqualTo("<child>value1</child>"));
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private static async IAsyncEnumerable<ReadResult> Create(ReadResult first, ReadResult second)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            yield return first;
            yield return second;
        }

        [Pure]
        private static ReadResult CreateReadResult(string xml1)
        {
            XmlReader reader = XmlReader.Create(new StringReader(xml1), new XmlReaderSettings { Async = true });
            reader.MoveToContent();
            Assert.That(reader.ReadToDescendant("child"));
            IXPathBuilder actualXPath = A.Fake<IXPathBuilder>();
            A.CallTo(() => actualXPath.GetXPath()).Returns("/root/child[1]");
            const string requestedXPath = "/root/child";

            XmlReader subReader = reader.ReadSubtree();
            subReader.MoveToContent();
            return new ReadResult(actualXPath, subReader, requestedXPath, "child");
        }
    }
}
