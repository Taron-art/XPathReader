using ARTX.XPath.Internal;

namespace ARTX.Common.Tests.Internal
{
    [TestFixture]
    [TestOf(typeof(ThreadSafeNameTable))]
    public class ThreadSafeNameTableTests
    {
        private ThreadSafeNameTable _nameTable;

        [SetUp]
        public void Setup()
        {
            _nameTable = new ThreadSafeNameTable();
        }

        [Test]
        public void Add_String_ReturnsInterned()
        {
            // Arrange
            const string input = "test";

            // Act
            string result = _nameTable.Add(input);

            // Assert
            Assert.That(result, Is.EqualTo(input));
            Assert.That(_nameTable.Get(input), Is.SameAs(result));
        }

        [Test]
        public void Add_CharArray_ReturnsInterned()
        {
            // Arrange
            char[] input = "test".ToCharArray();

            // Act
            string result = _nameTable.Add(input, 0, input.Length);

            // Assert
            Assert.That(result, Is.EqualTo(new string(input)));
            Assert.That(_nameTable.Get(input, 0, input.Length), Is.SameAs(result));
        }

        [Test]
        public void Add_SameString_ReturnsSameReference()
        {
            const string input = "duplicate";

            string first = _nameTable.Add(input);
            string second = _nameTable.Add(input);

            Assert.That(second, Is.SameAs(first));
        }

        [Test]
        public void Add_EmptyString_WorksCorrectly()
        {
            string result = _nameTable.Add(string.Empty);

            Assert.That(result, Is.Empty);
            Assert.That(_nameTable.Get(string.Empty), Is.SameAs(result));
        }

        [Test]
        public void Add_ConcurrentAccess_ThreadSafe()
        {
            const int threadCount = 10;
            const int iterationsPerThread = 1000;
            Task[] tasks = new Task[threadCount];
            HashSet<string>[] results = new HashSet<string>[threadCount];

            for (int i = 0; i < threadCount; i++)
            {
                int threadIndex = i;
                results[i] = new HashSet<string>();
                tasks[i] = Task.Run(() =>
                {
                    for (int j = 0; j < iterationsPerThread; j++)
                    {
                        string value = $"test{j}";
                        string result = _nameTable.Add(value);
                        results[threadIndex].Add(result);
                    }
                });
            }

            Task.WaitAll(tasks);

            // Verify all threads got the same references for the same strings
            HashSet<string> firstThreadResults = results[0];
            Parallel.For(0, threadCount, (i) =>
            {
                foreach (string value in firstThreadResults)
                {
                    Assert.That(results[i], Does.Contain(value),
                        $"Thread {i} missing value {value}");

                    Assert.That(results[i].TryGetValue(value, out string? retrievedValue));

                    Assert.That(_nameTable.Get(value), Is.SameAs(retrievedValue));
                }
            });
        }

        [Test]
        public void Add_CharArray_PartialRange()
        {
            char[] input = "testvalue".ToCharArray();
            string result = _nameTable.Add(input, 4, 5); // should get "value"

            Assert.That(result, Is.EqualTo("value"));
            Assert.That(_nameTable.Get(input, 4, 5), Is.SameAs(result));
        }
    }
}
