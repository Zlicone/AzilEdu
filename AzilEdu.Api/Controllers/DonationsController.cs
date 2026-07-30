using AzilEdu.Api.Data;
using AzilEdu.Shared.DTOs;
using AzilEdu.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AzilEdu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DonationsController : ControllerBase
{
    private readonly AzilEduDbContext _context;

    public DonationsController(AzilEduDbContext context)
    {
        _context = context;
    }

    // Kasnije će donator vidjeti samo svoje donacije.
    [HttpGet]
    public async Task<ActionResult<List<DonationDto>>> GetDonations(
        [FromQuery] int? donorId,
        [FromQuery] int? typeId,
        [FromQuery] int? statusId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo)
    {
        var query = _context.Donations
            .Include(donation => donation.Donor)
            .Include(donation => donation.DonationType)
            .Include(donation => donation.DonationStatus)
            .AsQueryable();

        if (donorId.HasValue)
        {
            query = query.Where(donation => donation.DonorId == donorId.Value);
        }

        if (typeId.HasValue)
        {
            query = query.Where(donation => donation.DonationTypeId == typeId.Value);
        }

        if (statusId.HasValue)
        {
            query = query.Where(donation => donation.DonationStatusId == statusId.Value);
        }

        if (dateFrom.HasValue)
        {
            query = query.Where(donation => donation.DonationDate >= dateFrom.Value);
        }

        if (dateTo.HasValue)
        {
            query = query.Where(donation => donation.DonationDate <= dateTo.Value);
        }

        var donations = await query
            .OrderByDescending(donation => donation.DonationDate)
            .ToListAsync();

        return Ok(donations.Select(ToDto).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DonationDto>> GetDonationById(int id)
    {
        var donation = await _context.Donations
            .Include(item => item.Donor)
            .Include(item => item.DonationType)
            .Include(item => item.DonationStatus)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (donation is null)
        {
            return NotFound();
        }

        return Ok(ToDto(donation));
    }

    [HttpPost]
    public async Task<ActionResult<DonationDto>> CreateDonation(SaveDonationDto request)
    {
        var validationError = await ValidateDonationAsync(request);

        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var donation = new Donation
        {
            DonationDate = request.DonationDate,
            Amount = request.Amount,
            ItemName = request.ItemName,
            Quantity = request.Quantity,
            EstimatedValue = request.EstimatedValue,
            Notes = request.Notes,
            DonorId = request.DonorId,
            DonationTypeId = request.DonationTypeId,
            DonationStatusId = request.DonationStatusId
        };

        _context.Donations.Add(donation);
        await _context.SaveChangesAsync();

        var created = await _context.Donations
            .Include(item => item.Donor)
            .Include(item => item.DonationType)
            .Include(item => item.DonationStatus)
            .FirstAsync(item => item.Id == donation.Id);

        return CreatedAtAction(
            nameof(GetDonationById),
            new { id = donation.Id },
            ToDto(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDonation(int id, SaveDonationDto request)
    {
        var donation = await _context.Donations.FindAsync(id);

        if (donation is null)
        {
            return NotFound();
        }

        var validationError = await ValidateDonationAsync(request);

        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        donation.DonationDate = request.DonationDate;
        donation.Amount = request.Amount;
        donation.ItemName = request.ItemName;
        donation.Quantity = request.Quantity;
        donation.EstimatedValue = request.EstimatedValue;
        donation.Notes = request.Notes;
        donation.DonorId = request.DonorId;
        donation.DonationTypeId = request.DonationTypeId;
        donation.DonationStatusId = request.DonationStatusId;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDonation(int id)
    {
        var donation = await _context.Donations.FindAsync(id);

        if (donation is null)
        {
            return NotFound();
        }

        _context.Donations.Remove(donation);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<string?> ValidateDonationAsync(SaveDonationDto request)
    {
        var donorExists = await _context.Donors.AnyAsync(d => d.Id == request.DonorId);

        if (!donorExists)
        {
            return "Odabrani donator ne postoji.";
        }

        var type = await _context.DonationTypes
            .FirstOrDefaultAsync(t => t.Id == request.DonationTypeId);

        if (type is null)
        {
            return "Odabrani tip donacije ne postoji.";
        }

        var statusExists = await _context.DonationStatuses
            .AnyAsync(s => s.Id == request.DonationStatusId);

        if (!statusExists)
        {
            return "Odabrani status donacije ne postoji.";
        }

        if (type.Name == "Novčana")
        {
            if (!request.Amount.HasValue || request.Amount.Value <= 0)
            {
                return "Novčana donacija mora imati iznos veći od nule.";
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(request.ItemName))
            {
                return "Materijalna donacija mora imati naziv stvari.";
            }
        }

        return null;
    }

    private static DonationDto ToDto(Donation donation)
    {
        return new DonationDto
        {
            Id = donation.Id,
            DonationDate = donation.DonationDate,
            Amount = donation.Amount,
            ItemName = donation.ItemName,
            Quantity = donation.Quantity,
            EstimatedValue = donation.EstimatedValue,
            Notes = donation.Notes,
            DonorId = donation.DonorId,
            DonorName = donation.Donor != null
                ? (donation.Donor.OrganizationName != string.Empty
                    ? donation.Donor.OrganizationName
                    : donation.Donor.LastName + " " + donation.Donor.FirstName)
                : string.Empty,
            DonationTypeId = donation.DonationTypeId,
            DonationTypeName = donation.DonationType != null ? donation.DonationType.Name : string.Empty,
            DonationStatusId = donation.DonationStatusId,
            Status = donation.DonationStatus != null ? donation.DonationStatus.Name : string.Empty
        };
    }
}