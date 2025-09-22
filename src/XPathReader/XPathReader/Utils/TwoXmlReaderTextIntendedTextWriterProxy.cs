namespace XPathReader.Utils
{
    public class TwoXmlReaderTextIntendedTextWriterProxy : IXmlReaderTextIntendedTextWriter
    {
        private readonly IXmlReaderTextIntendedTextWriter _first;
        private readonly IXmlReaderTextIntendedTextWriter _second;

        public TwoXmlReaderTextIntendedTextWriterProxy(
            IXmlReaderTextIntendedTextWriter first,
            IXmlReaderTextIntendedTextWriter second)
        {
            _first = first ?? throw new ArgumentNullException(nameof(first));
            _second = second ?? throw new ArgumentNullException(nameof(second));
        }

        public void Write(char value)
        {
            _first.Write(value);
            _second.Write(value);
        }

        public void Write(string s)
        {
            _first.Write(s);
            _second.Write(s);
        }

        public void WriteLine()
        {
            _first.WriteLine();
            _second.WriteLine();
        }

        public void WriteLine(char value)
        {
            _first.WriteLine(value);
            _second.WriteLine(value);
        }

        public void WriteLine(string s)
        {
            _first.WriteLine(s);
            _second.WriteLine(s);
        }

        public void WriteLineMethodSignature(string modifiers, string methodName)
        {
            _first.WriteLineMethodSignature(modifiers, methodName);
            _second.WriteLineMethodSignature(modifiers, methodName);
        }
        public void WriteLineMoveToContent(string? readerName = null)
        {
            _first.WriteLineMoveToContent(readerName);
            _second.WriteLineMoveToContent(readerName);
        }

        public void WriteLineRead()
        {
            _first.WriteLineRead();
            _second.WriteLineRead();
        }

        public void WriteLineSkip()
        {
            _first.WriteLineSkip();
            _second.WriteLineSkip();
        }

        public void CloseBrace(bool withSemicolon = false)
        {
            _first.CloseBrace(withSemicolon);
            _second.CloseBrace(withSemicolon);
        }

        public void OpenBrace()
        {
            _first.OpenBrace();
            _second.OpenBrace();
        }
    }
}