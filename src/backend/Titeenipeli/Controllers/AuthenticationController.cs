using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Titeenipeli.Context;
using Titeenipeli.Handlers;
using Titeenipeli.Inputs;
using Titeenipeli.Options;
using Titeenipeli.Schema;

namespace Titeenipeli.Controllers;

[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly ApiDbContext _dbContext;
    private readonly JwtOptions _jwtOptions;

    public AuthenticationController(JwtOptions jwtOptions, ApiDbContext dbContext)
    {
        _jwtOptions = jwtOptions;
        _dbContext = dbContext;
    }

    [HttpPost("login")]
    public IActionResult PostLogin([FromBody] PostLoginInput loginInput)
    {
        // TODO: Actual login validation 
        if (loginInput is not { Username: "test", Password: "test123" })
        {
            return Unauthorized();
        }

        User? user = _dbContext.Users.Include(user => user.Guild)
                               .FirstOrDefault(user => user.Code == loginInput.Username);

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