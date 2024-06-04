using System.Data;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Titeenipeli.Connectors;

namespace Titeenipeli.Controllers;

[ApiController, Authorize]
public class CtfController : ControllerBase
{
    [HttpPost("ctf/{flag}")]
    public IActionResult PostCtf(string flag)
    {
        DbConnector connector = new DbConnector();
        NpgsqlCommand command;
        
        string sql = """
                     SELECT * FROM "CtfFlags"
                     WHERE flag = @flag
                     """;

        command = new NpgsqlCommand(sql, connector.GetConnection());
        command.Parameters.AddWithValue("@flag", flag);
        DataSet dataSet = connector.ExecuteCommand(command);

        if (dataSet.Tables[0].Rows.Count == 0)
        {
            return BadRequest();
        }

        return Ok();
    }
}