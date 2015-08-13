namespace Bolt
{
    internal static class StringExensions
    {
        public static bool EqualsNoCase(this string first, string second)
        {
            if (Equals(first, second))
            {
                return true;
            }

            return string.CompareOrdinal(first?.ToLowerInvariant() ?? first, second?.ToLowerInvariant() ?? second) == 0;
        }
    }
}