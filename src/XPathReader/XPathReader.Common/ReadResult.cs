using System;
using System.Diagnostics;
using System.Xml;

namespace XPathReader.Common
{
    /// <summary>
    /// Represents the result of XML read operation, including the actual XPath encountered during the operation and the
    /// requested XPath(s) that guided the read.
    /// </summary>
    [DebuggerDisplay("Requested XPath: {RequestedXPath}, Node Reader: {NodeReader}")]
    public readonly struct ReadResult
    {
        private readonly string[]? _requestedXPaths;
        private readonly string? _requestedXPath;

        /// <summary>
        /// Represents the result of reading an XML node based on a requested XPath expression.
        /// </summary>
        /// <param name="actualXPath">The <see cref="IXPathBuilder"/> instance representing the actual XPath encountered during the read operation.</param>
        /// <param name="nodeReader">The <see cref="XmlReader"/> to read the XML node associated with the result.</param>
        /// <param name="requestedXPath">The XPath expression that was requested for the read operation.</param>
        public ReadResult(IXPathBuilder actualXPath, XmlReader nodeReader, string requestedXPath)
        {
            if (actualXPath is null)
            {
                throw new ArgumentNullException(nameof(actualXPath));
            }

            if (nodeReader is null)
            {
                throw new ArgumentNullException(nameof(nodeReader));
            }

            if (requestedXPath is null)
            {
                throw new ArgumentNullException(nameof(requestedXPath));
            }

            ActualXPath = actualXPath;
            NodeReader = nodeReader;
            _requestedXPath = requestedXPath;
        }

        /// <summary>
        /// Represents the result of reading an XML node based on a requested XPath expression.
        /// </summary>
        /// <param name="actualXPath">The <see cref="IXPathBuilder"/> instance representing the actual XPath encountered during the read operation.</param>
        /// <param name="nodeReader">The <see cref="XmlReader"/> to read the XML node associated with the result.</param>
        /// <param name="requestedXPaths">The XPath expressions that was requested for the read operation.</param>
        public ReadResult(IXPathBuilder actualXPath, XmlReader nodeReader, string[] requestedXPaths)
        {
            if (actualXPath is null)
            {
                throw new ArgumentNullException(nameof(actualXPath));
            }

            if (nodeReader is null)
            {
                throw new ArgumentNullException(nameof(nodeReader));
            }

            if (requestedXPaths is null)
            {
                throw new ArgumentNullException(nameof(requestedXPaths));
            }

            if (requestedXPaths.Length == 0)
            {
                throw new ArgumentException("Value cannot be an empty collection.", nameof(requestedXPaths));
            }

            ActualXPath = actualXPath;
            NodeReader = nodeReader;
            _requestedXPaths = requestedXPaths;
        }

        /// <summary>
        /// The XPath expressions that was requested for the read operation.
        /// </summary>
        /// <remarks>
        /// Return null if the read operation was based on a single XPath expression.
        /// </remarks>
        public string[]? RequestedXPaths
        {
            get
            {
                return _requestedXPaths;
            }
        }

        /// <summary>
        /// The XPath expression that was requested for the read operation.
        /// </summary>
        /// <remarks>
        /// Return first XPath expression if the read operation was based on multiple XPath expressions.
        /// </remarks>
        public string RequestedXPath
        {
            get
            {
                return _requestedXPath ?? _requestedXPaths![0];
            }
        }

        /// <summary>
        /// Actual XPath encountered during the read operation.
        /// </summary>
        public IXPathBuilder ActualXPath { get; }

        /// <summary>
        /// The <see cref="XmlReader"/> to read the XML node associated with the result.
        /// </summary>
        /// <remarks>
        /// Please make sure that you read all necessary data before requesting the next result from Enumeration.
        /// </remarks>
        public XmlReader NodeReader { get; }
    }
}
