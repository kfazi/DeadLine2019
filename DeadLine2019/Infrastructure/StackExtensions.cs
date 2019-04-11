namespace DeadLine2019.Infrastructure
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal static class StackExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryPop<T>(this Stack<T> stack, out T value)
        {
            if (stack.Count == 0)
            {
                value = default(T);
                return false;
            }

            value = stack.Pop();

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryPeek<T>(this Stack<T> stack, out T value)
        {
            if (stack.Count == 0)
            {
                value = default(T);
                return false;
            }

            value = stack.Peek();

            return true;
        }
    }
}