using System.ComponentModel.DataAnnotations;

namespace DataAccess.Domain;

public class Product : BaseEntity
{
    [Required]
    [StringLength(100)]
    public required string Title  { get; set; }
    [Required]
    [StringLength(100)]
    public required string Description  { get; set; }
    [Required]
    [StringLength(100)]
    public required string Link  { get; set; }
    [Required]
    [StringLength(100)]
    public required string SiteName  { get; set; }
}