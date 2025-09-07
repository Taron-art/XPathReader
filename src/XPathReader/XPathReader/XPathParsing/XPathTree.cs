using System;
using System.Collections.Generic;
using System.Text;

namespace XPathReader.XPathParsing
{
    public sealed class XPathTree
    {
        public XPathTree(XPathTreeElement root)
        {
            Root = root ?? throw new ArgumentNullException(nameof(root));
        }

        public XPathTreeElement Root { get; }
    }
}
