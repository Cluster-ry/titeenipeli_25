using Microsoft.AspNetCore.Mvc;
using Titeenipeli.Handlers;
using Titeenipeli.Models;

namespace Titeenipeli.Controllers;

[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthenticationController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("login")]
    public IActionResult PostLogin([FromBody] Login user)
    {
        if (user is not { Username: "test", Password: "test123" })
        {
            return Unauthorized();
        }


        return Ok(new LoginHandler(_configuration).GetJwtToken());
    }
}