using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace XPathReader.Common
{
    /// <summary>
    /// Contains extensions for <see cref="IEnumerable{ReadResult}"/> and <see cref="IAsyncEnumerable{ReadResult}"/>.
    /// </summary>
    public static class IEnumerableOfReadResultExtensions
    {
        /// <summary>
        /// Converts a sequence of <see cref="ReadResult"/> to a sequence of <see cref="PersistedReadResult"/>.
        /// This allows to store and process results after the <see cref="ReadResult"/> can no longer be used, like in Single() or First() functions.
        /// </summary>
        /// <param name="readResults">Enumeration of <see cref="ReadResult"/>.</param>
        /// <returns>Enumeration of <see cref="PersistedReadResult"/>.</returns>
        public static IEnumerable<PersistedReadResult> ToPersisted(this IEnumerable<ReadResult> readResults)
        {
            return readResults.Select(result => result.ToPersistedResult());
        }

        /// <summary>
        /// Asynchronously converts a sequence of <see cref="ReadResult"/> to a sequence of <see cref="PersistedReadResult"/>.
        /// This allows to store and process results after the <see cref="ReadResult"/> can no longer be used, like in Single() or First() functions.
        /// </summary>
        /// <param name="readResults">Enumeration of <see cref="ReadResult"/>.</param>
        /// <param name="cancellationToken">A token to cancel XML read operation.</param>
        /// <returns>Enumeration of <see cref="PersistedReadResult"/>.</returns>
        public static async IAsyncEnumerable<PersistedReadResult> ToPersistedAsync(this IAsyncEnumerable<ReadResult> readResults, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (ReadResult readResult in readResults.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                yield return await readResult.ToPersistedResultAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
