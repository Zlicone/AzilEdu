using AzilEdu.Api.Data;
using AzilEdu.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AzilEdu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeeStatusesController : ControllerBase
{
    private readonly AzilEduDbContext _context;

    public EmployeeStatusesController(AzilEduDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<LookupDto>>> GetEmployeeStatuses()
    {
        var result = await _context.EmployeeStatuses
            .OrderBy(s => s.Id)
            .Select(s => new LookupDto { Id = s.Id, Name = s.Name })
            .ToListAsync();

        return Ok(result);
    }
}