using System.ComponentModel.DataAnnotations;

namespace DataAccess.Domain;

public class Product : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string? Title  { get; set; }
    [Required]
    [StringLength(100)]
    public string? Description  { get; set; }
    [Required]
    [StringLength(100)]
    public string? Link  { get; set; }
}