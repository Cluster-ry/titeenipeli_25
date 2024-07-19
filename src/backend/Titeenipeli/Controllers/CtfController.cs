using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Titeenipeli.Inputs;
using Titeenipeli.Schema;
using Titeenipeli.Services.Interfaces;

namespace Titeenipeli.Controllers;

[ApiController]
[Authorize]
public class CtfController : ControllerBase
{
    private readonly ICtfFlagService _ctfFlagService;

    public CtfController(ICtfFlagService ctfFlagService)
    {
        _ctfFlagService = ctfFlagService;
    }

    [HttpPost("ctf")]
    public IActionResult PostCtf([FromBody] PostCtfInput ctfInput)
    {
        CtfFlag? ctfFlag = _ctfFlagService.GetByToken(ctfInput.Token);

        if (ctfFlag == null)
        {
            return BadRequest();
        }

        return Ok();
    }
}