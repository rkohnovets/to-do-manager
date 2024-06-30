using auth_server.Models;

namespace auth_server.DTOs;

public class UserDTO
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }

    public UserDTO(int id, string username, string email)
    {
        Id = id;
        Username = username;
        Email = email;
    }

    public UserDTO(User user) : this(user.Id, user.Username, user.Email)
    {

    }
}
