namespace auth_server.DTOs;

public class UserUpdateDTO
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
}
