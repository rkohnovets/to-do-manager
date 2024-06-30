using auth_server.DTOs;
using auth_server.Models;
using auth_server.Repositories;
using auth_server.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mime;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace auth_server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private IUserRepository _userRepository;

    public AuthController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    // POST api/auth/login
    [HttpPost("login")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorDTO>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Login([FromBody] LoginDTO dto)
    {
        var userRepo = _userRepository;

        var users = await userRepo.GetAllAsync();

        var thisUser = users.FirstOrDefault(u => u.Username == dto.Username);
        if (thisUser is null)
        {
            return BadRequest(new ErrorDTO(1, "Пользователь с таким юзернеймом не найден"));
        }

        var passwordHash = CustomPasswordHasher.Hash(dto.Password);
        if (passwordHash != thisUser.PasswordHash)
        {
            return BadRequest(new ErrorDTO(2, "Пароль не совпал"));
        }

        return Ok(GenerateJWT(thisUser));
    }

    // POST api/auth/register
    [HttpPost("register")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorDTO>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Register([FromBody] RegisterDTO dto)
    {
        // TODO: validation
        // password...
        // email...
        // username...

        var passwordHash = CustomPasswordHasher.Hash(dto.Password);

        var newUser = new User()
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = passwordHash
        };

        var userRepo = _userRepository;

        var users = await userRepo.GetAllAsync();
        if (users.FirstOrDefault(u => u.Username == dto.Username) is not null)
        {
            return BadRequest(new ErrorDTO(1, "Данный юзернейм занят"));
        }
        if (users.FirstOrDefault(u => u.Email == dto.Email) is not null)
        {
            return BadRequest(new ErrorDTO(2, "Данная почта занята"));
        }

        await userRepo.AddAsync(newUser);

        users = await userRepo.GetAllAsync();
        newUser = users.First(u => u.Username == dto.Username);

        return Ok(GenerateJWT(newUser));
    }

    private string GenerateJWT(User user)
    {
        var claims = new List<Claim> {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var jwt = new JwtSecurityToken(
            issuer: AuthOptions.ISSUER,
            audience: AuthOptions.AUDIENCE,
            claims: claims,
            expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(45)),
            signingCredentials: new SigningCredentials(
                AuthOptions.GetSymmetricSecurityKey(),
                SecurityAlgorithms.HmacSha256
            )
        );

        var jwtTokenHandler = new JwtSecurityTokenHandler();

        return jwtTokenHandler.WriteToken(jwt);
    }
}
