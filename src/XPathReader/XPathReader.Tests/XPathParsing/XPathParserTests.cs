using XPathReader.XPathParsing;

namespace XPathReader.Tests.XPathParsing
{
    [TestFixture]
    [TestOf(typeof(XPathParser))]
    internal class XPathParserTests
    {
        private XPathParser _parser;

        [OneTimeSetUp]
        public void Setup()
        {
            _parser = new XPathParser();
        }

        [Test]
        [TestCase("/root")]
        [TestCase("/root/child")]
        [TestCase("/root/child/grandchild")]
        public void Parse_SingleXPath_CreatesCorrectTree(string xPath)
        {
            // Act
            var result = _parser.Parse(xPath);

            // Assert
            Assert.That(result.Tree, Is.Not.Null);
            Assert.That(GetAllXPaths(result.Tree), Has.Member(xPath));
        }

        private static IEnumerable<TestCaseData<string, XPathTree>> MultipleXPathsTestCases()
        {
            var expectedTree = new XPathTree(
                new XPathTreeElementWithChildren(new XPathLevelIdentifier("root"))
                {
                    Children =
                    {
                        new XPathTreeLeafElement(new XPathLevelIdentifier("child1"), "/root/child1"),
                        new XPathTreeLeafElement(new XPathLevelIdentifier("child"), "/root/child"),
                    }
                });
            yield return new TestCaseData<string, XPathTree>("/root/child1|/root/child", expectedTree);
            yield return new TestCaseData<string, XPathTree>("/root/child1\n/root/child", expectedTree);
            yield return new TestCaseData<string, XPathTree>("/root/child1\r\n/root/child", expectedTree);
            expectedTree = new XPathTree(
                new XPathTreeElementWithChildren(new XPathLevelIdentifier("root"))
                {
                    Children =
                    {
                        new XPathTreeLeafElement(new XPathLevelIdentifier("a"), "/root/a"),
                        new XPathTreeLeafElement(new XPathLevelIdentifier("b"), "/root/b"),
                        new XPathTreeLeafElement(new XPathLevelIdentifier("c"), "/root/c"),
                    }
                });

            yield return new TestCaseData<string, XPathTree>("/root/a|/root/b|/root/c", expectedTree);

            // predicates are ignored
            expectedTree = new XPathTree(
                new XPathTreeElementWithChildren(new XPathLevelIdentifier("root"))
                {
                    Children =
                    {
                                    new XPathTreeLeafElement(new XPathLevelIdentifier("a"), "/root/a[1]"),
                                    new XPathTreeLeafElement(new XPathLevelIdentifier("b"), "/root/b[node()]"),
                                    new XPathTreeLeafElement(new XPathLevelIdentifier("c"), "/root/c[@attribute]"),
                    }
                });

            yield return new TestCaseData<string, XPathTree>("/root/a[1]|/root/b[node()]|/root/c[@attribute]", expectedTree);
        }

        [Test]
        [TestCaseSource(nameof(MultipleXPathsTestCases))]
        public void Parse_MultipleXPaths_CreatesCorrectTree(string xPaths, XPathTree expected)
        {
            // Act
            XPathTree result = _parser.Parse(xPaths).Tree;

            // Assert
            Assert.That(result, Is.EqualTo(expected).UsingPropertiesComparer());
        }

        [Test]
        [TestCase("/root/test1|/root/test1")]
        [TestCase("/root/test1\n/root/test1")]
        public void Parse_DuplicateXPaths_Duplicates(string xpaths)
        {
            // Act
            var result = _parser.Parse(xpaths);

            // Assert
            Assert.That(GetAllXPaths(result.Tree), Has.Count.EqualTo(2));
        }

        [Test]
        public void Parse_EmptyString_ThrowsArgumentException()
        {
            // Assert
            Assert.Throws<ArgumentException>(() => _parser.Parse(""));
        }

        [Test]
        public void Parse_WhitespaceString_ThrowsArgumentException()
        {
            // Assert
            Assert.Throws<ArgumentException>(() => _parser.Parse("   "));
        }

        [TestCase("/root/a[1]", "PredicateWillBeIgnored", "[1]")]
        [TestCase("/root/b[text()]", "PredicateWillBeIgnored", "[text()]")]
        [TestCase("/root/c[@attr]", "PredicateWillBeIgnored", "[@attr]")]
        public void Parse_XPathWithPredicate_AddsDiagnostic(string xpath, string expectedDiagnostic, string predicatePart)
        {
            // Arrange
            _parser.NonErrorDiagnostics.Clear();

            // Act
            var result = _parser.Parse(xpath);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(_parser.NonErrorDiagnostics, Has.Count.EqualTo(1));
                Assert.That(_parser.NonErrorDiagnostics[0].Descriptor, Is.EqualTo(Diagnostics.PredicateWillBeIgnored));

                object[]? args = _parser.NonErrorDiagnostics[0].Args;
                Assert.That(args, Is.Not.Null);
                Assert.That(args, Has.Length.EqualTo(1));

                if (args is not null)
                {
                    Assert.That(args[0], Is.EqualTo(predicatePart));
                }
            });
        }

        [Test]
        public void Parse_DuplicateXPaths_AddsDuplicateDiagnostic()
        {
            // Arrange
            _parser.NonErrorDiagnostics.Clear();
            string duplicateXPath = "/root/test1|/root/test1";

            // Act
            var result = _parser.Parse(duplicateXPath);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(_parser.NonErrorDiagnostics, Has.Count.EqualTo(1));
                Assert.That(_parser.NonErrorDiagnostics[0].Descriptor, Is.EqualTo(XPathReader.Diagnostics.ContainsDuplicates));

                object[]? args = _parser.NonErrorDiagnostics[0].Args;
                Assert.That(args, Is.Not.Null);
                Assert.That(args, Has.Length.EqualTo(1));

                if (args is not null)
                {
                    Assert.That(args[0], Is.EqualTo("/root/test1"));
                }
            }
        }

        [TestCase("//root", "Only absolute XPaths are supported.")]
        [TestCase("/root/child::child", "Axis specifiers are not supported.")]
        [TestCase("/root/*", "The part '*' is not supported.")]
        [TestCase("/root/text()", "The part 'text()' is not supported.")]
        [TestCase("/root/node()", "The part 'node()' is not supported.")]
        [TestCase("/root/@attr", "The attribute selection part '@attr' is not supported.")]

        public void Parse_UnsupportedXPath_ThrowsException(string xpath, string expectedMessage)
        {
            // Act & Assert
            var ex = Assert.Throws<UnsupportedXPathException>(() => _parser.Parse(xpath));
            Assert.That(ex.Message, Does.Contain(expectedMessage));
        }

        [TestCase("/root/<invalid>", "Expression must evaluate to a node-set.")]
        public void Parse_InvalidXPath_ThrowsException(string xpath, string expectedMessage)
        {
            // Act & Assert
            var ex = Assert.Throws<InvalidXPathException>(() => _parser.Parse(xpath));
            Assert.That(ex.Message, Does.Contain(expectedMessage));
        }

        [Test]
        [TestCase("/root/child|/different/child")]
        public void Parse_DifferentRoots_ThrowsException(string xPaths)
        {
            var ex = Assert.Throws<UnsupportedXPathException>(() => _parser.Parse(xPaths));
            Assert.That(ex.Message, Does.Contain("different root"));
        }

        [Test]
        [TestCase("/root|/root/child")]
        public void Parse_ConflictingDepths_ThrowsException(string xPaths)
        {
            var ex = Assert.Throws<UnsupportedXPathException>(() => _parser.Parse(xPaths));
            Assert.That(ex.Message, Does.Contain("deeper path"));
        }

        [Test]
        public void Parse_DiagnosticsAreClearedBetweenCalls()
        {
            // Arrange
            _parser.Parse("/root/test[1]"); // Should add a diagnostic
            Assert.That(_parser.NonErrorDiagnostics, Has.Count.EqualTo(1));

            // Act
            _parser.Parse("/root/simple"); // Should clear diagnostics

            // Assert
            Assert.That(_parser.NonErrorDiagnostics, Is.Empty);
        }


        private static List<string> GetAllXPaths(XPathTree tree)
        {
            var result = new List<string>();
            CollectXPaths(tree.Root, result);
            return result;
        }

        private static void CollectXPaths(XPathTreeElement element, List<string> xPaths)
        {
            if (element is XPathTreeLeafElement leaf)
            {
                xPaths.AddRange(leaf.RequestedXPaths);
            }
            else if (element is XPathTreeElementWithChildren withChildren)
            {
                foreach (var child in withChildren.Children)
                {
                    CollectXPaths(child, xPaths);
                }
            }
        }
    }
}
