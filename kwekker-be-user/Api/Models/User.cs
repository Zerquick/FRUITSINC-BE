using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public class User
{
    public int Id { get; set; }
    
    [Required]
    public string ProviderId { get; set; }
    
    [Required]
    public string Username { get; set; }
    
    [Required]
    public string Email { get; set; }
    
    [Required]
    public string DisplayName { get; set; }

    [Required]
    public string AvatarUrl { get; set; }
}