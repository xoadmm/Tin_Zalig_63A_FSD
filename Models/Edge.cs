namespace FullstackHA.Models
{
    public class Edge
    {
        public string FromId { get; set; } = string.Empty;
        public string ToId { get; set; } = string.Empty;
        public int Weight { get; set; }
    }
}
