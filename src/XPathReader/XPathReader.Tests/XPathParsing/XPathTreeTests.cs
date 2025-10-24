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
            XPathTreeLeafElement root = new(new XPathLevelIdentifier("root"), "/root");
            XPathTree tree = new(root);

            // Act
            List<string> names = tree.GetLocalNames().ToList();

            // Assert
            Assert.That(names, Is.EqualTo(["root"]));
        }

        [Test]
        public void GetLocalNames_MultipleNodes_ReturnsUniqueNames()
        {
            // Arrange
            XPathTreeElementWithChildren root = new(new XPathLevelIdentifier("root"));

            // first child is a leaf named "child"
            XPathTreeLeafElement childLeaf = new(new XPathLevelIdentifier("child"), "/root/child");
            root.Children.Add(childLeaf);

            // second child is a parent node named "parent"
            XPathTreeElementWithChildren parentNode = new(new XPathLevelIdentifier("parent"));
            // add a child with the same local name "child" (duplicate)
            parentNode.Children.Add(new XPathTreeLeafElement(new XPathLevelIdentifier("child"), "/root/parent/child"));
            // add another distinct child "grandchild"
            parentNode.Children.Add(new XPathTreeLeafElement(new XPathLevelIdentifier("grandchild"), "/root/parent/grandchild"));

            root.Children.Add(parentNode);

            XPathTree tree = new(root);

            // Act
            List<string> names = tree.GetLocalNames().ToList();

            // Assert
            Assert.That(names, Is.EqualTo(["root", "child", "parent", "grandchild"]));
        }
    }
}
