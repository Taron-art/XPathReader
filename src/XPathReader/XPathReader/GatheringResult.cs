using Microsoft.CodeAnalysis;

namespace XPathReader
{
    internal sealed record GatheringResult(XPathReaderDataToGenerate? XPathToGenerate, DiagnosticData? DiagnosticData);

    public sealed record DiagnosticData(DiagnosticDescriptor Descriptor, Location Location, params object[]? Args);
}
