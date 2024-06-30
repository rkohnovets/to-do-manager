using auth_server.DTOs;
using auth_server.Models;
using auth_server.Repositories;
using auth_server.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace auth_server.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ProfileController : ControllerBase
{
    private IUserRepository _userRepository;

    public ProfileController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    // GET api/profile
    [HttpGet]
    [ProducesResponseType<UserDTO>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorDTO>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> GetProfile()
    {
        var user = await GetUserByHttpContext();
        if (user == null)
        {
            // спорный момент, пользователю нет смысла видеть такое сообщение
            return BadRequest(new ErrorDTO(1, "Пользователь с таким Id (из JWT) не найден"));
        }

        return Ok(new UserDTO(user));
    }

    // POST api/profile
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorDTO>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdateProfile([FromBody] UserUpdateDTO dto)
    {
        if (dto.Username == null && dto.Email == null && dto.Password == null)
        {
            return BadRequest(new ErrorDTO(0, "Нет полей для обновления"));
        }

        var user = await GetUserByHttpContext();
        if (user == null)
        {
            // спорный момент, пользователю нет смысла видеть такое сообщение
            return BadRequest(new ErrorDTO(1, "Пользователь с таким Id (из JWT) не найден"));
        }

        var users = await _userRepository.GetAllAsync();
        if (dto.Username != null)
        {
            if (users.Any(u => u.Username == dto.Username && u.Id != user.Id))
            {
                return BadRequest(new ErrorDTO(2, "Данный юзернейм занят"));
            }
            user.Username = dto.Username;
        }

        if (dto.Email != null)
        {
            if (users.Any(u => u.Email == dto.Email && u.Id != user.Id))
            {
                return BadRequest(new ErrorDTO(3, "Данная почта занята"));
            }
            user.Email = dto.Email;
        }

        if (dto.Password != null)
        {
            var passwordHash = CustomPasswordHasher.Hash(dto.Password);
            user.PasswordHash = passwordHash;
        }

        await _userRepository.UpdateAsync(user);

        return Ok();
    }

    private async Task<User?> GetUserByHttpContext()
    {
        var idStr = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var id = int.Parse(idStr);

        var userRepo = _userRepository;

        return await userRepo.GetByIdAsync(id);
    }
}
