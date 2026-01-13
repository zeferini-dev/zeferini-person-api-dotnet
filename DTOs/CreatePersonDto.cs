using System.ComponentModel.DataAnnotations;

namespace ZeferiniPersonApi.DTOs;

public class CreatePersonDto
{
    /// <summary>
    /// Full name of the person
    /// </summary>
    /// <example>Ada Lovelace</example>
    [Required]
    [StringLength(120, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Email address of the person
    /// </summary>
    /// <example>ada@example.com</example>
    [Required]
    [EmailAddress]
    [StringLength(180)]
    public string Email { get; set; } = string.Empty;
}
