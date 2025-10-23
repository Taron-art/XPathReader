namespace ARTX.XPathReader.Utils
{
    public interface IXmlReaderTextIntendedTextWriter
    {
        void Write(char value);

        void Write(string s);

        void WriteLine(char value);

        void WriteLine(string s);

        void WriteLine();

        void WriteLineMethodSignature(string modifiers, string methodName);

        void WriteLineMoveToContent(string? readerName = null);

        void WriteLineRead();

        void WriteLineSkip();

        void CloseBrace(bool withSemicolon = false);

        void OpenBrace();
    }
}
