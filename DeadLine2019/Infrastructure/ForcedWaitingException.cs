namespace DeadLine2019.Infrastructure
{
    using System;

    public class ForcedWaitingException : Exception
    {
        public ForcedWaitingException(int waitTime)
        {
            WaitTime = waitTime;
        }

        public int WaitTime { get; }
    }
}