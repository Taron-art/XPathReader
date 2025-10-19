using System;
using System.Diagnostics;
using System.Xml.Linq;

namespace ARTX.XPath
{
    /// <summary>
    /// Represents the result of XML read operation, including the actual XPath encountered during the operation and the
    /// requested XPath(s) that guided the read. Comparing to <seealso cref="ReadResult"/> is class with a permanent data.
    /// </summary>
    [DebuggerDisplay("Requested XPath: {RequestedXPath}, Node : {Node}")]
    public sealed record PersistedReadResult
    {
        private readonly string[]? _requestedXPaths;
        private readonly string? _requestedXPath;


        /// <summary>
        /// Creates a new instance of PersistedReadResult.
        /// </summary>
        /// <param name="actualXPath">The actual XPath encountered during the read operation.</param>
        /// <param name="node">The <see cref="XElement"/> with the XML node associated with the result.</param>
        /// <param name="requestedXPath">The XPath expression that was requested for the read operation.</param>
        /// <param name="elementLocalName">Found element local name.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="actualXPath"/>, <paramref name="node"/>, <paramref name="requestedXPath"/>.</exception>
        public PersistedReadResult(string actualXPath, XElement node, string requestedXPath, string elementLocalName)
        {
            if (actualXPath is null)
            {
                throw new ArgumentNullException(nameof(actualXPath));
            }

            if (node is null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            if (requestedXPath is null)
            {
                throw new ArgumentNullException(nameof(requestedXPath));
            }

            if (elementLocalName is null)
            {
                throw new ArgumentNullException(nameof(elementLocalName));
            }

            ActualXPath = actualXPath;
            Node = node;
            _requestedXPath = requestedXPath;
            ElementLocalName = elementLocalName;
        }

        /// <summary>
        /// Creates a new instance of PersistedReadResult.
        /// </summary>
        /// <param name="actualXPath">The actual XPath encountered during the read operation.</param>
        /// <param name="node">The <see cref="XElement"/> with the XML node associated with the result.</param>
        /// <param name="requestedXPaths">The XPath expressions that was requested for the read operation.</param>
        /// <param name="elementLocalName">Found element local name.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="actualXPath"/>, <paramref name="node"/>, <paramref name="requestedXPaths"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="requestedXPaths"/> is empty.</exception>
        public PersistedReadResult(string actualXPath, XElement node, string[] requestedXPaths, string elementLocalName)
        {
            if (actualXPath is null)
            {
                throw new ArgumentNullException(nameof(actualXPath));
            }

            if (node is null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            if (requestedXPaths is null)
            {
                throw new ArgumentNullException(nameof(requestedXPaths));
            }

            if (requestedXPaths.Length == 0)
            {
                throw new ArgumentException("Value cannot be an empty collection.", nameof(requestedXPaths));
            }

            if (elementLocalName is null)
            {
                throw new ArgumentNullException(nameof(elementLocalName));
            }

            ActualXPath = actualXPath;
            Node = node;
            _requestedXPaths = requestedXPaths;
            ElementLocalName = elementLocalName;
        }

        /// <summary>
        /// Found element local name.
        /// </summary>
        public string ElementLocalName { get; }

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
        public string ActualXPath { get; }

        /// <summary>
        /// The <see cref="XElement"/> with the result.
        /// </summary>
        public XElement Node { get; }
    }
}
