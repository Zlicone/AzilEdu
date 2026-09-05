# AzilEdu

Sustav za upravljanje azilom za životinje. Rješenje se sastoji od tri projekta:

| Projekt | Uloga |
| --- | --- |
| `AzilEdu.App` | Blazor Web App korisničko sučelje (MudBlazor) |
| `AzilEdu.Api` | ASP.NET Core Web API, EF Core, SQLite baza, AI servisi |
| `AzilEdu.Shared` | zajednički entity modeli i DTO klase |

App nikada ne pristupa bazi izravno. Sva komunikacija ide preko HTTP zahtjeva prema API projektu.

---

## Pokretanje

### Preduvjeti

- .NET 10 SDK
- Visual Studio 2026 (ili novije) s workloadom **ASP.NET and web development**

### Koraci

1. Otvori `AzilEdu.slnx`.
2. Desni klik na solution → **Configure Startup Projects** → **Multiple startup projects**.
3. Postavi `AzilEdu.Api` i `AzilEdu.App` na **Start**, uz `AzilEdu.Api` iznad App projekta.
4. Pokreni (F5).

Baza se stvara automatski pri prvom pokretanju API projekta. `Program.cs` poziva `MigrateAsync()`, primjenjuje sve migracije i zatim puni početne podatke.

Ako želiš krenuti od potpuno prazne baze, obriši `AzilEdu.Api/AzilEdu.db` i ponovno pokreni API.

### Portovi

| Projekt | Adresa |
| --- | --- |
| `AzilEdu.App` | `https://localhost:7192` |
| `AzilEdu.Api` | `https://localhost:7114` |
| Swagger | `https://localhost:7114/swagger` |

App zna adresu API projekta preko `HttpClient.BaseAddress` u `AzilEdu.App/Program.cs`. Ako se portovi razlikuju, uskladi ih s `AzilEdu.Api/Properties/launchSettings.json`.

---

## Demo računi

Početni računi nastaju kroz `AppUserSeeder`. Lozinke se ne zapisuju u ovaj dokument; nalaze se u seed kodu i ispisane su na stranici za prijavu radi demonstracije.

| E-mail | Uloge | Povezani profil |
| --- | --- | --- |
| `admin@aziledu.local` | User, Admin | — |
| `employee@aziledu.local` | User, Employee | prvi djelatnik iz seeda |
| `volunteer@aziledu.local` | User, Volunteer | prvi volonter iz seeda |
| `donor@aziledu.local` | User, Donor | prvi donator iz seeda |

Lozinke se spremaju isključivo kao hash (`PasswordHasher<AppUser>`). Hash nikada ne izlazi kroz DTO.

---

## Relacije korisničkih računa

Korisnički račun i poslovni profil namjerno su odvojeni. Račun čuva podatke za prijavu i ovlasti, a poslovne tablice čuvaju podatke o osobi ili organizaciji.

### AppUser – AppRole

Veza više-na-više preko spojne tablice `AppUserRole`. Jedan račun može imati više uloga, a jedna uloga pripada većem broju računa. Primarni ključ spojne tablice je kombinacija `AppUserId` i `AppRoleId`, pa isti par ne može postojati dvaput.

```
AppUsers ──< AppUserRoles >── AppRoles
```

### AppUser – Volunteer, Donor, Employee

Tri odvojene veze jedan-na-jedan preko nullable stranih ključeva `VolunteerId`, `DonorId` i `EmployeeId`. Sve tri su opcionalne jer su moguće sve kombinacije:

- donator bez računa (donirao telefonom, nikada se ne prijavljuje)
- administrator bez poslovnog profila
- volonter koji ima i račun i profil, povezane preko `VolunteerId`

Brisanje poslovnog profila koristi `DeleteBehavior.SetNull`, pa račun ostaje, samo gubi vezu.

Povezani ID zapisuje se u JWT token kao vlastiti claim (`volunteer_id`, `donor_id`, `employee_id`) i služi za provjeru vlasništva podataka.

---

## 401 i 403

| Status | Značenje | Kada nastaje |
| --- | --- | --- |
| **401 Unauthorized** | poslužitelj ne zna tko je pošiljatelj | token nije poslan, istekao je ili potpis ne odgovara sadržaju |
| **403 Forbidden** | identitet je poznat, ali ovlast nije dovoljna | token je valjan, ali korisnik nema traženu ulogu ili nema povezani profil |

401 znači "prijavi se", 403 znači "prijavljen si, ali ovo nije za tebe".

Zaštita je uključena po zadanom. `FallbackPolicy` u `Program.cs` traži prijavljenog korisnika za svaki endpoint koji nema vlastiti atribut. Iznimke se rade svjesno, atributom `[AllowAnonymous]`, i trenutačno postoji samo jedna: prijava.

Dvije imenovane politike:

| Policy | Uloge |
| --- | --- |
| `Staff` | Admin, Employee |
| `AdminOnly` | Admin |

---

## AI funkcionalnosti

App nikada ne poziva vanjski AI servis izravno. Svaki zahtjev ide kroz `AiController`, koji provjerava ulogu, ograničava svrhu i duljinu ulaza te sam priprema podatke iz baze.

| Endpoint | Tko smije | Što se šalje provideru |
| --- | --- | --- |
| `GET /api/ai/status` | Staff | ništa; vraća samo naziv providera i modela |
| `POST /api/ai/text` | Staff | tekst koji je pripremilo sučelje, uz jednu od tri dopuštene svrhe i ograničenje od 4000 znakova |
| `GET /api/ai/daily-summary` | Staff | samo agregirane brojke koje API sam prebroji u bazi |
| `GET /api/ai/volunteer-summary/mine` | Volunteer | naslov, tip, ime životinje, status i rok za najviše deset vlastitih otvorenih zadataka |
| `POST /api/ai/animal-intake` | Staff | slobodna bilješka s terena |
| `POST /api/ai/animal-data-check` | Staff | odabrana polja životinje koja API sam prepiše prije slanja |

Dopuštene svrhe za `POST /api/ai/text` su `animal-adoption`, `donor-thank-you` i `social-post`. Svaka druga vrijednost vraća 400.

Nijedan AI odgovor ne sprema se automatski. Prijedlog se prikaže korisniku, korisnik ga može urediti ili odbaciti, a spremanje ide kroz postojeći CUD tok i uobičajenu validaciju.

### Mock i OpenAI način rada

Obje implementacije zadovoljavaju isto sučelje `IAiService`. Aktivnu bira konfiguracija, pa promjena providera ne mijenja Blazor stranice, DTO modele, `AiController` ni rute.

Zadani način rada je Mock, definiran u `appsettings.json`:

```json
"Ai": {
  "Provider": "Mock",
  "Model": "gpt-5.6-luna",
  "ApiKey": ""
}
```

Mock daje predvidljive odgovore i radi bez interneta i bez ključa.

Za vanjski provider ključ se postavlja kroz user secrets, izvan repozitorija:

```powershell
cd AzilEdu.Api
dotnet user-secrets init
dotnet user-secrets set "Ai:Provider" "OpenAI"
dotnet user-secrets set "Ai:ApiKey" "LOKALNI-KLJUC"
dotnet user-secrets set "Ai:Model" "gpt-4o-mini"
```

Povratak na Mock:

```powershell
dotnet user-secrets set "Ai:Provider" "Mock"
```

Ključ se ne zapisuje u `appsettings.json`, ne postoji u App projektu i ne ulazi u repozitorij. Kartica na početnoj stranici prikazuje aktivni provider, ali nikada ključ.

---

## Multimedija

| Što | Gdje |
| --- | --- |
| datoteka | `AzilEdu.Api/wwwroot/uploads/animals` |
| podaci o datoteci | tablica `AnimalMedia` |

U bazu se ne sprema sadržaj datoteke, nego samo metapodaci: kojoj životinji pripada, ime na disku, izvorno ime, MIME tip, veličina, je li naslovna i redoslijed prikaza.

Ime datoteke na disku generira API (`Guid`), a ne korisnik. Ekstenzija se uzima iz popisa dopuštenih MIME tipova, pa korisnik ne može utjecati na putanju.

Provjere pri uploadu: postoji li životinja, je li datoteka prazna, je li veća od 25 MB i je li tip dopušten (JPG, PNG, WEBP, MP4, WEBM).

Datoteke poslužuje API preko `UseStaticFiles()`, a DTO vraća punu adresu jer App radi na drugom portu.

---

## Poznata ograničenja

- Token vrijedi 60 minuta i nema mehanizam obnove; nakon isteka potrebna je nova prijava.
- Razvojni potpisni ključ nalazi se u `appsettings.json` i služi isključivo za lokalni rad.
- Brisanje životinje uklanja zapise u `AnimalMedia` kaskadno, ali datoteke ostaju na disku.
- Filtriranje na nekim stranicama i dalje se dijelom odvija u pregledniku, što je prihvatljivo za ovu količinu podataka, ali ne i za veće skupove.
- Mock AI ne prepoznaje ime, pasminu ni starost iz slobodnog teksta; te podatke korisnik dopunjuje ručno.
- Nema evidencije o tome tko je i kada pokrenuo pojedinu AI funkciju.

## Prijedlozi za sljedeću verziju

1. **Refresh token i evidencija prijava.** Uz pristupni token uvesti refresh token, ograničiti broj neuspjelih pokušaja prijave i zapisivati vrijeme te uređaj s kojeg je prijava napravljena.
2. **Serversko straničenje i filtriranje.** Prebaciti pretragu, filtriranje i sortiranje na API uz straničenje, čime liste ostaju upotrebljive i kad broj zapisa naraste.
