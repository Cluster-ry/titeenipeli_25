using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Common.Enums;
using Titeenipeli.Common.Results;
using Titeenipeli.Inputs;

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