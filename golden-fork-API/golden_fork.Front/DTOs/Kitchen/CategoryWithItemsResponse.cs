// CategoryWithItemsResponse.cs
using golden_fork.Front.DTOs.Kitchen;

public class CategoryWithItemsResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int ItemCount { get; set; }
    public List<ItemResponse> Items { get; set; } = new();  // or ItemResponse
}