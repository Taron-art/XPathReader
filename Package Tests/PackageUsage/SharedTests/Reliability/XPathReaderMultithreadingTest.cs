using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ARTX.XPath;
using NUnit.Framework;

namespace PackageUsage.Reliability.Tests
{
    [TestFixture]
    public partial record XPathReaderMultithreadingTests
    {

        [GeneratedXPathReader("/Scientists/Scientist/Field|/Scientists/Scientist/Nationality")]
        private static partial XPathReader GetScientistsReader();

        [Test]
        public async Task GetScientistsReader_MultipleThreads_CanReadConcurrently()
        {
            const int threadCount = 100;
            const int iterationsPerThread = 10;
            XPathReader reader = GetScientistsReader();

            ConcurrentBag<Task> tasks = new();
            Parallel.For(0, threadCount, _ =>
            {
                for (int i = 0; i < iterationsPerThread; i++)
                {
                    int fileIndex = (i % 2) == 0 ? 1 : 2;
                    tasks.Add(AssertReadingContent(reader, fileIndex));
                }
            });

            await Task.WhenAll(tasks);
        }

        private async Task AssertReadingContent(XPathReader reader, int fileIndex)
        {
            FileInfo fileInfo = new($"Reliability/Scientists{fileIndex}.xml");

            Assert.That(fileInfo.Exists, Is.True, $"Test file not found: {fileInfo.FullName}");
            using FileStream fileStream = fileInfo.OpenRead();

            if (fileIndex == 2)
            {
                IGrouping<string, string>[] result = reader.Read(fileStream)
                    .Where(item => item.RequestedXPath == "/Scientists/Scientist/Nationality")
                    .Select(item => item.NodeReader.ReadElementContentAsString())
                    .GroupBy(item => item)
                    .ToArray();

                Assert.That(result, Has.Length.EqualTo(9));
                Assert.That(result.First(g => g.Key == "British"), Has.Exactly(4).Items, "Expected 4 British scientists.");
            }
            else
            {
                int expectedCount = 10;
                int count = 0;
                await foreach (var item in reader.ReadAsync(fileStream).ToPersistedAsync())
                {
                    switch (item.RequestedXPath)
                    {
                        case "/Scientists/Scientist/Field":
                            Assert.That(item.Node.Value, Is.Not.Null.And.Not.Empty, "Field value should not be null or empty.");
                            count++;
                            break;
                    }
                }
                Assert.That(count, Is.EqualTo(expectedCount));
            }
        }
    }
}