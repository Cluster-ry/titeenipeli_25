using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Titeenipeli.Context;
using Titeenipeli.Handlers;
using Titeenipeli.Inputs;
using Titeenipeli.Models;
using Titeenipeli.Options;
using Titeenipeli.Schema;

namespace Titeenipeli.Controllers;

[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly JwtOptions _jwtOptions;
    private readonly ApiDbContext _dbContext;

    public AuthenticationController(JwtOptions jwtOptions, ApiDbContext dbContext)
    {
        _jwtOptions = jwtOptions;
        _dbContext = dbContext;
    }

    [HttpPost("login")]
    public IActionResult PostLogin([FromBody] PostLoginInput input)
    {
        // TODO: Actual login validation 
        if (input is not { Username: "test", Password: "test123" })
        {
            return Unauthorized();
        }

        User? user = _dbContext.Users.Include(user => user.Guild).FirstOrDefault(user => user.Code == input.Username);

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