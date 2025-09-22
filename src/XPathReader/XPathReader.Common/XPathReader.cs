using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;
using XPathReader.Common.Internal;

namespace XPathReader.Common
{
    public abstract class XPathReader
    {
        private readonly ThreadSafeNameTable _nameTable = new();
        private XmlReaderSettings? _syncSettings;
        private XmlReaderSettings? _asyncSettings;

        public IEnumerable<ReadResult> Read(Stream input)
        {
            return Read(input, _syncSettings ??= CreateXmlSettings(false), null);
        }

        public IEnumerable<ReadResult> Read(Stream input, XmlReaderSettings xmlReaderSettings, XmlParserContext? inputContext)
        {
            if (xmlReaderSettings.NameTable is null)
            {
                xmlReaderSettings.NameTable = _nameTable;
            }
            XmlReader xmlReader = XmlReader.Create(input, xmlReaderSettings, inputContext);
            return ReadInternal(xmlReader);
        }

        public IAsyncEnumerable<ReadResult> ReadAsync(Stream input, CancellationToken cancellationToken = default)
        {
            return ReadAsync(input, _asyncSettings ??= CreateXmlSettings(true), null, cancellationToken);
        }

        public IAsyncEnumerable<ReadResult> ReadAsync(Stream input, XmlReaderSettings xmlReaderSettings, XmlParserContext? inputContext, CancellationToken cancellationToken = default)
        {
            if (xmlReaderSettings.NameTable is null)
            {
                xmlReaderSettings.NameTable = _nameTable;
            }
            XmlReader xmlReader = XmlReader.Create(input, xmlReaderSettings, inputContext);
            return ReadInternalAsync(xmlReader, cancellationToken);
        }


        protected abstract IEnumerable<ReadResult> ReadInternal(XmlReader reader);

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
