using System.Runtime.CompilerServices;

namespace XPathReader.Tests
{
    public static class Initializer
    {
        [ModuleInitializer]
        public static void Init() => VerifySourceGenerators.Initialize();
    }
}
