using SharpBoostVoronoi.Parabolas;

namespace SharpBoostVoronoi.Exceptions
{
    public interface IParabolaException
    {
         ParabolaProblemInformation InputParabolaProblemInfo { get; set; }
    }
}
