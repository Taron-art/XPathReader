using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ARTX.XPath;
using NUnit.Framework;

namespace SharedTests.Functionality
{
    [TestFixture]
    public partial class LimitationsTests
    {
        [GeneratedXPathReader("/a/b/a/b/a/b/a/b/a/b/a/b/a/b/a/b/a/b/a/b/a/b/a/b/a/b/a/b/a/b/a/b/a/b/a/b/a/b/a/b/a/b/a/b/a/b/a/b/a/b/a/b/a/b/a/b/a/b/a/b/a/b/a/c")]
        private partial XPathReader Get64DepthReader();

        [Test]
        public void XPathReader_Supports64DepthXml()
        {
            // Arrange
            const int depth = 64;
            const int leaves = 8;
            string xmlContent = Utils.XmlGenerator.GenerateXml(depth, leaves);
            byte[] xmlBytes = Encoding.UTF8.GetBytes(xmlContent);
            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                using var stream = new MemoryStream(xmlBytes);
                Assert.That(Get64DepthReader().Read(stream).ToPersisted().Count(), Is.EqualTo(leaves));
            });
        }
    }
}
