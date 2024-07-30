using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Titeenipeli.Inputs;
using Titeenipeli.Schema;
using Titeenipeli.Services.RepositoryServices.Interfaces;

namespace Titeenipeli.Controllers;

[ApiController]
[Authorize(Policy = "MustHaveGuild")]
public class CtfController : ControllerBase
{
    private readonly ICtfFlagRepositoryService _ctfFlagRepositoryService;

    public CtfController(ICtfFlagRepositoryService ctfFlagRepositoryService)
    {
        _ctfFlagRepositoryService = ctfFlagRepositoryService;
    }

    [HttpPost("ctf")]
    public IActionResult PostCtf([FromBody] PostCtfInput ctfInput)
    {
        CtfFlag? ctfFlag = _ctfFlagRepositoryService.GetByToken(ctfInput.Token);

        if (ctfFlag == null)
        {
            return BadRequest();
        }

        return Ok();
    }
}