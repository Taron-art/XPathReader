using System.Diagnostics;
using System.Xml;
using System.Xml.XPath;
using ARTX.XPathReader.Utils;
using Microsoft.CodeAnalysis;

namespace ARTX.XPathReader.XPathParsing
{
    public class XPathParser
    {
        public static readonly byte MaxSupportedDepth = 64;

        public List<DiagnosticData> NonErrorDiagnostics { get; } = [];

        public (XPathTree Tree, HashSet<string> XPaths) Parse(string xPathsInOneString)
        {
            NonErrorDiagnostics.Clear();

            if (string.IsNullOrWhiteSpace(xPathsInOneString))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(xPathsInOneString));
            }

            string[] xPaths = xPathsInOneString.Split(['|', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries).Select(xpath => xpath.Trim()).ToArray();
            HashSet<string> uniqueXPaths = new();

            XPathTreeElement? root = null;

            char[] pathSplit = ['/'];

            for (int i = 0; i < xPaths.Length; i++)
            {
                ValidateXPath(xPaths[i]);

                string[] parts = xPaths[i].Split(pathSplit, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > MaxSupportedDepth)
                {
                    throw new UnsupportedXPathException($"The provided XPath exceeds the maximum supported depth of {MaxSupportedDepth}.", xPaths[i]);
                }

                XPathLevelIdentifier identifier = CreateIdentifier(parts[0], xPaths[i]);
                if (i == 0)
                {

                    root = parts.Length > 1
                        ? new XPathTreeElementWithChildren(identifier)
                        : new XPathTreeLeafElement(identifier, xPaths[i]); // Xpath has only one part, intersting but possible.
                }

                if (root!.Identifier != identifier)
                {
                    throw new UnsupportedXPathException($"This implementation already expects root '{root.Identifier}'. It is not possible to also expect '{xPaths[i]}' since it has a different root.", xPaths[i]);
                }

                if (parts.Length != 1)
                {
                    MergeIntoTree(root, new ArraySegment<string>(parts, 1, parts.Length - 1), xPaths[i]);
                }

                if (!uniqueXPaths.Add(xPaths[i]))
                {
                    NonErrorDiagnostics.Add(new DiagnosticData(Diagnostics.ContainsDuplicates, Location.None, xPaths[i]));
                }
            }

            if (root is null)
            {
                throw new XPathParsingException("Failed to extract single valid xpath from the argument");
            }

            return (new XPathTree(root), uniqueXPaths);
        }

        private static void ValidateXPath(string xPath)
        {
            try
            {
                XPathExpression xPathExpression = XPathExpression.Compile(xPath);
                Debug.Assert(xPathExpression.ReturnType == XPathResultType.NodeSet, "XPath return type is Node Set");
            }
            catch (XPathException ex)
            {
                throw new InvalidXPathException(ex.Message, xPath);
            }

            if (!xPath.StartsWith("/", StringComparison.Ordinal) || xPath.Contains("//"))
            {
                throw new UnsupportedXPathException("Only absolute XPaths are supported.", xPath);
            }

            if (xPath.Contains("::"))
            {
                throw new UnsupportedXPathException("Axis specifiers are not supported.", xPath);
            }
        }

        private void MergeIntoTree(XPathTreeElement element, ArraySegment<string> parts, string requestedXPath)
        {
            switch (element)
            {
                case XPathTreeLeafElement leaf:
                    if (parts.Count > 1 || CreateIdentifier(parts.GetItem(0), requestedXPath) != element.Identifier)
                    {
                        throw new UnsupportedXPathException($"This implementation already expects '{leaf.RequestedXPaths[0]}'. It is not possible to also expect '{requestedXPath}' since it is a deeper path.", requestedXPath);
                    }

                    leaf.RequestedXPaths.Add(requestedXPath);
                    break;
                case XPathTreeElementWithChildren withChildren:
                    XPathLevelIdentifier currentPart = CreateIdentifier(parts.GetItem(0), requestedXPath);
                    XPathTreeElement? matchingChild = withChildren.Children.FirstOrDefault(c => c.Identifier == currentPart);
                    bool isNewChild = false;
                    if (matchingChild is null)
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
                            throw new UnsupportedXPathException($"This implementation already expects deeper elements. It is not possible to also expect {requestedXPath} since it is a shallower element.", requestedXPath);
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

        private XPathLevelIdentifier CreateIdentifier(string xpathPart, string fullXPath)
        {
            int indexOfPredicate = xpathPart.IndexOf('[');
            if (indexOfPredicate >= 0)
            {
                // We ignore predicates for now.
                NonErrorDiagnostics.Add(new DiagnosticData(Diagnostics.PredicateWillBeIgnored, Location.None, xpathPart.Substring(indexOfPredicate)));
                xpathPart = xpathPart.Substring(0, indexOfPredicate);
            }

            ValidateXPathName(xpathPart, fullXPath);

            return new XPathLevelIdentifier(xpathPart);
        }

        private static void ValidateXPathName(string xpathPart, string fullXPath)
        {
            if (xpathPart.Contains('*') || xpathPart.Contains("text()") || xpathPart.Contains("node()"))
            {
                throw new UnsupportedXPathException($"The part '{xpathPart}' is not supported.", fullXPath);
            }

            if (xpathPart.Contains('@'))
            {
                throw new UnsupportedXPathException($"The attribute selection part '{xpathPart}' is not supported.", fullXPath);
            }

            if (!XmlReader.IsName(xpathPart))
            {
                throw new InvalidXPathException($"The part '{xpathPart}' is not a valid XML name.", fullXPath);
            }
        }
    }
}
