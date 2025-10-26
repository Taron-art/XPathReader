using Microsoft.CodeAnalysis;

namespace XPathReader
{
    internal static class Diagnostics
    {
        public static readonly DiagnosticDescriptor LicenseViolation = new(
            id: "XPR001",
            title: "Possible license agreement violation detected",
            messageFormat: "Please read the license agreement. Code won't be emmitted.",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );
    }
}
