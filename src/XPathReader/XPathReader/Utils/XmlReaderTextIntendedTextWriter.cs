namespace XPathReader.Utils
{

    public abstract class XmlReaderTextIntendedTextWriter : IntendedTextWriterExtended, IXmlReaderTextIntendedTextWriter
    {
        protected readonly string _xmlReaderVariableName;

        protected XmlReaderTextIntendedTextWriter(TextWriter writer, string xmlReaderVariableName) : base(writer)
        {
            _xmlReaderVariableName = xmlReaderVariableName;
        }

        public abstract void WriteLineMoveToContent(string? readerName = null);

        public abstract void WriteLineRead();

        public abstract void WriteLineSkip();

        public abstract void WriteLineMethodSignature(string modifiers, string methodName);
    }
}
