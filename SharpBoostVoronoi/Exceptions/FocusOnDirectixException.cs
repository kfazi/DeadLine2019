﻿using SharpBoostVoronoi.Parabolas;
using System;

namespace SharpBoostVoronoi.Exceptions
{
    public class FocusOnDirectixException : Exception,IParabolaException
    {
        public ParabolaProblemInformation InputParabolaProblemInfo { get; set; }

        public FocusOnDirectixException(ParabolaProblemInformation parabolaProblemInformation)
        {
            InputParabolaProblemInfo = parabolaProblemInformation;
        }

        public FocusOnDirectixException(string message)
            : base(message)
        {
        }

        public FocusOnDirectixException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
