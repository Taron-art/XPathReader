using System;
using System.Collections.Generic;
using System.Text;

namespace XPathReader.Common
{

    /// <summary>
    /// Instructs the source generator to generate an XPath reader method for the specified XPath expression.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class GeneratedXPathReaderAttribute : Attribute
    {
        public GeneratedXPathReaderAttribute(string xPath)
        {
            XPath = xPath;
        }

        public string XPath { get; }
    }
}
