using System.ComponentModel.DataAnnotations;

namespace Api.Dtos;

public class KwekInputDTO
{
    [Required]
    [StringLength(256, MinimumLength = 1)]
    public string Text { get; set; }
}