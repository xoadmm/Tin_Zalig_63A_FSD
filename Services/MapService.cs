using FullstackHA.Models;

namespace FullstackHA.Services;

public class MapService : IMapService
{
    private Graph? _graph;

    public void SetMap(Graph graph)
    {
        _graph = graph;
    }

    public Graph? GetMap()
    {
        return _graph;
    }

    public bool HasMap()
    {
        return _graph != null;
    }

    public string GetShortestRoute(string fromId, string toId)
    {
        if (_graph == null)
            throw new InvalidOperationException("Map has not been set");

        var path = FindShortestPath(fromId, toId);
        return string.Join("", path);
    }

    public int GetShortestDistance(string fromId, string toId)
    {
        if (_graph == null)
            throw new InvalidOperationException("Map has not been set");

        var (distance, _) = CalculateShortestPath(fromId, toId);
        return distance;
    }

    private List<string> FindShortestPath(string fromId, string toId)
    {
        var (_, previous) = CalculateShortestPath(fromId, toId);

        // Reconstruct path from previous nodes
        var path = new List<string>();
        var current = toId;

        while (current != null)
        {
            path.Insert(0, current);
            current = previous.ContainsKey(current) ? previous[current] : null;
        }

        return path;
    }

    private (int distance, Dictionary<string, string> previous) CalculateShortestPath(string fromId, string toId)
    {
        if (_graph == null)
            throw new InvalidOperationException("Map has not been set");

        // Validate that nodes exist
        if (!_graph.Nodes.Any(n => n.Id == fromId))
            throw new ArgumentException($"Node '{fromId}' does not exist in the graph");

        if (!_graph.Nodes.Any(n => n.Id == toId))
            throw new ArgumentException($"Node '{toId}' does not exist in the graph");

        // Build adjacency list for the graph (bi-directional)
        var adjacencyList = new Dictionary<string, List<(string nodeId, int weight)>>();
        foreach (var node in _graph.Nodes)
        {
            adjacencyList[node.Id] = new List<(string, int)>();
        }

        foreach (var edge in _graph.Edges)
        {
            // Since edges are bi-directional, add both directions
            adjacencyList[edge.FromId].Add((edge.ToId, edge.Weight));
            adjacencyList[edge.ToId].Add((edge.FromId, edge.Weight));
        }

        // Dijkstra's algorithm initialization
        var distances = new Dictionary<string, int>();
        var previous = new Dictionary<string, string>();
        var unvisited = new HashSet<string>();

        foreach (var node in _graph.Nodes)
        {
            distances[node.Id] = int.MaxValue;
            unvisited.Add(node.Id);
        }
        distances[fromId] = 0;

        // Main algorithm loop
        while (unvisited.Count > 0)
        {
            // Find unvisited node with smallest distance
            string? current = null;
            int smallestDistance = int.MaxValue;

            foreach (var nodeId in unvisited)
            {
                if (distances[nodeId] < smallestDistance)
                {
                    smallestDistance = distances[nodeId];
                    current = nodeId;
                }
            }

            if (current == null || distances[current] == int.MaxValue)
                break; // No more reachable nodes

            // If we reached the destination, we can stop
            if (current == toId)
                break;

            unvisited.Remove(current);

            // Update distances to neighbors
            foreach (var (neighborId, weight) in adjacencyList[current])
            {
                if (!unvisited.Contains(neighborId))
                    continue;

                int alternativeDistance = distances[current] + weight;

                if (alternativeDistance < distances[neighborId])
                {
                    distances[neighborId] = alternativeDistance;
                    previous[neighborId] = current;
                }
            }
        }

        if (distances[toId] == int.MaxValue)
            throw new InvalidOperationException($"No path exists between {fromId} and {toId}");

        return (distances[toId], previous);
    }
}