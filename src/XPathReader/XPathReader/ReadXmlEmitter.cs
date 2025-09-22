using System.Text;
using System.Xml;
using Medallion.Collections;
using XPathReader.Utils;
using XPathReader.XPathParsing;

namespace XPathReader
{
    internal class ReadXmlEmitter
    {
        private const string NameOfTheXPathBuilder = "currentXPathBuilder";

        private int _variableIndex = 0;

        private readonly string _modifiers;
        private readonly XPathTree _tree;
        private readonly VariableStorage _variableStorage = new();

        public ReadXmlEmitter(XPathTree tree, string modifiers = "protected override")
        {
            _modifiers = modifiers;
            _tree = tree;
        }

        public void GenerateCode(IXmlReaderTextIntendedTextWriter writer)
        {
            Generate(writer);
        }

        private void Generate(IXmlReaderTextIntendedTextWriter writer)
        {
            writer.WriteLineMethodSignature(_modifiers, "ReadInternal");
            writer.OpenBrace();
            writer.WriteLine("using (reader)");
            writer.OpenBrace();
            WriteNameTableInitialization(writer);

            writer.WriteLineMoveToContent();
            writer.WriteLine($"XPathBuilder {NameOfTheXPathBuilder} = new XPathBuilder();");
            writer.WriteLine("while (!reader.EOF)");
            writer.OpenBrace();

            XPathTreeElement[] array = { _tree.Root };
            GenerateForNode(array, writer);
            writer.CloseBrace();
            writer.CloseBrace();
            writer.CloseBrace();
        }

        private void WriteNameTableInitialization(IXmlReaderTextIntendedTextWriter writer)
        {
            IEnumerable<string> allNames = _tree.GetLocalNames();

            foreach (string name in allNames)
            {
                writer.WriteLine($"string {_variableStorage.Add(name)} = reader.NameTable.Add(\"{name}\");");
            }
        }

        private void GenerateForNode(IReadOnlyCollection<XPathTreeElement> elements, IXmlReaderTextIntendedTextWriter writer, string? parentNameVariable = null)
        {
            IGrouping<XPathLevelIdentifier, XPathTreeElement>[] groupsByName = elements.GroupBy(element => element.Identifier).ToArray();

            bool isRoot = false;
            if (elements.Count == 1 && elements.First() == _tree.Root)
            {
                isRoot = true;
            }

            string[] counterVariables = new string[groupsByName.Length];
            for (int i = 0; i < groupsByName.Length; i++)
            {
                counterVariables[i] = $"counter{++_variableIndex:x8}";
                writer.WriteLine($"int {counterVariables[i]} = 0;");
            }

            writer.WriteLine();

            if (parentNameVariable != null)
            {
                writer.WriteLine(
                    $"while (reader.NodeType != XmlNodeType.EndElement || !ReferenceEquals(reader.LocalName, {parentNameVariable}))");
                writer.OpenBrace();
            }

            writer.WriteLine("if (reader.NodeType == XmlNodeType.Element)");
            writer.OpenBrace();
            int index = 0;

            for (int i = 0; i < groupsByName.Length; i++)
            {
                var group = groupsByName[i];
                string originalLengthVariableName = $"originalLength{++_variableIndex:x8}";

                writer.WriteLine(
                    $"{(i == 0 ? "" : "else ")}if (ReferenceEquals(reader.LocalName, {_variableStorage[group.Key.LocalName]}))");
                writer.OpenBrace();

                writer.WriteLine($"++{counterVariables[index]};");
                writer.WriteLine($"int {originalLengthVariableName} = {NameOfTheXPathBuilder}.Length;");

                if (isRoot)
                {
                    // No predicate for the root.
                    writer.WriteLine($"{NameOfTheXPathBuilder}.AddLevel(\"{group.Key.LocalName}\");");
                    // Check that we have only one root.
                    writer.WriteLine($"if ({counterVariables[index]} != 1)");
                    writer.OpenBrace();
                    writer.WriteLine("throw new XmlException(\"More than one root found in the document.\");");
                    writer.CloseBrace();
                }
                else
                {
                    writer.WriteLine($"{NameOfTheXPathBuilder}.AddLevel(\"{group.Key.LocalName}\", {counterVariables[index]});");
                }

                foreach (XPathTreeElement element in group)
                {
                    if (element is XPathTreeElementWithChildren elementWithChildren)
                    {
                        GenerateForNode(elementWithChildren.Children, writer, _variableStorage[group.Key.LocalName]);
                    }
                    else if (element is XPathTreeLeafElement leaf)
                    {
                        writer.WriteLine();
                        writer.WriteLine("XmlReader localReader = reader.ReadSubtree();");
                        writer.WriteLineMoveToContent("localReader");
                        if (leaf.RequestedXPaths.Count > 1)
                        {
                            StringBuilder typeList = new();
                            typeList.Append("new string[] { ");
                            bool first = true;
                            foreach (string xpath in leaf.RequestedXPaths)
                            {
                                if (!first)
                                {
                                    typeList.Append(", ");
                                }
                                typeList.Append($"\"{xpath}\"");
                                first = false;
                            }
                            typeList.Append(" }");
                            writer.WriteLine($"yield return new ReadResult({NameOfTheXPathBuilder}, localReader, {typeList});");
                        }
                        else
                        {
                            writer.WriteLine($"yield return new ReadResult({NameOfTheXPathBuilder}, localReader, \"{leaf.RequestedXPaths[0]}\");");
                        }

                        writer.WriteLine("localReader.Dispose();");
                        writer.WriteLine();
                    }
                }

                writer.WriteLine($"{NameOfTheXPathBuilder}.Length = {originalLengthVariableName};");
                writer.CloseBrace();
                index++;
            }

            if (parentNameVariable != null)
            {
                writer.WriteLine($"else if (!ReferenceEquals(reader.LocalName, {parentNameVariable}))");
                writer.OpenBrace();
                writer.WriteLineSkip();
                writer.CloseBrace();
            }

            AppendElseReadXml(writer);
            writer.CloseBrace();
            AppendElseReadXml(writer);
            if (parentNameVariable != null)
            {
                writer.CloseBrace();
            }
        }

        private static void AppendElseReadXml(IXmlReaderTextIntendedTextWriter builder)
        {
            builder.WriteLine("else");
            builder.OpenBrace();

            builder.WriteLineRead();
            builder.CloseBrace();
        }
    }
}
