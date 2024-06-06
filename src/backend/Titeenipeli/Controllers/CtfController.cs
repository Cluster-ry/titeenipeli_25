using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Titeenipeli.Context;
using Titeenipeli.Models;

namespace Titeenipeli.Controllers;

[ApiController, Authorize]
public class CtfController : ControllerBase
{
    private readonly ApiDbContext _context;

    public CtfController(ApiDbContext context)
    {
        _context = context;
    }
    
    [HttpPost("ctf/{flag}")]
    public IActionResult PostCtf(string flag)
    {
        CtfFlag? ctfFlag = _context.Flags!.FirstOrDefault(ctfFlag => ctfFlag.Flag == flag);

        if (ctfFlag == null)
        {
            return BadRequest();
        }
        
        return Ok();
    }
}