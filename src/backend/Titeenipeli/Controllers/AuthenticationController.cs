using Microsoft.AspNetCore.Mvc;
using Titeenipeli.Handlers;
using Titeenipeli.Inputs;
using Titeenipeli.Options;
using Titeenipeli.Schema;
using Titeenipeli.Services.Interfaces;

namespace Titeenipeli.Controllers;

[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly JwtOptions _jwtOptions;
    private readonly IUserService _userService;

    public AuthenticationController(JwtOptions jwtOptions, IUserService userService)
    {
        _jwtOptions = jwtOptions;
        _userService = userService;
    }

    [HttpPost("login")]
    public IActionResult PostLogin([FromBody] PostLoginInput loginInput)
    {
        // TODO: Actual login validation 
        if (loginInput is not { Username: "test", Password: "test123" })
        {
            return Unauthorized();
        }

        User? user = _userService.GetUserByCode(loginInput.Username);

        if (user == null)
        {
            return BadRequest();
        }

        JwtHandler jwtHandler = new JwtHandler(_jwtOptions);

        Response.Cookies.Append(_jwtOptions.CookieName, jwtHandler.GetJwtToken(user),
            jwtHandler.GetAuthorizationCookieOptions());

        return Ok();
    }
}