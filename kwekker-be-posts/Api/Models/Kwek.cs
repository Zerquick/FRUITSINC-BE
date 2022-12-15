using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public class Kwek
{
    public int Id { get; set; }
    
    
    [Required]
    public string Text { get; set; }
    
    public DateTime PostedAt { get; set; }
}