using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;
using ARTX.XPath.Internal;

namespace ARTX.XPath
{
    /// <summary>
    /// Represents immutable, thread-safe XML reader based on the provided XPaths.
    /// </summary>
    public abstract class XPathReader
    {
        private readonly ThreadSafeNameTable _nameTable = new();
        private XmlReaderSettings? _syncSettings;
        private XmlReaderSettings? _asyncSettings;

        /// <summary>
        /// Reads XML data from the provided stream and yields XPath matches.
        /// </summary>
        /// <param name="input">
        /// The stream that contains the XML data. The <see cref="XmlReader"/> scans the first
        /// bytes of the stream looking for a byte order mark or other sign of encoding.
        /// When encoding is determined, the encoding is used to continue reading the stream,
        /// and processing continues parsing the input as a stream of (Unicode) characters.
        /// </param>
        /// <returns>An enumerable collection of <see cref="ReadResult"/> containing matched XPath nodes.</returns>
        /// <remarks>
        /// This method uses default XML reader settings with comments ignored.
        /// The name table is thread-safe and shared across all reads.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown if the input stream is null.</exception>
        /// <exception cref="XmlException">Incorrect XML encountered in the input stream.</exception>
        /// <exception cref="InvalidOperationException">An <see cref="XmlReader"/> method was called before a previous asynchronous operation finished.</exception>
        public IEnumerable<ReadResult> Read(Stream input)
        {
            return Read(input, _syncSettings ??= CreateXmlSettings(false), null);
        }

        /// <summary>
        /// Reads XML data from the provided stream and yields XPath matches.
        /// </summary>
        /// <param name="input">
        /// The stream that contains the XML data. The <see cref="XmlReader"/> scans the first
        /// bytes of the stream looking for a byte order mark or other sign of encoding.
        /// When encoding is determined, the encoding is used to continue reading the stream,
        /// and processing continues parsing the input as a stream of (Unicode) characters.
        /// </param>
        /// <param name="xmlReaderSettings">Custom XML reader settings for controlling the XML parsing behavior.</param>
        /// <param name="inputContext">The XML parser context for custom XML parsing scenarios.</param>
        /// <returns>An enumerable collection of <see cref="ReadResult"/> containing matched XPath nodes.</returns>
        /// <remarks>
        /// If the provided XML reader settings don't have a name table assigned,
        /// the reader's thread-safe name table will be used.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="input"/> or <paramref name="xmlReaderSettings"/> is null.</exception>
        /// <exception cref="XmlException">Incorrect XML encountered in the input stream.</exception>
        /// <exception cref="InvalidOperationException">An <see cref="XmlReader"/> method was called before a previous asynchronous operation finished.</exception>
        public IEnumerable<ReadResult> Read(Stream input, XmlReaderSettings xmlReaderSettings, XmlParserContext? inputContext)
        {
            if (xmlReaderSettings is null)
            {
                throw new ArgumentNullException(nameof(xmlReaderSettings));
            }

            if (xmlReaderSettings.NameTable is null && inputContext?.NameTable is null)
            {
                xmlReaderSettings.NameTable = _nameTable;
            }

            XmlReader xmlReader = XmlReader.Create(input, xmlReaderSettings, inputContext);
            return ReadInternal(xmlReader);
        }

        /// <summary>
        /// Reads XML data from the provided <see cref="XmlReader"/> and yields XPath matches.
        /// </summary>
        /// <remarks>
        /// Methods creates a sub-tree reader from the provided reader.
        /// </remarks>
        /// <param name="reader">Reader to read from. Reader must be positioned on an element.</param>
        /// <returns>An enumerable collection of <see cref="ReadResult"/> containing matched XPath nodes.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> is null.</exception>
        /// <exception cref="InvalidOperationException">An <see cref="XmlReader"/> method was called before a previous asynchronous operation finished.</exception>
        /// <exception cref="InvalidOperationException">The <paramref name="reader"/> isn't positioned on an element when this method is called.</exception>
        /// <exception cref="XmlException">Incorrect XML encountered in the input stream.</exception>
        public IEnumerable<ReadResult> ReadFromSubtree(XmlReader reader)
        {
            if (reader is null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            XmlReader subTreeReader = reader.ReadSubtree();
            return ReadInternal(subTreeReader);
        }

        /// <summary>
        /// Asynchronously reads XML data from the provided stream and yields XPath matches.
        /// </summary>
        /// <param name="input">
        /// The stream that contains the XML data. The <see cref="XmlReader"/> scans the first
        /// bytes of the stream looking for a byte order mark or other sign of encoding.
        /// When encoding is determined, the encoding is used to continue reading the stream,
        /// and processing continues parsing the input as a stream of (Unicode) characters.
        /// </param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>An asynchronous enumerable collection of <see cref="ReadResult"/> containing matched XPath nodes.</returns>
        /// <remarks>
        /// This method uses default XML reader settings with comments ignored and async reading enabled.
        /// The name table is thread-safe and shared across all reads.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown if the input stream is null.</exception>
        /// <exception cref="XmlException">Incorrect XML encountered in the input stream.</exception>
        /// <exception cref="InvalidOperationException">An <see cref="XmlReader"/> method was called before a previous asynchronous operation finished.</exception>
        public IAsyncEnumerable<ReadResult> ReadAsync(Stream input, CancellationToken cancellationToken = default)
        {
            return ReadAsync(input, _asyncSettings ??= CreateXmlSettings(true), null, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads XML data from the provided stream using custom XML reader settings.
        /// </summary>
        /// <param name="input">
        /// The stream that contains the XML data. The <see cref="XmlReader"/> scans the first
        /// bytes of the stream looking for a byte order mark or other sign of encoding.
        /// When encoding is determined, the encoding is used to continue reading the stream,
        /// and processing continues parsing the input as a stream of (Unicode) characters.
        /// </param>
        /// <param name="xmlReaderSettings">Custom XML reader settings for controlling the XML parsing behavior.</param>
        /// <param name="inputContext">The XML parser context for custom XML parsing scenarios.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>An asynchronous enumerable collection of <see cref="ReadResult"/> containing matched XPath nodes.</returns>
        /// <remarks>
        /// If the provided XML reader settings don't have a name table assigned,
        /// the reader's thread-safe name table will be used.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="input"/> or <paramref name="xmlReaderSettings"/> is null.</exception>
        /// <exception cref="XmlException">Incorrect XML encountered in the input stream.</exception>
        /// <exception cref="InvalidOperationException">An <see cref="XmlReader"/> method was called before a previous asynchronous operation finished.</exception>
        public IAsyncEnumerable<ReadResult> ReadAsync(Stream input, XmlReaderSettings xmlReaderSettings, XmlParserContext? inputContext, CancellationToken cancellationToken = default)
        {
            if (xmlReaderSettings is null)
            {
                throw new ArgumentNullException(nameof(xmlReaderSettings));
            }

            if (xmlReaderSettings.NameTable is null && inputContext?.NameTable is null)
            {
                xmlReaderSettings.NameTable = _nameTable;
            }

            XmlReader xmlReader = XmlReader.Create(input, xmlReaderSettings, inputContext);
            return ReadInternalAsync(xmlReader, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads XML data from the provided <see cref="XmlReader"/> and yields XPath matches.
        /// </summary>
        /// <remarks>
        /// Methods creates a sub-tree reader from the provided reader.
        /// </remarks>
        /// <param name="reader">Reader to read from. Reader must be positioned on an element.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>An enumerable collection of <see cref="ReadResult"/> containing matched XPath nodes.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> is null.</exception>
        /// <exception cref="InvalidOperationException">An <see cref="XmlReader"/> method was called before a previous asynchronous operation finished.</exception>
        /// <exception cref="InvalidOperationException">The <paramref name="reader"/> isn't positioned on an element when this method is called.</exception>
        /// <exception cref="XmlException">Incorrect XML encountered in the input stream.</exception>
        public IAsyncEnumerable<ReadResult> ReadFromSubtreeAsync(XmlReader reader, CancellationToken cancellationToken = default)
        {
            if (reader is null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            XmlReader subTreeReader = reader.ReadSubtree();
            return ReadInternalAsync(subTreeReader, cancellationToken);
        }

        /// <summary>
        /// When implemented in a derived class, reads XML data synchronously and yields XPath matches.
        /// </summary>
        /// <param name="reader">The XML reader instance to use for reading the XML data.</param>
        /// <returns>An enumerable collection of <see cref="ReadResult"/> containing matched XPath nodes.</returns>
        protected abstract IEnumerable<ReadResult> ReadInternal(XmlReader reader);

        /// <summary>
        /// When implemented in a derived class, reads XML data asynchronously and yields XPath matches.
        /// </summary>
        /// <param name="reader">The XML reader instance to use for reading the XML data.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>An asynchronous enumerable collection of <see cref="ReadResult"/> containing matched XPath nodes.</returns>
        protected abstract IAsyncEnumerable<ReadResult> ReadInternalAsync(XmlReader reader, CancellationToken cancellationToken);

        private XmlReaderSettings CreateXmlSettings(bool isAsync)
        {
            return new XmlReaderSettings
            {
                Async = isAsync,
                IgnoreComments = true,
            };
        }
    }
}
