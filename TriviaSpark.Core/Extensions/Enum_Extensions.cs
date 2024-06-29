namespace TriviaSpark.Core.Extensions
{
    public static class Enum_Extensions
    {
        public static T ParseEnum<T>(this string value) where T : struct, Enum
        {
            if (!Enum.TryParse(value, true, out T result))
            {
                result = default(T);
            }
            return result;
        }
    }
}
