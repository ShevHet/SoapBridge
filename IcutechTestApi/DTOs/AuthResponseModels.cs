namespace IcutechTestApi.DTOs;

public class AuthResponse<T>
{
    public bool Success { get; set; }
    public T? Entity { get; set; }
}

public class RegisterResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? CreatedCustomerId { get; set; }
}

public class AuthErrorResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

