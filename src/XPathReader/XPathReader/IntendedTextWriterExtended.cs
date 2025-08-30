using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace XPathReader
{
    internal class IntendedTextWriterExtended : IndentedTextWriter
    {
        public IntendedTextWriterExtended(TextWriter writer) : base(writer, "    ")
        {
        }

        public IntendedTextWriterExtended(TextWriter writer, string tabString) : base(writer, tabString)
        {
        }

        public void OpenBrace()
        {
            WriteLine('{');
            Indent++;
        }

        public void CloseBrace(bool withSemicolon = false)
        {
            Indent--;
            WriteLine(withSemicolon ? "};" : "}");
        }

        public SourceText ToSourceText()
        {
            Debug.Assert(Indent == 0, "Some braces may be not closed.");
            Flush();
            return SourceText.From(InnerWriter.ToString(), Encoding.UTF8);
        }
    }
}
