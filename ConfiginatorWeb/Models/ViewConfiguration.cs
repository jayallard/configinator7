using System.ComponentModel.DataAnnotations;

namespace ConfiginatorWeb.Models;

public class ViewConfiguration
{
    [Key]
    public string Name { get; set; }
    
    [Required]
    public string Path { get; set; }
    
    [DataType(DataType.MultilineText)]
    public string? Schema { get; set; }
}