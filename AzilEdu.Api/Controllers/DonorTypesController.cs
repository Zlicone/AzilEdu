using AzilEdu.Api.Data;
using AzilEdu.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AzilEdu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Microsoft.AspNetCore.Authorization.Authorize(
    Policy = AzilEdu.Api.Security.AuthorizationPolicies.Staff)]
public class DonorTypesController : ControllerBase
{
    private readonly AzilEduDbContext _context;

    public DonorTypesController(AzilEduDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<LookupDto>>> GetDonorTypes()
    {
        var result = await _context.DonorTypes
            .OrderBy(t => t.Id)
            .Select(t => new LookupDto { Id = t.Id, Name = t.Name })
            .ToListAsync();

        return Ok(result);
    }
}