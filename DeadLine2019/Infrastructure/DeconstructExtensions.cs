namespace DeadLine2019.Infrastructure
{
    using System.Collections.Generic;

    public static class DeconstructExtensions
    {
        public static void Deconstruct<T1, T2>(this KeyValuePair<T1, T2> keyValuePair, out T1 key, out T2 value)
        {
            key = keyValuePair.Key;
            value = keyValuePair.Value;
        }
    }
}