using System;

namespace XPathReader.Common
{

    /// <summary>
    /// Instructs the source generator to generate an XPath reader method for the specified XPath expression.
    /// </summary>
    /// <example>
    /// [GeneratedXPathReader("/bookstore/book/title|/bookstore/book/author")]
    /// </example>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class GeneratedXPathReaderAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratedXPathReaderAttribute"/> class with the specified XPath
        /// expression.
        /// </summary>
        /// <param name="xPath">The XPath expression associated with this attribute. Multiple XPaths can be separated using '|' or new line characters.</param>
        public GeneratedXPathReaderAttribute(string xPath)
        {
            XPath = xPath;
        }

        /// <summary>
        /// Gets the XPath expression associated with this instance.
        /// </summary>
        public string XPath { get; }
    }
}
