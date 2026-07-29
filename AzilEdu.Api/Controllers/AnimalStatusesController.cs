using AzilEdu.Api.Data;
using AzilEdu.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AzilEdu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnimalStatusesController : ControllerBase
{
    private readonly AzilEduDbContext _context;

    public AnimalStatusesController(AzilEduDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<LookupDto>>> GetAnimalStatuses()
    {
        var result = await _context.AnimalStatuses
            .OrderBy(s => s.Name)
            .Select(s => new LookupDto
            {
                Id = s.Id,
                Name = s.Name
            })
            .ToListAsync();

        return Ok(result);
    }
}