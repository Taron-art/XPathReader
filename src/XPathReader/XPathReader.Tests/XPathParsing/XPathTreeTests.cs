using ARTX.XPathReader.XPathParsing;

namespace ARTX.XPathReader.Tests.XPathParsing
{
    [TestFixture]
    [TestOf(typeof(XPathTree))]
    public class XPathTreeTests
    {
        [Test]
        public void GetLocalNames_SingleNode_ReturnsLocalName()
        {
            // Arrange
            var root = new XPathTreeLeafElement(new XPathLevelIdentifier("root"), "/root");
            var tree = new XPathTree(root);

            // Act
            var names = tree.GetLocalNames().ToList();

            // Assert
            Assert.That(names, Is.EqualTo(new[] { "root" }));
        }

        [Test]
        public void GetLocalNames_MultipleNodes_ReturnsUniqueNames()
        {
            // Arrange
            var root = new XPathTreeElementWithChildren(new XPathLevelIdentifier("root"));

            // first child is a leaf named "child"
            var childLeaf = new XPathTreeLeafElement(new XPathLevelIdentifier("child"), "/root/child");
            root.Children.Add(childLeaf);

            // second child is a parent node named "parent"
            var parentNode = new XPathTreeElementWithChildren(new XPathLevelIdentifier("parent"));
            // add a child with the same local name "child" (duplicate)
            parentNode.Children.Add(new XPathTreeLeafElement(new XPathLevelIdentifier("child"), "/root/parent/child"));
            // add another distinct child "grandchild"
            parentNode.Children.Add(new XPathTreeLeafElement(new XPathLevelIdentifier("grandchild"), "/root/parent/grandchild"));

            root.Children.Add(parentNode);

            var tree = new XPathTree(root);

            // Act
            var names = tree.GetLocalNames().ToList();

            // Assert
            Assert.That(names, Is.EqualTo(new[] { "root", "child", "parent", "grandchild" }));
        }
    }
}
