using Microsoft.CodeAnalysis;

namespace XPathReader
{
    internal sealed record XPathReaderDataToGenerate(MemberInfo MethodInfo, bool IsProperty, Location DiagnosticLocation, string MemberName, string Modifiers, string XPaths, CompilationData CompilationData)
    {
        public string? GeneratedName { get; set; }
    }
}
