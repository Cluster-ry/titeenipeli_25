using Microsoft.AspNetCore.Mvc;
using Titeenipeli.Extensions;
using Titeenipeli.Inputs;
using Titeenipeli.Schema;
using Titeenipeli.Services;
using Titeenipeli.Services.RepositoryServices.Interfaces;

namespace Titeenipeli.Controllers;

[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly JwtService _jwtService;
    private readonly IUserRepositoryService _userRepositoryService;

    public AuthenticationController(JwtService jwtService, IUserRepositoryService userRepositoryService)
    {
        _jwtService = jwtService;
        _userRepositoryService = userRepositoryService;
    }

    [HttpPost("login")]
    public IActionResult PostLogin([FromBody] PostLoginInput loginInput)
    {
        // TODO: Actual login validation 
        if (loginInput is not { Username: "test", Password: "test123" })
        {
            return Unauthorized();
        }

        User? user = _userRepositoryService.GetByCode(loginInput.Username);

        if (user == null)
        {
            return BadRequest();
        }

        Response.Cookies.AppendJwtCookie(_jwtService, user);

        return Ok();
    }
}