using System.Text;

namespace XPathReader.Common.Internal
{
    /// <summary>
    /// Provides a utility for building XPath expressions dynamically.
    /// </summary>
    public class XPathBuilder : IXPathBuilder
    {
        private readonly StringBuilder _builder = new();

        /// <summary>
        /// Length of the underlying XPath string. Allows to truncate the XPath.
        /// </summary>
        public int Length
        {
            get { return _builder.Length; }
            set { _builder.Length = value; }
        }

        /// <summary>
        /// Adds a new level for a builder.
        /// </summary>
        /// <param name="localName">Local name of a node.</param>
        /// <param name="index">Index of a node.</param>
        public void AddLevel(string localName, int index = 0)
        {
            if (index == 0)
            {
                _builder.AppendFormat("/{0}", localName);
            }
            else
            {
                _builder.AppendFormat("/{0}[{1}]", localName, index);
            }
        }

        /// <inheritdoc/>
        public string GetXPath()
        {
            return _builder.ToString();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return GetXPath();
        }
    }
}
