namespace ARTX.XPathReader.Utils
{
    internal static class ArraySegmentExtensions
    {
        /// <summary>
        /// Emulates the Slice method for ArraySegment in .NET Standard.
        /// </summary>
        public static ArraySegment<T> Slice<T>(this ArraySegment<T> segment, int offset)
        {
            if (offset < 0 || offset > segment.Count)
                throw new ArgumentOutOfRangeException(nameof(offset));

            return new ArraySegment<T>(segment.Array, segment.Offset + offset, segment.Count - offset);
        }

        public static T GetItem<T>(this ArraySegment<T> segment, int index)
        {
            if (index < 0 || index >= segment.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return segment.Array[segment.Offset + index];
        }
    }
}
