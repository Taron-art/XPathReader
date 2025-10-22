
using System;
using ARTX.XPath;
using NUnit.Framework;

#nullable enable

namespace PackageUsage.Tests
{

    [TestFixture]
    public partial class GeneratedCodeTests
    {
        [GeneratedXPathReader("/bookstore/title")]
        private partial XPathReader GetReader();

        [Test]
        public void XPathReader_GetReader_ReturnsReaderOfCurrentVersion()
        {
            Version? testAssemblyVersion = GetType().Assembly.GetName().Version;

            Version? readerAssemblyVersion = GetReader().GetType().Assembly.GetName().Version;

            Assert.That(testAssemblyVersion, Is.EqualTo(readerAssemblyVersion), "The generated code is not using the expected version of ARTX.XPath.");
        }
    }
}