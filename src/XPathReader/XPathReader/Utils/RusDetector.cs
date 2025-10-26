using System.Globalization;
using System.Runtime.InteropServices;

namespace ARTX.XPathReader.Utils
{
    internal static class RusDetector
    {
        private static bool? _cachedValue;

        private static CultureInfo[] LanguagesToCheck
        {
            get
            {
                return
                [
                    new CultureInfo("ru-RU"),
                    new CultureInfo("tt-RU"),
                    new CultureInfo("ba-RU"),
                    new CultureInfo("cv-RU"),
                    new CultureInfo("ce-RU"),
                    new CultureInfo("mhr-RU"),
                    new CultureInfo("mrj-RU"),
                    new CultureInfo("udm-RU"),
                    new CultureInfo("sah-RU"),
                    new CultureInfo("krl-RU"),
                    new CultureInfo("nog-RU")
                ];
            }

        }

        public static bool Check()
        {
            if (!_cachedValue.HasValue)
            {
                CultureInfo[] languages = GetInputLanguges().ToArray();
                CultureInfo[] russianLanguagesFound = languages.Intersect(LanguagesToCheck).ToArray();
                _cachedValue = russianLanguagesFound.Length == 0 || !languages.Except(LanguagesToCheck).All(language => language.TwoLetterISOLanguageName.Equals("en", StringComparison.OrdinalIgnoreCase));
            }

            return (bool)_cachedValue;
        }

        private static IEnumerable<CultureInfo> GetInputLanguges()
        {
            int size = User32.GetKeyboardLayoutList(0, null);
            User32.HKL[] locales = new User32.HKL[size];
            User32.GetKeyboardLayoutList(size, locales);
            foreach (User32.HKL locale in locales)
            {
                yield return CultureInfo.GetCultureInfo(locale.LangId);
            }
        }

        private static class User32
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct HKL
            {
                private IntPtr _handle;

                public int LangId
                {
                    get
                    {
                        // LOWORD of HKL is the LANGID
                        return (ushort)((ulong)_handle.ToInt64() & 0xFFFF);
                    }
                }
            }

            [DllImport("user32.dll", SetLastError = false)]
            public static extern int GetKeyboardLayoutList(int nBuff, [Out] HKL[]? lpList);
        }
    }
}
