namespace DeadLine2019.Infrastructure
{
    using System.Linq;

    public static class SolutionDirectory
    {
        public static string Path { get; set; }

        public static string Get(params string[] parts)
        {
            return System.IO.Path.Combine(new[] { Path }.Concat(parts).ToArray());
        }
    }
}