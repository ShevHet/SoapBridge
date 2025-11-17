namespace IcutechTestApi.Models;

public class RegisterResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? CreatedCustomerId { get; set; }
}

