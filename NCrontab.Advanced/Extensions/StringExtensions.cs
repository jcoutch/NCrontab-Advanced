namespace NCrontab.Advanced.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNullOrWhiteSpace(this string value)
        {
            return (value == null || value.Trim().Length == 0);
        }
    }
}