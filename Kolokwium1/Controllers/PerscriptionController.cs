using Kolokwium1.DTOs;
using Kolokwium1.Services;
using Microsoft.AspNetCore.Mvc;

namespace Kolokwium1.Controllers;

[ApiController]
[Route("api/perscriptions")]
public class PerscriptionController (IDbService db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(string docName = null)
    {
        var result = await db.GetPerscriptions(docName);
        if (result is null) return NotFound($"Doc has not written any perscriptions.");
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Add(Perscription perscription)
    {
        var result = await db.AddPerscription(perscription);
        if (result is null) return BadRequest("Failed to add the prescription. Please check that data is valid.");
        return Ok(result);
    }
}