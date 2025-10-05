using System.Xml;

namespace ARTX.XPath.Internal
{
    /// <summary>
    /// Thread safe implementation of <see cref="NameTable"/>.
    /// </summary>
    public sealed class ThreadSafeNameTable : NameTable
    {
        private readonly object _lock = new();

        ///<inheritdoc/>
        public override string Add(char[] key, int start, int len)
        {
            // First, check if the key already exists without locking for performance
            string? result = Get(key, start, len);

            if (result is null)
            {
                lock (_lock)
                {
                    return base.Add(key, start, len);
                }
            }

            return result;
        }

        ///<inheritdoc/>
        public override string Add(string key)
        {
            // First, check if the key already exists without locking for performance
            string? result = Get(key);

            if (result is null)
            {
                lock (_lock)
                {
                    return base.Add(key);
                }
            }

            return result;
        }
    }
}
