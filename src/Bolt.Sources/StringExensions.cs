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

            return string.Compare(first, second, comparisonType: System.StringComparison.OrdinalIgnoreCase) == 0;
        }
    }
}