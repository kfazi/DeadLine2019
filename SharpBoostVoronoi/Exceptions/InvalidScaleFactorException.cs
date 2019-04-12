using System;

namespace SharpBoostVoronoi.Exceptions
{
    public class InvalidScaleFactorException : Exception
    {
        public InvalidScaleFactorException()
        {

        }

        public InvalidScaleFactorException(string message)
            : base(message)
        {
        }

        public InvalidScaleFactorException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
