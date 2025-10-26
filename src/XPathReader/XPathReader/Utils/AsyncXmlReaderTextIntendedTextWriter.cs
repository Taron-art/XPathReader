namespace XPathReader.Utils
{
    public class AsyncXmlReaderTextIntendedTextWriter : XmlReaderTextIntendedTextWriter
    {
        private const string CancellationTokenCheck = "cancellationToken.ThrowIfCancellationRequested();";

        private readonly string _moveToContent;
        private readonly string _read;
        private readonly string _skip;

        public AsyncXmlReaderTextIntendedTextWriter(TextWriter writer, string xmlReaderVariableName) : base(writer, xmlReaderVariableName)
        {
            _moveToContent = $"await {_xmlReaderVariableName}.MoveToContentAsync().ConfigureAwait(false);";
            _read = $"await {_xmlReaderVariableName}.ReadAsync().ConfigureAwait(false);";
            _skip = $"await {_xmlReaderVariableName}.SkipAsync().ConfigureAwait(false);";
        }

        public override void WriteLineMethodSignature(string modifiers, string methodName)
        {
            WriteLine($"{modifiers} async IAsyncEnumerable<ReadResult> {methodName}Async(XmlReader {_xmlReaderVariableName}, [EnumeratorCancellation]CancellationToken cancellationToken)");
        }

        public override void WriteLineMoveToContent(string? readerName = null)
        {
            WriteLine(CancellationTokenCheck);

            if (readerName is null)
            {
                WriteLine(_moveToContent);
            }
            else
            {
                WriteLine($"await {readerName}.MoveToContentAsync().ConfigureAwait(false);");
            }
        }

        public override void WriteLineRead()
        {
            WriteLine(CancellationTokenCheck);
            WriteLine(_read);
        }

        public override void WriteLineSkip()
        {
            WriteLine(CancellationTokenCheck);
            WriteLine(_skip);
        }
    }
}
