using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Titeenipeli.Context;
using Titeenipeli.Models;
using Titeenipeli.Schema;

namespace Titeenipeli.Controllers;

[ApiController]
[Authorize]
public class CtfController : ControllerBase
{
    private readonly ApiDbContext _dbContext;

    public CtfController(ApiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost("ctf")]
    public IActionResult PostCtf([FromBody] Flag flag)
    {
        CtfFlag? ctfFlag = _dbContext.CtfFlags.FirstOrDefault(ctfFlag => ctfFlag.Flag == flag.FlagCode);

        if (ctfFlag == null)
        {
            return BadRequest();
        }

        return Ok();
    }
}