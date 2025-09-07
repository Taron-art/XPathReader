using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using XPathReader.XPathParsing;

namespace XPathReader.Tests.XPathParsing
{
    [TestFixture]
    [TestOf(typeof(XPathReader.XPathParsing.XPathParser))]
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
        public void Parse_SingleXPath_CreatesCorrectTree(string xpath)
        {
            // Act
            var result = _parser.Parse(xpath);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(GetAllXPaths(result), Has.Member(xpath));
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
        }

        [Test]
        [TestCaseSource(nameof(MultipleXPathsTestCases))]
        public void Parse_MultipleXPaths_CreatesCorrectTree(string xpaths, XPathTree expected)
        {
            // Act
            XPathTree result = _parser.Parse(xpaths);

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
            Assert.That(GetAllXPaths(result).Count(), Is.EqualTo(2));
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

        private static IEnumerable<string> GetAllXPaths(XPathTree tree)
        {
            var result = new List<string>();
            CollectXPaths(tree.Root, result);
            return result;
        }

        private static void CollectXPaths(XPathTreeElement element, List<string> xpaths)
        {
            if (element is XPathTreeLeafElement leaf)
            {
                xpaths.AddRange(leaf.RequestedXPaths);
            }
            else if (element is XPathTreeElementWithChildren withChildren)
            {
                foreach (var child in withChildren.Children)
                {
                    CollectXPaths(child, xpaths);
                }
            }
        }


    }
}
