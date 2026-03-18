namespace NetReactProjectBackEnd.Models;

public class DataItem
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Created { get; set; }
}