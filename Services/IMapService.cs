using FullstackHA.Models;

namespace FullstackHA.Services
{
    public interface IMapService
    {
        void SetMap(Graph grah);
        Graph? GetMap();
        bool HasMap();
        string GetShortestRoute(string from, string to);
        int GetShortestDistance(string from, string to);
    }
}
