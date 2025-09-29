namespace DataAccess.Domain;

public class Product : BaseEntity
{
    public required string Title  { get; set; }
    public required string Description  { get; set; }
    public required string Link  { get; set; }
}