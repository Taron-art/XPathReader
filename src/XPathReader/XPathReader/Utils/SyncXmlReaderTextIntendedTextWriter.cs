using XPathReader.Utils;

namespace ARTX.XPathReader.Utils
{
    public class SyncXmlReaderTextIntendedTextWriter : XmlReaderTextIntendedTextWriter
    {
        private readonly string _moveToContent;
        private readonly string _read;
        private readonly string _skip;

        public SyncXmlReaderTextIntendedTextWriter(TextWriter writer, string xmlReaderVariableName) : base(writer, xmlReaderVariableName)
        {
            _moveToContent = $"{_xmlReaderVariableName}.MoveToContent();";
            _read = $"{_xmlReaderVariableName}.Read();";
            _skip = $"{_xmlReaderVariableName}.Skip();";
        }

        public override void WriteLineMethodSignature(string modifiers, string methodName)
        {
            WriteLine($"{modifiers} IEnumerable<ReadResult> {methodName}(XmlReader {_xmlReaderVariableName})");
        }

        public override void WriteLineMoveToContent(string? readerName = null)
        {
            if (readerName is null)
            {
                WriteLine(_moveToContent);
            }
            else
            {
                WriteLine($"{readerName}.MoveToContent();");
            }
        }
        public override void WriteLineRead()
        {
            WriteLine(_read);
        }
        public override void WriteLineSkip()
        {
            WriteLine(_skip);
        }
    }
}
