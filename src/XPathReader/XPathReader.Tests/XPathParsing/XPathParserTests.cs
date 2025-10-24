using ARTX.XPathReader.XPathParsing;

namespace ARTX.XPathReader.Tests.XPathParsing
{
    [TestFixture]
    [TestOf(typeof(XPathParser))]
    public class XPathParserTests
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
            (XPathTree tree, _) = _parser.Parse(xPath);

            // Assert
            Assert.That(tree, Is.Not.Null);
            Assert.That(GetAllXPaths(tree), Has.Member(xPath));
        }

        private static IEnumerable<TestCaseData<string, XPathTree>> MultipleXPathsTestCases()
        {
            XPathTree expectedTree = new(
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

        [TestCaseSource(nameof(MultipleXPathsTestCases))]
        public void Parse_MultipleXPaths_CreatesCorrectTree(string xPaths, XPathTree expected)
        {
            // Act
            XPathTree result = _parser.Parse(xPaths).Tree;

            // Assert
            Assert.That(result, Is.EqualTo(expected).UsingPropertiesComparer());
        }

        [TestCase("/root/test1|/root/test1")]
        [TestCase("/root/test1\n/root/test1")]
        public void Parse_DuplicateXPaths_Duplicates(string xpaths)
        {
            // Act
            (XPathTree tree, _) = _parser.Parse(xpaths);

            // Assert
            Assert.That(GetAllXPaths(tree), Has.Count.EqualTo(2));
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

        [TestCase("/root/a[1]", "[1]")]
        [TestCase("/root/b[text()]", "[text()]")]
        [TestCase("/root/c[@attr]", "[@attr]")]
        public void Parse_XPathWithPredicate_AddsDiagnostic(string xpath, string predicatePart)
        {
            // Arrange
            _parser.NonErrorDiagnostics.Clear();

            // Act
            (XPathTree tree, _) = _parser.Parse(xpath);

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
            const string duplicateXPath = "/root/test1|/root/test1";

            // Act
            _ = _parser.Parse(duplicateXPath);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(_parser.NonErrorDiagnostics, Has.Count.EqualTo(1));
                Assert.That(_parser.NonErrorDiagnostics[0].Descriptor, Is.EqualTo(Diagnostics.ContainsDuplicates));

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
            UnsupportedXPathException? ex = Assert.Throws<UnsupportedXPathException>(() => _parser.Parse(xpath));
            Assert.That(ex.Message, Does.Contain(expectedMessage));
        }

        [TestCase("/root/<invalid>", "Expression must evaluate to a node-set.")]
        public void Parse_InvalidXPath_ThrowsException(string xpath, string expectedMessage)
        {
            // Act & Assert
            InvalidXPathException? ex = Assert.Throws<InvalidXPathException>(() => _parser.Parse(xpath));
            Assert.That(ex.Message, Does.Contain(expectedMessage));
        }

        [TestCase("/root/child|/different/child")]
        public void Parse_DifferentRoots_ThrowsException(string xPaths)
        {
            UnsupportedXPathException? ex = Assert.Throws<UnsupportedXPathException>(() => _parser.Parse(xPaths));
            Assert.That(ex.Message, Does.Contain("different root"));
        }

        [TestCase("/root|/root/child")]
        public void Parse_ConflictingDepths_ThrowsException(string xPaths)
        {
            UnsupportedXPathException? ex = Assert.Throws<UnsupportedXPathException>(() => _parser.Parse(xPaths));
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

        [Test]
        public void Parse_LongXPath_ProcessesSuccessfully()
        {

            string longXPath = "/root" + string.Concat(Enumerable.Range(1, XPathParser.MaxSupportedDepth - 1).Select(i => $"/child{i}"));

            (XPathTree tree, _) = _parser.Parse(longXPath);

            Assert.That(GetAllXPaths(tree), Has.Member(longXPath));
        }

        [Test]
        public void Parse_ToLongXPath_ThrowsUnsupportedException()
        {
            string longXPath = "/root" + string.Concat(Enumerable.Range(1, XPathParser.MaxSupportedDepth).Select(i => $"/child{i}"));

            UnsupportedXPathException? exception = Assert.Throws<UnsupportedXPathException>(() => _parser.Parse(longXPath));

            Assert.That(exception.Message, Does.Contain("maximum supported depth"));
            Assert.That(exception.XPath, Is.SameAs(longXPath));
        }


        private static List<string> GetAllXPaths(XPathTree tree)
        {
            List<string> result = [];
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
                foreach (XPathTreeElement child in withChildren.Children)
                {
                    CollectXPaths(child, xPaths);
                }
            }
        }
    }
}
