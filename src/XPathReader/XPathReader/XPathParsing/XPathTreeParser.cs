using XPathReader.Utils;

namespace XPathReader.XPathParsing
{
    public class XPathParser
    {
        public XPathTree Parse(string xPathsInOneString)
        {
            if (string.IsNullOrWhiteSpace(xPathsInOneString))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(xPathsInOneString));
            }

            string[] xpaths = xPathsInOneString.Split(['|', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries).Select(xpath => xpath.Trim()).ToArray();

            XPathTreeElement? root = null;

            char[] pathSplit = ['/'];
            for (int i = 0; i < xpaths.Length; i++)
            {
                string[] parts = xpaths[i].Split(pathSplit, StringSplitOptions.RemoveEmptyEntries);

                if (i == 0)
                {
                    root = parts.Length > 1
                        ? new XPathTreeElementWithChildren(CreateIdentifier(parts[0]))
                        : new XPathTreeLeafElement(CreateIdentifier(parts[0]), xpaths[i]); // Xpath has only one part, intersting but possible.
                }

                if (root!.Identifier != CreateIdentifier(parts[0]))
                {
                    throw new UnsupportedXPathException($"This implementation already expects root '{root.Identifier}'. It is not possible to also expect '{xpaths[i]}' since it has a different root.");
                }

                if (parts.Length != 1)
                {
                    MergeIntoTree(root, new ArraySegment<string>(parts, 1, parts.Length - 1), xpaths[i]);
                }
            }

            if (root == null)
            {
                throw new XPathParsingException("Failed to extract single valid xpath from the argument");
            }

            return new XPathTree(root);
        }

        private void MergeIntoTree(XPathTreeElement element, ArraySegment<string> parts, string requestedXPath)
        {
            switch (element)
            {
                case XPathTreeLeafElement leaf:
                    if (parts.Count > 1 || CreateIdentifier(parts.GetItem(0)) != element.Identifier)
                    {
                        throw new UnsupportedXPathException($"This implementation already expects {leaf.RequestedXPaths}. It is not possible to also expect {requestedXPath} since it is a deeper element.");
                    }

                    leaf.RequestedXPaths.Add(requestedXPath);
                    break;
                case XPathTreeElementWithChildren withChildren:
                    XPathLevelIdentifier currentPart = CreateIdentifier(parts.GetItem(0));
                    XPathTreeElement? matchingChild = withChildren.Children.FirstOrDefault(c => c.Identifier == currentPart);
                    bool isNewChild = false;
                    if (matchingChild == null)
                    {
                        if (parts.Count == 1)
                        {
                            matchingChild = new XPathTreeLeafElement(currentPart, requestedXPath);
                        }
                        else
                        {
                            matchingChild = new XPathTreeElementWithChildren(currentPart);
                        }
                        withChildren.Children.Add(matchingChild);

                        isNewChild = true;
                    }

                    if (parts.Count == 1)
                    {
                        if (matchingChild is XPathTreeElementWithChildren)
                        {
                            throw new UnsupportedXPathException($"This implementation already expects deeper elements. It is not possible to also expect {requestedXPath} since it is a shallower element.");
                        }
                        if (!isNewChild)
                        {
                            ((XPathTreeLeafElement)matchingChild).RequestedXPaths.Add(requestedXPath);
                        }
                        break;
                    }
                    else
                    {
                        try
                        {
                            MergeIntoTree(matchingChild, parts.Slice(1), requestedXPath);
                        }
                        catch (Exception)
                        {
                            if (isNewChild)
                            {
                                withChildren.Children.Remove(matchingChild);
                            }
                            throw;
                        }
                    }
                    break;
                default:
                    throw new InvalidOperationException("Unknown XPathTreeElement type");
            }
        }

        private XPathLevelIdentifier CreateIdentifier(string xpathPart)
        {
            return new XPathLevelIdentifier(xpathPart);
        }
    }
}
