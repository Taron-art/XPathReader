using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace XPathReader.Common
{
    public readonly struct ReadResult
    {
        private readonly string[]? _requestedXPaths;
        private readonly string? _requestedXPath;


        public ReadResult(IXPathBuilder actualXPath, XmlReader nodeReader, string requestedXPath)
        {
            ActualXPath = actualXPath;
            NodeReader = nodeReader;
            _requestedXPath = requestedXPath;
        }

        public ReadResult(IXPathBuilder actualXPath, XmlReader nodeReader, string[] requestedXPaths)
        {
            if (requestedXPaths.Length == 0)
            {
                throw new ArgumentException("Value cannot be an empty collection.", nameof(requestedXPaths));
            }

            ActualXPath = actualXPath;
            NodeReader = nodeReader;
            _requestedXPaths = requestedXPaths;
        }


        public string[]? RequestedXPaths
        {
            get
            {
                return _requestedXPaths;
            }
        }

        public string RequestedXPath
        {
            get
            {
                return _requestedXPath ?? _requestedXPaths![0];
            }
        }

        public IXPathBuilder ActualXPath { get; }

        public XmlReader NodeReader { get; }
    }
}
