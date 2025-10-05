namespace ARTX.XPathReader.XPathParsing
{
    public abstract class XPathReaderGenerationException : Exception
    {
        public XPathReaderGenerationException(string message, string xPath) : base(message)
        {
            XPath = xPath;
        }
        public string XPath { get; }
    }

    public class UnsupportedXPathException(string message, string xPath) : XPathReaderGenerationException(message, xPath);

    public class InvalidXPathException(string message, string xPath) : XPathReaderGenerationException(message, xPath);
}
