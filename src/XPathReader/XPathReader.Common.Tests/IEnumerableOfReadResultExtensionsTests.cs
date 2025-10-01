using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using FakeItEasy;

namespace XPathReader.Common.Tests
{
    [TestFixture]
    public class IEnumerableOfReadResultExtensionsTests
    {
        private const string _xml1 = """
                <root>
                    <child>value</child>
                </root>
                """;
        private const string _xml2 = """
                <root>
                    <child>value1</child>
                </root>
                """;

        [Test]
        public void ToPersisted_ReturnsIEnumerableOfPersistedReadResult()
        {

            var first = CreateReadResult(_xml1);
            var second = CreateReadResult(_xml2);

            IEnumerable<ReadResult> collection = [first, second];

            List<PersistedReadResult> results = collection.ToPersisted().ToList();

            Assert.That(results.Count, Is.EqualTo(2));

            Assert.That(results[0].Node.ToString(SaveOptions.DisableFormatting), Is.EqualTo("<child>value</child>"));
            Assert.That(results[1].Node.ToString(SaveOptions.DisableFormatting), Is.EqualTo("<child>value1</child>"));
        }

        [Test]
        public void ToPersisted_ReturnsIAsyncEnumerableOfPersistedReadResult()
        {

            var first = CreateReadResult(_xml1);
            var second = CreateReadResult(_xml2);

            IAsyncEnumerable<ReadResult> collection = Create(first, second);

            List<PersistedReadResult> results = collection.ToPersistedAsync().ToBlockingEnumerable().ToList();

            Assert.That(results.Count, Is.EqualTo(2));

            Assert.That(results[0].Node.ToString(SaveOptions.DisableFormatting), Is.EqualTo("<child>value</child>"));
            Assert.That(results[1].Node.ToString(SaveOptions.DisableFormatting), Is.EqualTo("<child>value1</child>"));
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async IAsyncEnumerable<ReadResult> Create(ReadResult first, ReadResult second)
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
            string requestedXPath = "/root/child";

            var subReader = reader.ReadSubtree();
            subReader.MoveToContent();
            return new ReadResult(actualXPath, subReader, requestedXPath);
        }
    }
}
