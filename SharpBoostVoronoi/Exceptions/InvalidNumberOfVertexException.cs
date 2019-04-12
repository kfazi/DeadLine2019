using System;

namespace SharpBoostVoronoi.Exceptions
{
    class InvalidNumberOfVertexException : Exception
    {
        public InvalidNumberOfVertexException()
        {

        }

        public InvalidNumberOfVertexException(string message)
            : base(message)
        {
        }

        public InvalidNumberOfVertexException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
