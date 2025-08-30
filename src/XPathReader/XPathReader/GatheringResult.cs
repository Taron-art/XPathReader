namespace XPathReader
{
    internal sealed record GatheringResult(XPathReaderDataToGenerate? XPathToGenerate, DiagnosticData? DiagnosticData);

    internal sealed record DiagnosticData;
}
