using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Titeenipeli.Database.Services.Interfaces;
using Titeenipeli.Enumeration;
using Titeenipeli.Inputs;
using Titeenipeli.Results;
using Titeenipeli.Schema;

namespace Titeenipeli.Controllers;

[ApiController]
[Authorize]
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

        if (ctfFlag != null)
        {
            return Ok();
        }

        ErrorResult error = new ErrorResult
        {
            Title = "Invalid flag",
            Code = ErrorCode.InvalidCtfFlag,
            Description = "Better luck next time"
        };

        return BadRequest(error);
    }
}