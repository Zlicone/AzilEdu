using AzilEdu.Api.Data;
using Microsoft.EntityFrameworkCore;
using AzilEdu.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AzilEduDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://localhost:7114/")
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AzilEduDbContext>();

    await db.Database.MigrateAsync();

    if (!await db.Animals.AnyAsync())
    {
        db.Animals.AddRange(
            new Animal
            {
                Name = "Luna",
                Species = "Pas",
                Breed = "Labrador",
                Gender = "Ženka",
                Age = 3,
                ArrivalDate = new DateTime(2025, 10, 12),
                IsAdopted = false,
                ImageUrl = "/images/animals/luna.webp",
                Description = "Mirna i druželjubiva kujica koja voli šetnje."
            },
            new Animal
            {
                Name = "Maza",
                Species = "Mačka",
                Breed = "Domaća kratkodlaka",
                Gender = "Ženka",
                Age = 2,
                ArrivalDate = new DateTime(2025, 11, 5),
                IsAdopted = true,
                ImageUrl = "/images/animals/maza.webp",
                Description = "Zaigrana mačka naviknuta na boravak u zatvorenom prostoru."
            },
            new Animal
            {
                Name = "Rex",
                Species = "Pas",
                Breed = "Njemački ovčar",
                Gender = "Mužjak",
                Age = 5,
                ArrivalDate = new DateTime(2026, 1, 20),
                IsAdopted = false,
                ImageUrl = "/images/animals/rex.webp",
                Description = "Aktivan pas koji traži iskusnijeg vlasnika."
            },
            new Animal
            {
                Name = "Nala",
                Species = "Mačka",
                Breed = "Maine Coon mješanac",
                Gender = "Ženka",
                Age = null,
                ArrivalDate = new DateTime(2026, 2, 3),
                IsAdopted = false,
                ImageUrl = "/images/animals/nala.webp",
                Description = "Mlada mačka pronađena bez poznate povijesti."
            },
            new Animal
            {
                Name = "Tobi",
                Species = "Pas",
                Breed = "Mješanac",
                Gender = "Mužjak",
                Age = 1,
                ArrivalDate = null,
                IsAdopted = false,
                ImageUrl = "/images/animals/tobi.webp",
                Description = "Vesel pas kojem datum dolaska još nije potvrđen."
            },
            new Animal
            {
                Name = "Bruno",
                Species = "Pas",
                Breed = "Bigl",
                Gender = "Mužjak",
                Age = 4,
                ArrivalDate = new DateTime(2025, 9, 18),
                IsAdopted = true,
                ImageUrl = "/images/animals/bruno.webp",
                Description = "Udomljen pas koji ostaje u evidenciji azila."
            }
        );

        await db.SaveChangesAsync();
    }

    if (!await db.HousingUnits.AnyAsync())
    {
        db.HousingUnits.AddRange(
            new HousingUnit
            {
                Name = "Boks A1",
                UnitType = "Boks",
                Capacity = 4,
                Occupied = 4,
                LastCleanedAt = new DateTime(2026, 6, 10),
                IsActive = true,
                ImageUrl = "/images/housing-units/box-1.webp",
                Note = "Veliki boks za pse, trenutno popunjen."
            },
            new HousingUnit
            {
                Name = "Boks A2",
                UnitType = "Boks",
                Capacity = 4,
                Occupied = 2,
                LastCleanedAt = new DateTime(2026, 6, 9),
                IsActive = true,
                ImageUrl = "/images/housing-units/box-2.webp",
                Note = "Boks za pse sa slobodnim mjestima."
            },
            new HousingUnit
            {
                Name = "Mačja soba",
                UnitType = "Soba",
                Capacity = 6,
                Occupied = 3,
                LastCleanedAt = new DateTime(2026, 6, 11),
                IsActive = true,
                ImageUrl = "/images/housing-units/cat-room.webp",
                Note = "Prostor za mačke s prozorom."
            },
            new HousingUnit
            {
                Name = "Karantena 1",
                UnitType = "Karantena",
                Capacity = 2,
                Occupied = 1,
                LastCleanedAt = null,
                IsActive = true,
                ImageUrl = "/images/housing-units/quarantine.webp",
                Note = "Karantena za novopridošle životinje."
            },
            new HousingUnit
            {
                Name = "Vanjski boks",
                UnitType = "Boks",
                Capacity = 3,
                Occupied = 0,
                LastCleanedAt = new DateTime(2026, 6, 8),
                IsActive = false,
                ImageUrl = "/images/housing-units/inactive-unit.webp",
                Note = "Trenutno izvan upotrebe zbog popravka."
            },
            new HousingUnit
            {
                Name = "Dvorišna jedinica",
                UnitType = "Soba",
                Capacity = 5,
                Occupied = 2,
                LastCleanedAt = new DateTime(2026, 6, 7),
                IsActive = true,
                ImageUrl = "/images/housing-units/yard-unit.webp",
                Note = "Natkriveni prostor s pristupom dvorištu."
            }
        );

        await db.SaveChangesAsync();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();