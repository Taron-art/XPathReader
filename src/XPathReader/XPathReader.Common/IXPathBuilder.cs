namespace ARTX.XPath
{
    /// <summary>
    /// Represents a builder for constructing XPath expressions.
    /// </summary>
    public interface IXPathBuilder
    {
        /// <summary>
        /// Generates and returns the XPath expression that represents the current node's location in the XML document.
        /// </summary>
        /// <remarks>
        /// All nodes (except root) have indices in the XPath expression.
        /// </remarks>
        /// <returns>A string containing the XPath expression for the current node.</returns>
        string GetXPath();
    }
}