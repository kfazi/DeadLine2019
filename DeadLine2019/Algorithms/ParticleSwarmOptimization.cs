namespace DeadLine2019.Algorithms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ParticleSwarmOptimization
    {
        public class Bound
        {
            public Bound(double min, double max)
            {
                Min = Math.Min(min, max);
                Max = Math.Max(min, max);
            }

            public double Max { get; }

            public double Min { get; }
        }

        public class Particle
        {
            private readonly Random _random;

            private readonly Func<IReadOnlyList<double>, double> _objectiveFunction;

            private readonly IReadOnlyList<Bound> _bounds;

            private readonly List<double> _position;

            private readonly List<double> _velocity;

            private readonly double _omega;

            private readonly double _phiP;

            private readonly double _phiG;

            private IReadOnlyList<double> _previousPosition;

            private IReadOnlyList<double> _bestPosition;

            private double _previousScore;

            private double _bestScore;

            public Particle(Random random, Func<IReadOnlyList<double>, double> objectiveFunction, IReadOnlyList<Bound> bounds, double omega, double phiP, double phiG)
            {
                _random = random;
                _objectiveFunction = objectiveFunction;
                _bounds = bounds;
                _omega = omega;
                _phiP = phiP;
                _phiG = phiG;

                _position = bounds.Select(x => x.Min + random.NextDouble() * (x.Max - x.Min)).ToList();
                _velocity = bounds.Select(x => 2.0 * (random.NextDouble() - 0.5) * (x.Max - x.Min)).ToList();
                _bestPosition = _position.ToList();
                Score = _objectiveFunction(_position);
                _bestScore = Score;
            }

            public IReadOnlyList<double> Position => _position.ToList();

            public double Score { get; private set; }

            public void Update(IReadOnlyList<double> swarmBestPosition)
            {
                for (var index = 0; index < _velocity.Count; index++)
                {
                    var rp = _random.NextDouble();
                    var rg = _random.NextDouble();
                    _velocity[index] = _omega * _velocity[index] + _phiP * rp * (_bestPosition[index] - _position[index]) +
                                       _phiG * rg * (swarmBestPosition[index] - _position[index]);
                }

                _previousPosition = _position.ToList();
                for (var index = 0; index < _position.Count; index++)
                {
                    _position[index] += _velocity[index];
                    _position[index] = Math.Min(_bounds[index].Max, Math.Max(_bounds[index].Min, _position[index]));
                }

                _previousScore = Score;
                Score = _objectiveFunction(_position);
                if (Score < _bestScore)
                {
                    return;
                }

                _bestScore = Score;
                _bestPosition = _position.ToList();
            }

            public bool Finished(double minImprovement, double minMovementSquared)
            {
                if (_previousPosition == null)
                {
                    return false;
                }

                var movementSquared = 0.0;
                for (var index = 0; index < _previousPosition.Count; index++)
                {
                    var difference = _position[index] - _previousPosition[index];
                    movementSquared += difference * difference;
                }

                var improvement = _previousScore - Score;

                return improvement < minImprovement && movementSquared < minMovementSquared;
            }
        }

        private readonly List<Particle> _particles;

        private double _bestScore;

        public ParticleSwarmOptimization(
            Random random,
            int swarmSize,
            Func<IReadOnlyList<double>, double> objectiveFunction,
            IReadOnlyList<Bound> bounds,
            double omega,
            double phiP,
            double phiG)
        {
            _particles = Enumerable.Range(0, swarmSize).Select(x => new Particle(random, objectiveFunction, bounds, omega, phiP, phiG)).ToList();
            _bestScore = double.NegativeInfinity;

            foreach (var particle in _particles)
            {
                UpdateBestValues(particle);
            }
        }

        public IReadOnlyList<Particle> Particles => _particles;

        public IReadOnlyList<double> BestPosition { get; private set; }

        public void Update()
        {
            foreach (var particle in _particles)
            {
                particle.Update(BestPosition);
                UpdateBestValues(particle);
            }
        }

        public bool Finished(double minImprovement, double minMovementSquared)
        {
            foreach (var particle in _particles)
            {
                if (!particle.Finished(minImprovement, minMovementSquared))
                {
                    return false;
                }
            }

            return true;
        }

        private void UpdateBestValues(Particle particle)
        {
            if (particle.Score < _bestScore)
            {
                return;
            }

            _bestScore = particle.Score;
            BestPosition = particle.Position;
        }
    }
}