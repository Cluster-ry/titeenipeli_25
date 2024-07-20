using Microsoft.AspNetCore.Mvc;
using Titeenipeli.Handlers;
using Titeenipeli.Inputs;
using Titeenipeli.Options;
using Titeenipeli.Schema;
using Titeenipeli.Services.RepositoryServices.Interfaces;

namespace Titeenipeli.Controllers;

[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly JwtOptions _jwtOptions;
    private readonly IUserRepositoryService _userRepositoryService;

    public AuthenticationController(JwtOptions jwtOptions, IUserRepositoryService userRepositoryService)
    {
        _jwtOptions = jwtOptions;
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

        JwtHandler jwtHandler = new JwtHandler(_jwtOptions);

        Response.Cookies.Append(_jwtOptions.CookieName, jwtHandler.GetJwtToken(user),
            jwtHandler.GetAuthorizationCookieOptions());

        return Ok();
    }
}