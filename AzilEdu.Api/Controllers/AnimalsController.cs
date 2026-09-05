using AzilEdu.Api.Data;
using AzilEdu.Shared.DTOs;
using AzilEdu.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AzilEdu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnimalsController : ControllerBase
{
    private readonly AzilEduDbContext _context;

    public AnimalsController(AzilEduDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<AnimalDto>>> GetAnimals()
    {
        var animals = await _context.Animals
            .Include(animal => animal.AnimalStatus)
            .OrderBy(animal => animal.Name)
            .Select(animal => new AnimalDto
            {
                Id = animal.Id,
                Name = animal.Name,
                Species = animal.Species,
                Breed = animal.Breed,
                Gender = animal.Gender,
                Age = animal.Age,
                ArrivalDate = animal.ArrivalDate,
                AnimalStatusId = animal.AnimalStatusId,
                Status = animal.AnimalStatus != null ? animal.AnimalStatus.Name : string.Empty,
                ImageUrl = animal.Media
                    .Where(media => media.IsCover && media.MediaType == AnimalMediaType.Image)
                    .OrderBy(media => media.SortOrder)
                    .Select(media => "/uploads/animals/" + media.StoredFileName)
                    .FirstOrDefault() ?? animal.ImageUrl,
                Description = animal.Description
            })
            .ToListAsync();

        foreach (var animal in animals)
            animal.ImageUrl = ToPublicImageUrl(animal.ImageUrl);

        return Ok(animals);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AnimalDto>> GetAnimalById(int id)
    {
        var animal = await _context.Animals
            .Include(item => item.AnimalStatus)
            .Include(item => item.Media)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (animal is null)
            return NotFound();

        return Ok(new AnimalDto
        {
            Id = animal.Id,
            Name = animal.Name,
            Species = animal.Species,
            Breed = animal.Breed,
            Gender = animal.Gender,
            Age = animal.Age,
            ArrivalDate = animal.ArrivalDate,
            AnimalStatusId = animal.AnimalStatusId,
            Status = animal.AnimalStatus != null ? animal.AnimalStatus.Name : string.Empty,
            ImageUrl = ToPublicImageUrl(GetCoverImagePath(animal)),
            Description = animal.Description
        });
    }

    [HttpPost]
    [Microsoft.AspNetCore.Authorization.Authorize(
    Policy = AzilEdu.Api.Security.AuthorizationPolicies.Staff)]
    public async Task<ActionResult<AnimalDto>> CreateAnimal(SaveAnimalDto dto)
    {
        var animal = new Animal
        {
            Name = dto.Name,
            Species = dto.Species,
            Breed = dto.Breed,
            Gender = dto.Gender,
            Age = dto.Age,
            ArrivalDate = dto.ArrivalDate,
            AnimalStatusId = dto.AnimalStatusId,
            ImageUrl = dto.ImageUrl,
            Description = dto.Description
        };

        _context.Animals.Add(animal);
        await _context.SaveChangesAsync();

        var saved = await _context.Animals
            .Include(a => a.AnimalStatus)
            .FirstOrDefaultAsync(a => a.Id == animal.Id);

        var result = new AnimalDto
        {
            Id = animal.Id,
            Name = animal.Name,
            Species = animal.Species,
            Breed = animal.Breed,
            Gender = animal.Gender,
            Age = animal.Age,
            ArrivalDate = animal.ArrivalDate,
            AnimalStatusId = animal.AnimalStatusId,
            Status = saved?.AnimalStatus?.Name ?? string.Empty,
            ImageUrl = animal.ImageUrl,
            Description = animal.Description
        };

        return CreatedAtAction(nameof(GetAnimalById), new { id = animal.Id }, result);
    }

    [HttpPut("{id}")]
    [Microsoft.AspNetCore.Authorization.Authorize(
    Policy = AzilEdu.Api.Security.AuthorizationPolicies.Staff)]
    public async Task<IActionResult> UpdateAnimal(int id, SaveAnimalDto dto)
    {
        var animal = await _context.Animals.FindAsync(id);

        if (animal is null)
            return NotFound();

        animal.Name = dto.Name;
        animal.Species = dto.Species;
        animal.Breed = dto.Breed;
        animal.Gender = dto.Gender;
        animal.Age = dto.Age;
        animal.ArrivalDate = dto.ArrivalDate;
        animal.AnimalStatusId = dto.AnimalStatusId;
        animal.ImageUrl = dto.ImageUrl;
        animal.Description = dto.Description;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Microsoft.AspNetCore.Authorization.Authorize(
    Policy = AzilEdu.Api.Security.AuthorizationPolicies.Staff)]
    public async Task<IActionResult> DeleteAnimal(int id)
    {
        var animal = await _context.Animals.FindAsync(id);

        if (animal is null)
            return NotFound();

        _context.Animals.Remove(animal);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("lookup")]
    public async Task<ActionResult<List<LookupDto>>> GetAnimalsLookup()
    {
        var result = await _context.Animals
            .OrderBy(a => a.Name)
            .Select(a => new LookupDto
            {
                Id = a.Id,
                Name = a.Name
            })
            .ToListAsync();

        return Ok(result);
    }

    private static string GetCoverImagePath(Animal animal)
    {
        var cover = animal.Media
            .Where(media => media.IsCover && media.MediaType == AnimalMediaType.Image)
            .OrderBy(media => media.SortOrder)
            .FirstOrDefault();

        return cover is null
            ? animal.ImageUrl
            : $"/uploads/animals/{cover.StoredFileName}";
    }

    private string ToPublicImageUrl(string imageUrl)
    {
        if (!imageUrl.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
            return imageUrl;

        return $"{Request.Scheme}://{Request.Host}{imageUrl}";
    }
}