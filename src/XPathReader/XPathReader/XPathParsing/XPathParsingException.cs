using System;
using System.Collections.Generic;
using System.Text;

namespace XPathReader.XPathParsing
{
    public class XPathParsingException : Exception
    {
        public XPathParsingException()
        {
        }

        public XPathParsingException(string message) : base(message)
        {
        }

        public XPathParsingException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
