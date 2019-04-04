namespace DeadLine2019.Infrastructure
{
    using System;

    public class FailedOperationException : Exception
    {
        public FailedOperationException(int code, string message)
            : base(message)
        {
            Code = code;
        }

        public int Code { get; }
    }
}