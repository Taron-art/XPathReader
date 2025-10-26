using Microsoft.CodeAnalysis;

namespace XPathReader
{
    internal static class Diagnostics
    {
        public static readonly DiagnosticDescriptor LicenseViolation = new(
            id: "XPR001",
            title: "Possible license agreement violation detected",
            messageFormat: "Please read the license agreement. Code won't be emitted.",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor InvalidArgument = new(
            id: "XPR002",
            title: "Invalid XPaths argument",
            messageFormat: "The argument '{0}' for the GeneratedXPathReader attribute is not a valid XPath: {1}",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor UnsupportedXPath = new(
            id: "XPR003",
            title: "The provided XPaths is unsupported",
            messageFormat: "XPath {0} is unsupported: {1}",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor XPathReaderMemberMustHaveValidSignature = new(
            id: "XPR004",
            title: "Invalid GeneratedXPathReaderAttribute usage",
            messageFormat: "GeneratedXPathReaderAttribute member must be partial, parameterless, non-generic, non-abstract, non-nullable and return XPathReader. Non-static members cannot be declared in an Interface or a readonly Struct. If a property, it must also be get-only.",
            category: "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            customTags: WellKnownDiagnosticTags.NotConfigurable);


        public static readonly DiagnosticDescriptor PredicateWillBeIgnored = new(
            id: "XPR005",
            title: "Predicate will be ignored",
            messageFormat: "Predicate {0} is not recognized and will be ignored",
            category: "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            customTags: WellKnownDiagnosticTags.CustomSeverityConfigurable);

        public static readonly DiagnosticDescriptor ContainsDuplicates = new(
            id: "XPR006",
            title: "GeneratedXPathReader attribute contains duplicates",
            messageFormat: "The argument for the GeneratedXPathReader attribute contains duplicated '{0}'",
            category: "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            customTags: WellKnownDiagnosticTags.CustomSeverityConfigurable);
    }
}
