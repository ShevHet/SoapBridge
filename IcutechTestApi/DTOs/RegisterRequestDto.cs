namespace IcutechTestApi.DTOs;

public class RegisterRequestDto
{
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

