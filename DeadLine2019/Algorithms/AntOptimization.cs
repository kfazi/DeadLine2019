namespace DeadLine2019.Algorithms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DeadLine2019.Infrastructure;

    public class AntOptimization<TNode>
    {
        public class State
        {
            public Dictionary<TNode, float> PheromoneNodes { get; set; }

            public Random Random { get; set; }

            public IEqualityComparer<TNode> EqualityComparer { get; set; }

            public float Alpha { get; set; }

            public float Beta { get; set; }

            public float Rho { get; set; }

            public float Q { get; set; }
        }

        public class Ant
        {
            private readonly State _state;

            public Ant(State state)
            {
                _state = state;
            }

            public List<TNode> VisitedNodes { get; } = new List<TNode>();

            public TNode GetNextNode(Func<TNode, IEnumerable<TNode>> getNeighbors, Func<TNode, TNode, float> getCost)
            {
                var currentNode = VisitedNodes.Last();

                var neighbors = getNeighbors(currentNode).Where(x => !VisitedNodes.Contains(x, _state.EqualityComparer));

                var probabilitiesMap = neighbors.Select(x => new Tuple<TNode, float>(x, getCost(currentNode, x))).Where(x => !float.IsInfinity(x.Item2)).ToLookup(x => Math.Pow(_state.PheromoneNodes[x.Item1], _state.Alpha) / Math.Pow(x.Item2, _state.Beta), x => x.Item1);

                var probabilitiesSum = probabilitiesMap.Sum(x => x.Key);

                var randomProbability = _state.Random.NextDouble() * probabilitiesSum;

                var selectedNode = default(TNode);

                double probSum = 0;
                foreach (var (probability, node) in probabilitiesMap.SelectMany(x => x.Select(y => new KeyValuePair<double, TNode>(x.Key, y))).OrderBy(x => x.Key))
                {
                    if (probSum > randomProbability)
                    {
                        break;
                    }

                    probSum += probability;

                    selectedNode = node;
                }

                return selectedNode;
            }

            public void GoTo(TNode node)
            {
                VisitedNodes.Add(node);
                _state.PheromoneNodes[node] += _state.Q / VisitedNodes.Count;
            }
        }

        private readonly State _state;

        public AntOptimization(
            IEnumerable<TNode> nodes,
            float alpha,
            float beta,
            float rho,
            float q,
            Random random,
            IEqualityComparer<TNode> equalityComparer)
        {
            _state = new State
            {
                PheromoneNodes = nodes.ToDictionary(x => x, x => 0.01f, equalityComparer),
                EqualityComparer = equalityComparer,
                Random = random,
                Alpha = alpha,
                Beta = beta,
                Rho = rho,
                Q = q
            };
        }

        public IDictionary<TNode, float> Pheromones => _state.PheromoneNodes;

        public IEnumerable<Ant> CreateAnts(int amount)
        {
            return Enumerable.Range(0, amount).Select(x => new Ant(_state));
        }

        public void EvaporatePheromone()
        {
            foreach (var pheromoneNode in _state.PheromoneNodes.Keys.ToList())
            {
                _state.PheromoneNodes[pheromoneNode] *= 1.0f - _state.Rho;
            }
        }
    }
}