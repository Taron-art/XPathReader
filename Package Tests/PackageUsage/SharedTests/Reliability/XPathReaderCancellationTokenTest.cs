using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ARTX.XPath;
using NUnit.Framework;

namespace SharedTests.Reliability
{
    [TestFixture]
    public partial class XPathReaderCancellationTokenTest
    {
        [GeneratedXPathReader("/Scientists/Scientist/Name")]
        private static partial XPathReader GetScientistsReader();

        [Test]
        [CancelAfter(1000)]
        public void WhenCancelledOnRead_ThrowsOperationCanceledException(CancellationToken testTimeoutToken)
        {
            CancellationTokenSource cts = new();
            cts.CancelAfter(TimeSpan.FromMilliseconds(200));

            Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                while (!testTimeoutToken.IsCancellationRequested)
                {
                    XPathReader reader = GetScientistsReader();
                    FileInfo fileInfo = new($"Reliability/Scientists1.xml");

                    Assert.That(fileInfo.Exists, Is.True, $"Test file not found: {fileInfo.FullName}");
                    using FileStream fileStream = fileInfo.OpenRead();
                    await foreach (var item in reader.ReadAsync(fileStream, cts.Token))
                    {
                        // Simulate some processing delay
                        _ = item.NodeReader.ReadElementContentAsString();
                    }
                }
            });
        }


        [Test]
        [CancelAfter(1000)]
        public void WhenCancelledOnPersistent_ThrowsOperationCanceledException(CancellationToken testTimeoutToken)
        {
            CancellationTokenSource cts = new();
            cts.CancelAfter(TimeSpan.FromMilliseconds(200));

            Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                while (!testTimeoutToken.IsCancellationRequested)
                {
                    XPathReader reader = GetScientistsReader();
                    FileInfo fileInfo = new($"Reliability/Scientists1.xml");

                    Assert.That(fileInfo.Exists, Is.True, $"Test file not found: {fileInfo.FullName}");
                    using FileStream fileStream = fileInfo.OpenRead();
                    await foreach (var item in reader.ReadAsync(fileStream).ToPersistedAsync(cts.Token))
                    {
                    }
                }
            });
        }
    }
}
