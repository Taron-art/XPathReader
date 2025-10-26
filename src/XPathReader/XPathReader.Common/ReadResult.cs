using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace ARTX.XPath
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
        private readonly XmlReader _nodeReader;

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
            _nodeReader = nodeReader;
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
            _nodeReader = nodeReader;
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
        /// By default is positioned on the Start Element.
        /// Please make sure that you read all necessary data before requesting the next result from Enumeration.
        /// </remarks>
        public XmlReader NodeReader
        {
            get
            {
                if (_nodeReader.NodeType == XmlNodeType.None)
                {
                    throw new InvalidOperationException("""
                        The XmlReader is not positioned on a node. Most likely it has been already closed. 
                        If seen using First() or Single() LINQ functions, please consider using ToPersistedResult or Select() from the NodeReader first.
                        """);
                }
                return _nodeReader;
            }
        }

        /// <summary>
        /// Converts the current <see cref="ReadResult"/> into a <see cref="PersistedReadResult"/> by fully reading the XML node.
        /// </summary>
        /// <remarks>
        /// If you want to preserve the result after the next read operation, you must call this method to read the node fully.
        /// </remarks>
        /// <returns>Instance of <seealso cref="PersistedReadResult"/>.</returns>
        public PersistedReadResult ToPersistedResult()
        {
            XElement element = XElement.Load(NodeReader);
            // Since we read the node fully, there is no more to read from there, so we just close it.
            _nodeReader.Close();
            return ToPersistedResultInternal(element);
        }

        /// <summary>
        /// Asynchronously converts the current <see cref="ReadResult"/> into a <see cref="PersistedReadResult"/> by fully reading the XML node.
        /// </summary>
        /// <remarks>
        /// If you want to preserve the result after the next read operation, you must call this method to read the node fully.
        /// </remarks>
        /// <param name="cancellationToken">A token to cancel XML read operation.</param>
        /// <returns>Instance of <seealso cref="PersistedReadResult"/>.</returns>
        public async Task<PersistedReadResult> ToPersistedResultAsync(CancellationToken cancellationToken = default)
        {
#if NETSTANDARD2_1_OR_GREATER
            XElement element = await XElement.LoadAsync(NodeReader, LoadOptions.None, cancellationToken).ConfigureAwait(false);
#else
            XElement element = XElement.Load(new StringReader(await NodeReader.ReadOuterXmlAsync().ConfigureAwait(false)), LoadOptions.None);
#endif
            // Since we read the node fully, there is no more to read from there, so we just close it.
            _nodeReader.Close();
            return ToPersistedResultInternal(element);
        }

        private PersistedReadResult ToPersistedResultInternal(XElement xElement)
        {
            if (_requestedXPaths is not null)
            {
                return new PersistedReadResult(ActualXPath.GetXPath(), xElement, _requestedXPaths);
            }
            else
            {
                return new PersistedReadResult(ActualXPath.GetXPath(), xElement, RequestedXPath);
            }
        }
    }
}
