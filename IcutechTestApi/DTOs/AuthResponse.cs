namespace IcutechTestApi.DTOs;

public class AuthResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Entity { get; set; }
}

