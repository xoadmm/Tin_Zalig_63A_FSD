namespace FullstackHA.Models
{
    public class Graph
    {
        public List<Node> Nodes { get; set; } = new List<Node> ();
        public List<Edge> Edges { get; set;} = new List<Edge> ();
    }
}
