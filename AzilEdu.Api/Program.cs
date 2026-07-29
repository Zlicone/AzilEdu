using AzilEdu.Api.Data;
using Microsoft.EntityFrameworkCore;
using AzilEdu.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AzilEduDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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
                AnimalStatusId = 1,
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
                AnimalStatusId = 3,
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
                AnimalStatusId = 1,
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
                AnimalStatusId = 1,
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
                AnimalStatusId = 2,
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
                AnimalStatusId = 3,
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

    if (!await db.Donors.AnyAsync())
    {
        db.Donors.AddRange(
            new Donor
            {
                FirstName = "Ivan",
                LastName = "Perić",
                OrganizationName = "",
                Email = "ivan.peric@example.com",
                Phone = "091 111 2222",
                Address = "Vukovarska 12",
                City = "Split",
                Notes = "Redoviti mjesečni donator.",
                CreatedAt = new DateTime(2026, 3, 14),
                DonorTypeId = 1,
                DonorStatusId = 2
            },
            new Donor
            {
                FirstName = "",
                LastName = "",
                OrganizationName = "Petrić d.o.o.",
                Email = "info@petric.hr",
                Phone = "021 333 444",
                Address = "Poljička cesta 5",
                City = "Split",
                Notes = "Donira hranu za pse.",
                CreatedAt = new DateTime(2026, 1, 20),
                DonorTypeId = 2,
                DonorStatusId = 2
            },
            new Donor
            {
                FirstName = "Marija",
                LastName = "Kovač",
                OrganizationName = "",
                Email = "marija.kovac@example.com",
                Phone = "098 555 6666",
                Address = "Šetalište bb",
                City = "Solin",
                Notes = "Donirala jednom, javit će se ponovo.",
                CreatedAt = new DateTime(2026, 5, 2),
                DonorTypeId = 1,
                DonorStatusId = 3
            },
            new Donor
            {
                FirstName = "",
                LastName = "",
                OrganizationName = "Udruga Šapa",
                Email = "kontakt@sapa.hr",
                Phone = "021 777 888",
                Address = "Kralja Zvonimira 3",
                City = "Kaštela",
                Notes = "Suradnja na akcijama udomljavanja.",
                CreatedAt = new DateTime(2025, 11, 8),
                DonorTypeId = 3,
                DonorStatusId = 4
            }
        );

        await db.SaveChangesAsync();
    }

    if (!await db.Employees.AnyAsync())
    {
        db.Employees.AddRange(
            new Employee
            {
                FirstName = "Ana",
                LastName = "Babić",
                Email = "ana.babic@aziledu.hr",
                Phone = "021 100 200",
                EmployeeNumber = "DJ-001",
                HireDate = new DateTime(2023, 4, 3),
                Notes = "Voditeljica smjene.",
                EmployeePositionId = 1,
                EmployeeStatusId = 1
            },
            new Employee
            {
                FirstName = "Marko",
                LastName = "Jurić",
                Email = "marko.juric@aziledu.hr",
                Phone = "021 100 201",
                EmployeeNumber = "VET-002",
                HireDate = new DateTime(2022, 9, 12),
                Notes = "Veterinar, dolazi utorkom i četvrtkom.",
                EmployeePositionId = 2,
                EmployeeStatusId = 1
            },
            new Employee
            {
                FirstName = "Petra",
                LastName = "Novak",
                Email = "petra.novak@aziledu.hr",
                Phone = "021 100 202",
                EmployeeNumber = "KOO-003",
                HireDate = new DateTime(2024, 1, 15),
                Notes = "Koordinira raspored volontera.",
                EmployeePositionId = 3,
                EmployeeStatusId = 2
            },
            new Employee
            {
                FirstName = "Luka",
                LastName = "Šimić",
                Email = "luka.simic@aziledu.hr",
                Phone = "021 100 203",
                EmployeeNumber = "ADM-004",
                HireDate = new DateTime(2021, 6, 1),
                Notes = "Administrator sustava.",
                EmployeePositionId = 4,
                EmployeeStatusId = 1
            },
            new Employee
            {
                FirstName = "Ivana",
                LastName = "Marić",
                Email = "ivana.maric@aziledu.hr",
                Phone = "021 100 204",
                EmployeeNumber = "DJ-005",
                HireDate = new DateTime(2020, 11, 23),
                Notes = "Trenutno neaktivna.",
                EmployeePositionId = 1,
                EmployeeStatusId = 3
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