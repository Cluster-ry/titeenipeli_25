using Microsoft.AspNetCore.Mvc;
using Titeenipeli.Models;

namespace Titeenipeli.Controllers;

[Route("/api/")]
public class ApiController : ControllerBase
{
    [HttpGet("ping")]
    public string Get()
    {
        return "pong";
    }

    [HttpPost("echo")]
    public string PostEcho(string msg)
    {
        return msg;
    }

    [HttpPost("objectecho")]
    public TestObject PostObjectEcho([FromBody] TestObject testObject)
    {
        return testObject;
    }
}