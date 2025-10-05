using System.Runtime.CompilerServices;

namespace ARTX.XPathReader.Tests
{
    public static class Initializer
    {
        [ModuleInitializer]
        public static void Init() => VerifySourceGenerators.Initialize();
    }
}
