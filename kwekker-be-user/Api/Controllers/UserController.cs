#nullable disable
using Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Api.Exceptions;
using Api.Services;

namespace Api.Controllers;

[ApiController]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet("users/{username}")]
    public async Task<ActionResult<UserOutputDTO>> GetUser(string username)
    {
        var user = await _userService.GetUserFromUsername(username);

        if (user == null)
        {
            return NotFound();
        }

        return new UserOutputDTO
        {
            Username = user.Username,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl
        };
    }

    [HttpPost("webhooks/new-user")]
    public async Task<IActionResult> ProcessNewUser(string userId)
    {
        try
        {
            await _userService.LoadAndCreateFromAuth0(userId);
        }
        catch (Auth0UserNotFoundException)
        {
            return BadRequest("Auth0 user not found");
        }
        catch (UserAlreadyExistsException)
        {
            return BadRequest("User already exists");
        }

        return Ok();
    }
}