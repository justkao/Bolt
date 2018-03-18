using System;

namespace Bolt
{
    public static class BoltSpanExtensions
    {
        public static string ConvertToString(this ReadOnlySpan<char> span)
        {
            return new string(span.ToArray());
        }

        public static bool AreEqualInvariant(this ReadOnlySpan<char> span, ReadOnlySpan<char> other)
        {
            if (span.Length != other.Length)
            {
                return false;
            }

            for (int i = 0; i < span.Length; i++)
            {
                char a = span[i];
                char b = other[i];

                if (a == b)
                {
                    continue;
                }

                if (char.ToLowerInvariant(a) == char.ToLowerInvariant(b))
                {
                    continue;
                }

                return false;
            }


            return true;
        }

        public static bool EndsWithInvariant(this ReadOnlySpan<char> span, ReadOnlySpan<char> ending)
        {
            if (span.Length < ending.Length)
            {
                return false;
            }

            return AreEqualInvariant(span.Slice(span.Length - ending.Length), ending);
        }
    }
}
