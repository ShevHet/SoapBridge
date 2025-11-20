namespace IcutechTestApi.DTOs;

public class UserProfileDto
{
    public string? UserId { get; set; }
    public string? Login { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FullName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class UpdateProfileRequestDto
{
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

public class ChangePasswordRequestDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class ProfileResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public UserProfileDto? Profile { get; set; }
}

