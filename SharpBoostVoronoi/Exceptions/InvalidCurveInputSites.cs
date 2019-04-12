using System;

namespace SharpBoostVoronoi.Exceptions
{
    class InvalidCurveInputSites : Exception
    {
        public InvalidCurveInputSites()
        {

        }

        public InvalidCurveInputSites(string message)
            : base(message)
        {
        }

        public InvalidCurveInputSites(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
