using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using ARTX;
using ARTX.XPath;
using NUnit.Framework;

namespace SharedTests.CustomSettings
{
    [TestFixture]
    public partial class CustomSettingsTests
    {
        [GeneratedXPathReader("/comedyArchive/performers/comedian/quote[1]")]
        private static partial XPathReader GetComedyQuotesReader();

        [GeneratedXPathReader("/comedyArchive/genres/genre")]
        private static partial XPathReader GetComedyGenresReader();

        [Test]
        public void WhenDocTypeIsPresent_ValuesAreExtracted_AndValidationPassed()
        {
            FileStream stream = File.OpenRead("CustomSettings/comedy.xml");

            XmlSchemaSet sc = new();
            sc.Add("http://example.org/comedy", "CustomSettings/comedy.xsd");

            XmlReaderSettings settings = new()
            {
                DtdProcessing = DtdProcessing.Parse,
                ValidationType = ValidationType.Schema,
                Schemas = sc
            };

            settings.ValidationEventHandler += (sender, args) =>
            {
                Assert.Fail($"XML Validation Error: {args.Message}");
            };

            string[] quotes = GetComedyQuotesReader().Read(stream, settings, null).Select(item => item.NodeReader.ReadElementContentAsString()).ToArray();

            Assert.That(quotes, Has.Length.EqualTo(2));

            Assert.That(quotes[0], Is.EqualTo("Politics is the only circus where the clowns are in charge."));
            Assert.That(quotes[1], Is.EqualTo("Funny faces speak louder than words."));
        }

        [Test]
        public async Task WhenDocTypeIsPresent_ValuesAreExtracted_AndValidationPassedAsync()
        {
            FileStream stream = File.OpenRead("CustomSettings/comedy.xml");

            XmlSchemaSet sc = new();
            sc.Add("http://example.org/comedy", "CustomSettings/comedy.xsd");

            XmlReaderSettings settings = new()
            {
                DtdProcessing = DtdProcessing.Parse,
                ValidationType = ValidationType.Schema,
                Schemas = sc,
                Async = true
            };

            settings.ValidationEventHandler += (sender, args) =>
            {
                Assert.Fail($"XML Validation Error: {args.Message}");
            };

            List<string> quotes = new();

            await foreach (ReadResult item in GetComedyQuotesReader().ReadAsync(stream, settings, null))
            {
                string quote = await item.NodeReader.ReadElementContentAsStringAsync();
                quotes.Add(quote);
            }

            Assert.That(quotes, Has.Count.EqualTo(2));

            Assert.That(quotes[0], Is.EqualTo("Politics is the only circus where the clowns are in charge."));
            Assert.That(quotes[1], Is.EqualTo("Funny faces speak louder than words."));
        }

        [Test]
        public void DeserializeComedyGenres_FromXmlFile()
        {
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(Genre));
            using FileStream stream = File.OpenRead("CustomSettings/comedy.xml");
            XmlReaderSettings settings = new()
            {
                DtdProcessing = DtdProcessing.Parse
            };

            var genres = GetComedyGenresReader().Read(stream, settings, null).Select(genreXml => serializer.Deserialize(genreXml.NodeReader) as Genre).ToArray();
            Assert.That(genres, Has.Length.EqualTo(4));
            Assert.That(genres[0]?.Name, Is.EqualTo("Satire"));
            Assert.That(genres[0]?.Origin, Is.EqualTo("Ancient Greece"));
            Assert.That(genres[1]?.Name, Is.EqualTo("Slapstick"));
            Assert.That(genres[1]?.Origin, Is.EqualTo("Italy"));
            Assert.That(genres[2]?.Name, Is.EqualTo("Stand-up"));
            Assert.That(genres[2]?.Origin, Is.EqualTo("USA"));
            Assert.That(genres[3]?.Name, Is.EqualTo("Sketch"));
            Assert.That(genres[3]?.Origin, Is.EqualTo("UK"));
        }
    }
}
