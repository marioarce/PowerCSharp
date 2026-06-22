namespace PowerCSharp.Extensions.Linq;

/// <summary>
/// Represents an ordering clause for dynamic expression parsing
/// </summary>
internal class OrderClause
{
    public string Property { get; set; } = string.Empty;
    public bool Descending { get; set; }
}
