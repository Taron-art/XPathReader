using System.Diagnostics;

namespace XPathReader.XPathParsing
{
    public abstract class XPathTreeElement
    {
        public XPathLevelIdentifier Identifier { get; }

        public XPathTreeElement(XPathLevelIdentifier identifier)
        {
            Identifier = identifier;
        }
    }

    [DebuggerDisplay("Identifier: {Identifier}, Children Count = {Children.Count}")]
    public sealed class XPathTreeElementWithChildren : XPathTreeElement
    {
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public List<XPathTreeElement> Children { get; } = new();

        public XPathTreeElementWithChildren(XPathLevelIdentifier identifier) : base(identifier)
        {
        }
    }

    [DebuggerDisplay("Identifier: {Identifier}, First Reqested Path = {RequestedXPaths[0]}")]
    public sealed class XPathTreeLeafElement : XPathTreeElement
    {
        public XPathTreeLeafElement(XPathLevelIdentifier identifier, string requestedXPath) : base(identifier)
        {
            RequestedXPaths = [requestedXPath];
        }

        public List<string> RequestedXPaths { get; }
    }
}
