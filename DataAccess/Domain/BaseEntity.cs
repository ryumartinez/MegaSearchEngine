namespace DataAccess.Domain;

public class BaseEntity 
{
    public int Id { get; set; }
    public required DateTime DateCreated { get; set; }
    public required DateTime DateUpdated { get; set; }
    public required string CreatedBy { get; set; }
    public required string UpdatedBy { get; set; }
}