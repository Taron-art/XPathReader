using System.Reflection;

namespace XPathReader
{
    internal sealed record MemberInfo(string Keyword, string Namespace, string Name)
    {
        public MemberInfo? Parent { get; set; }
    }
}
