namespace ARTX.XPathReader.XPathParsing
{
    public sealed class XPathTree
    {
        public XPathTree(XPathTreeElement root)
        {
            Root = root ?? throw new ArgumentNullException(nameof(root));
        }

        public XPathTreeElement Root { get; }

        /// <summary>
        /// Enumerates local names (Identifier.LocalName) for every element in the XPath tree.
        /// </summary>
        /// <returns>IEnumerable of local name strings.</returns>
        public IEnumerable<string> GetLocalNames()
        {
            Queue<XPathTreeElement> queue = new Queue<XPathTreeElement>();
            queue.Enqueue(Root);

            HashSet<string> seen = new();

            while (queue.Count > 0)
            {
                XPathTreeElement node = queue.Dequeue();

                string name = node.Identifier.LocalName;
                if (seen.Add(name))
                {
                    yield return name;
                }

                if (node is XPathTreeElementWithChildren parent)
                {
                    foreach (XPathTreeElement child in parent.Children)
                    {
                        if (child != null) queue.Enqueue(child);
                    }
                }
            }
        }
    }
}
