using System;
using System.Collections.Generic;
using System.Text;

namespace XPathReader.XPathParsing
{
    internal class UnsupportedXPathException : Exception
    {
        public UnsupportedXPathException()
        {
        }

        public UnsupportedXPathException(string message) : base(message)
        {
        }

        public UnsupportedXPathException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
