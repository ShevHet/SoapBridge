namespace IcutechTestApi.DTOs;

public class RegisterResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? CreatedCustomerId { get; set; }
}

