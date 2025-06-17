using Microsoft.EntityFrameworkCore;
using KnjiznicaAPI.Data;
using KnjiznicaAPI.Models;
using KnjiznicaAPI.DTOs;
using KnjiznicaAPI.Configuration;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

// Configure SQLite and DbContext
builder.Services.ConfigureSQLite(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Knjižnica API", Version = "v1" });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();

// Initialize database
try
{
    DatabaseInitializer.EnsureDatabaseCreated(app.Services);
    app.Logger.LogInformation("Database initialization completed successfully");
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Failed to initialize database");
    throw;
}

#region Test Endpoints

app.MapGet("/", () => new {
    Message = "Knjižnica API je uspešno zagnan!",
    Version = "1.0.0",
    Timestamp = DateTime.Now,
    Endpoints = new
    {
        Swagger = "/swagger",
        Knjige = "/api/knjige",
        Avtorji = "/api/avtorji",
        Kategorije = "/api/kategorije",
        Statistike = "/api/statistike"
    }
})
.WithName("GetRoot")
.WithTags("System");

app.MapGet("/health", async (KnjiznicaDbContext db) =>
{
    try
    {
        // Test database connection
        var canConnect = await db.Database.CanConnectAsync();
        var knjigCount = await db.Knjige.CountAsync();
        var avtorCount = await db.Avtorji.CountAsync();
        var kategorijaCount = await db.Kategorije.CountAsync();

        return Results.Ok(new
        {
            Status = "Healthy",
            Database = canConnect ? "Connected" : "Disconnected",
            DataCounts = new
            {
                Knjige = knjigCount,
                Avtorji = avtorCount,
                Kategorije = kategorijaCount
            },
            Timestamp = DateTime.Now
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Health Check Failed",
            detail: ex.Message,
            statusCode: 500
        );
    }
})
.WithName("HealthCheck")
.WithTags("System");

#endregion

#region Kategorije Endpoints

app.MapGet("/api/kategorije", async (KnjiznicaDbContext db) =>
{
    var kategorije = await db.Kategorije
        .Select(k => new KategorijaResponseDto
        {
            Id = k.Id,
            Ime = k.Ime,
            Opis = k.Opis,
            BrojKnjig = k.Knjige.Count()
        })
        .ToListAsync();

    return Results.Ok(kategorije);
})
.WithName("GetKategorije")
.WithTags("Kategorije");

app.MapGet("/api/kategorije/{id}", async (int id, KnjiznicaDbContext db) =>
{
    var kategorija = await db.Kategorije
        .Include(k => k.Knjige)
        .FirstOrDefaultAsync(k => k.Id == id);

    if (kategorija == null)
        return Results.NotFound($"Kategorija z ID {id} ni bila najdena.");

    var response = new KategorijaResponseDto
    {
        Id = kategorija.Id,
        Ime = kategorija.Ime,
        Opis = kategorija.Opis,
        BrojKnjig = kategorija.Knjige.Count,
        Knjige = kategorija.Knjige.Select(k => new KnjigaResponseDto
        {
            Id = k.Id,
            Naslov = k.Naslov,
            ISBN = k.ISBN,
            DatumIzdaje = k.DatumIzdaje,
            AvtorId = k.AvtorId,
            KategorijaId = k.KategorijaId
        }).ToList()
    };

    return Results.Ok(response);
})
.WithName("GetKategorija")
.WithTags("Kategorije");

app.MapPost("/api/kategorije", async (KategorijaCreateDto kategorijaDto, KnjiznicaDbContext db) =>
{
    // Validation
    var validationResults = new List<ValidationResult>();
    var validationContext = new ValidationContext(kategorijaDto);

    if (!Validator.TryValidateObject(kategorijaDto, validationContext, validationResults, true))
    {
        return Results.BadRequest(validationResults.Select(v => v.ErrorMessage));
    }

    // Check if category with same name exists
    var existingKategorija = await db.Kategorije
        .FirstOrDefaultAsync(k => k.Ime.ToLower() == kategorijaDto.Ime.ToLower());

    if (existingKategorija != null)
        return Results.BadRequest("Kategorija s tem imenom že obstaja.");

    var kategorija = new Kategorija
    {
        Ime = kategorijaDto.Ime,
        Opis = kategorijaDto.Opis
    };

    db.Kategorije.Add(kategorija);
    await db.SaveChangesAsync();

    var response = new KategorijaResponseDto
    {
        Id = kategorija.Id,
        Ime = kategorija.Ime,
        Opis = kategorija.Opis,
        BrojKnjig = 0
    };

    return Results.Created($"/api/kategorije/{kategorija.Id}", response);
})
.WithName("CreateKategorija")
.WithTags("Kategorije");

app.MapPut("/api/kategorije/{id}", async (int id, KategorijaUpdateDto kategorijaDto, KnjiznicaDbContext db) =>
{
    var kategorija = await db.Kategorije.FindAsync(id);
    if (kategorija == null)
        return Results.NotFound($"Kategorija z ID {id} ni bila najdena.");

    // Validation
    var validationResults = new List<ValidationResult>();
    var validationContext = new ValidationContext(kategorijaDto);

    if (!Validator.TryValidateObject(kategorijaDto, validationContext, validationResults, true))
    {
        return Results.BadRequest(validationResults.Select(v => v.ErrorMessage));
    }

    // Check if another category with same name exists
    var existingKategorija = await db.Kategorije
        .FirstOrDefaultAsync(k => k.Ime.ToLower() == kategorijaDto.Ime.ToLower() && k.Id != id);

    if (existingKategorija != null)
        return Results.BadRequest("Kategorija s tem imenom že obstaja.");

    kategorija.Ime = kategorijaDto.Ime;
    kategorija.Opis = kategorijaDto.Opis;

    await db.SaveChangesAsync();

    var response = new KategorijaResponseDto
    {
        Id = kategorija.Id,
        Ime = kategorija.Ime,
        Opis = kategorija.Opis,
        BrojKnjig = await db.Knjige.CountAsync(k => k.KategorijaId == kategorija.Id)
    };

    return Results.Ok(response);
})
.WithName("UpdateKategorija")
.WithTags("Kategorije");

app.MapDelete("/api/kategorije/{id}", async (int id, KnjiznicaDbContext db) =>
{
    var kategorija = await db.Kategorije
        .Include(k => k.Knjige)
        .FirstOrDefaultAsync(k => k.Id == id);

    if (kategorija == null)
        return Results.NotFound($"Kategorija z ID {id} ni bila najdena.");

    if (kategorija.Knjige.Any())
        return Results.BadRequest("Kategorije ni mogoèe izbrisati, ker vsebuje knjige.");

    db.Kategorije.Remove(kategorija);
    await db.SaveChangesAsync();

    return Results.NoContent();
})
.WithName("DeleteKategorija")
.WithTags("Kategorije");

#endregion

#region Avtorji Endpoints

app.MapGet("/api/avtorji", async (KnjiznicaDbContext db, string? search = null, string? sortBy = null) =>
{
    var query = db.Avtorji.AsQueryable();

    // Search functionality
    if (!string.IsNullOrEmpty(search))
    {
        query = query.Where(a =>
            a.Ime.Contains(search) ||
            a.Priimek.Contains(search) ||
            a.Email.Contains(search));
    }

    // Sorting functionality
    query = sortBy?.ToLower() switch
    {
        "ime" => query.OrderBy(a => a.Ime),
        "priimek" => query.OrderBy(a => a.Priimek),
        "email" => query.OrderBy(a => a.Email),
        "datum" => query.OrderBy(a => a.DatumRojstva),
        _ => query.OrderBy(a => a.Priimek).ThenBy(a => a.Ime)
    };

    var avtorji = await query
        .Select(a => new AvtorResponseDto
        {
            Id = a.Id,
            Ime = a.Ime,
            Priimek = a.Priimek,
            DatumRojstva = a.DatumRojstva,
            Email = a.Email,
            Biografija = a.Biografija,
            BrojKnjig = a.Knjige.Count()
        })
        .ToListAsync();

    return Results.Ok(avtorji);
})
.WithName("GetAvtorji")
.WithTags("Avtorji");

app.MapGet("/api/avtorji/{id}", async (int id, KnjiznicaDbContext db) =>
{
    var avtor = await db.Avtorji
        .Include(a => a.Knjige)
            .ThenInclude(k => k.Kategorija)
        .FirstOrDefaultAsync(a => a.Id == id);

    if (avtor == null)
        return Results.NotFound($"Avtor z ID {id} ni bil najden.");

    var response = new AvtorResponseDto
    {
        Id = avtor.Id,
        Ime = avtor.Ime,
        Priimek = avtor.Priimek,
        DatumRojstva = avtor.DatumRojstva,
        Email = avtor.Email,
        Biografija = avtor.Biografija,
        BrojKnjig = avtor.Knjige.Count,
        Knjige = avtor.Knjige.Select(k => new KnjigaResponseDto
        {
            Id = k.Id,
            Naslov = k.Naslov,
            ISBN = k.ISBN,
            DatumIzdaje = k.DatumIzdaje,
            AvtorId = k.AvtorId,
            KategorijaId = k.KategorijaId,
            KategorijaNaziv = k.Kategorija?.Ime
        }).ToList()
    };

    return Results.Ok(response);
})
.WithName("GetAvtor")
.WithTags("Avtorji");

app.MapPost("/api/avtorji", async (AvtorCreateDto avtorDto, KnjiznicaDbContext db) =>
{
    // Validation
    var validationResults = new List<ValidationResult>();
    var validationContext = new ValidationContext(avtorDto);

    if (!Validator.TryValidateObject(avtorDto, validationContext, validationResults, true))
    {
        return Results.BadRequest(validationResults.Select(v => v.ErrorMessage));
    }

    // Check if author with same email exists
    var existingAvtor = await db.Avtorji
        .FirstOrDefaultAsync(a => a.Email.ToLower() == avtorDto.Email.ToLower());

    if (existingAvtor != null)
        return Results.BadRequest("Avtor s tem email naslovom že obstaja.");

    var avtor = new Avtor
    {
        Ime = avtorDto.Ime,
        Priimek = avtorDto.Priimek,
        DatumRojstva = avtorDto.DatumRojstva,
        Email = avtorDto.Email,
        Biografija = avtorDto.Biografija
    };

    db.Avtorji.Add(avtor);
    await db.SaveChangesAsync();

    var response = new AvtorResponseDto
    {
        Id = avtor.Id,
        Ime = avtor.Ime,
        Priimek = avtor.Priimek,
        DatumRojstva = avtor.DatumRojstva,
        Email = avtor.Email,
        Biografija = avtor.Biografija,
        BrojKnjig = 0
    };

    return Results.Created($"/api/avtorji/{avtor.Id}", response);
})
.WithName("CreateAvtor")
.WithTags("Avtorji");

app.MapPut("/api/avtorji/{id}", async (int id, AvtorUpdateDto avtorDto, KnjiznicaDbContext db) =>
{
    var avtor = await db.Avtorji.FindAsync(id);
    if (avtor == null)
        return Results.NotFound($"Avtor z ID {id} ni bil najden.");

    // Validation
    var validationResults = new List<ValidationResult>();
    var validationContext = new ValidationContext(avtorDto);

    if (!Validator.TryValidateObject(avtorDto, validationContext, validationResults, true))
    {
        return Results.BadRequest(validationResults.Select(v => v.ErrorMessage));
    }

    // Check if another author with same email exists
    var existingAvtor = await db.Avtorji
        .FirstOrDefaultAsync(a => a.Email.ToLower() == avtorDto.Email.ToLower() && a.Id != id);

    if (existingAvtor != null)
        return Results.BadRequest("Avtor s tem email naslovom že obstaja.");

    avtor.Ime = avtorDto.Ime;
    avtor.Priimek = avtorDto.Priimek;
    avtor.DatumRojstva = avtorDto.DatumRojstva;
    avtor.Email = avtorDto.Email;
    avtor.Biografija = avtorDto.Biografija;

    await db.SaveChangesAsync();

    var response = new AvtorResponseDto
    {
        Id = avtor.Id,
        Ime = avtor.Ime,
        Priimek = avtor.Priimek,
        DatumRojstva = avtor.DatumRojstva,
        Email = avtor.Email,
        Biografija = avtor.Biografija,
        BrojKnjig = await db.Knjige.CountAsync(k => k.AvtorId == avtor.Id)
    };

    return Results.Ok(response);
})
.WithName("UpdateAvtor")
.WithTags("Avtorji");

app.MapDelete("/api/avtorji/{id}", async (int id, KnjiznicaDbContext db) =>
{
    var avtor = await db.Avtorji
        .Include(a => a.Knjige)
        .FirstOrDefaultAsync(a => a.Id == id);

    if (avtor == null)
        return Results.NotFound($"Avtor z ID {id} ni bil najden.");

    if (avtor.Knjige.Any())
        return Results.BadRequest("Avtorja ni mogoèe izbrisati, ker ima objavljene knjige.");

    db.Avtorji.Remove(avtor);
    await db.SaveChangesAsync();

    return Results.NoContent();
})
.WithName("DeleteAvtor")
.WithTags("Avtorji");

#endregion

#region Knjige Endpoints

app.MapGet("/api/knjige", async (KnjiznicaDbContext db, string? search = null, string? sortBy = null, int? kategorijaId = null, int? avtorId = null) =>
{
    var query = db.Knjige
        .Include(k => k.Avtor)
        .Include(k => k.Kategorija)
        .AsQueryable();

    // Filter functionality
    if (kategorijaId.HasValue)
        query = query.Where(k => k.KategorijaId == kategorijaId.Value);

    if (avtorId.HasValue)
        query = query.Where(k => k.AvtorId == avtorId.Value);

    // Search functionality
    if (!string.IsNullOrEmpty(search))
    {
        query = query.Where(k =>
            k.Naslov.Contains(search) ||
            k.ISBN.Contains(search) ||
            k.Avtor.Ime.Contains(search) ||
            k.Avtor.Priimek.Contains(search) ||
            k.Kategorija.Ime.Contains(search));
    }

    // Sorting functionality
    query = sortBy?.ToLower() switch
    {
        "naslov" => query.OrderBy(k => k.Naslov),
        "isbn" => query.OrderBy(k => k.ISBN),
        "datum" => query.OrderBy(k => k.DatumIzdaje),
        "avtor" => query.OrderBy(k => k.Avtor.Priimek).ThenBy(k => k.Avtor.Ime),
        "kategorija" => query.OrderBy(k => k.Kategorija.Ime),
        _ => query.OrderBy(k => k.Naslov)
    };

    var knjige = await query
        .Select(k => new KnjigaResponseDto
        {
            Id = k.Id,
            Naslov = k.Naslov,
            ISBN = k.ISBN,
            DatumIzdaje = k.DatumIzdaje,
            AvtorId = k.AvtorId,
            AvtorIme = k.Avtor.Ime + " " + k.Avtor.Priimek,
            KategorijaId = k.KategorijaId,
            KategorijaNaziv = k.Kategorija.Ime
        })
        .ToListAsync();

    return Results.Ok(knjige);
})
.WithName("GetKnjige")
.WithTags("Knjige");

app.MapGet("/api/knjige/{id}", async (int id, KnjiznicaDbContext db) =>
{
    var knjiga = await db.Knjige
        .Include(k => k.Avtor)
        .Include(k => k.Kategorija)
        .FirstOrDefaultAsync(k => k.Id == id);

    if (knjiga == null)
        return Results.NotFound($"Knjiga z ID {id} ni bila najdena.");

    var response = new KnjigaResponseDto
    {
        Id = knjiga.Id,
        Naslov = knjiga.Naslov,
        ISBN = knjiga.ISBN,
        DatumIzdaje = knjiga.DatumIzdaje,
        AvtorId = knjiga.AvtorId,
        AvtorIme = knjiga.Avtor.Ime + " " + knjiga.Avtor.Priimek,
        KategorijaId = knjiga.KategorijaId,
        KategorijaNaziv = knjiga.Kategorija.Ime
    };

    return Results.Ok(response);
})
.WithName("GetKnjiga")
.WithTags("Knjige");

app.MapPost("/api/knjige", async (KnjigaCreateDto knjigaDto, KnjiznicaDbContext db) =>
{
    // Validation
    var validationResults = new List<ValidationResult>();
    var validationContext = new ValidationContext(knjigaDto);

    if (!Validator.TryValidateObject(knjigaDto, validationContext, validationResults, true))
    {
        return Results.BadRequest(validationResults.Select(v => v.ErrorMessage));
    }

    // Check if book with same ISBN exists
    var existingKnjiga = await db.Knjige
        .FirstOrDefaultAsync(k => k.ISBN == knjigaDto.ISBN);

    if (existingKnjiga != null)
        return Results.BadRequest("Knjiga s tem ISBN že obstaja.");

    // Check if author exists
    var avtorExists = await db.Avtorji.AnyAsync(a => a.Id == knjigaDto.AvtorId);
    if (!avtorExists)
        return Results.BadRequest("Avtor ne obstaja.");

    // Check if category exists
    var kategorijaExists = await db.Kategorije.AnyAsync(k => k.Id == knjigaDto.KategorijaId);
    if (!kategorijaExists)
        return Results.BadRequest("Kategorija ne obstaja.");

    var knjiga = new Knjiga
    {
        Naslov = knjigaDto.Naslov,
        ISBN = knjigaDto.ISBN,
        DatumIzdaje = knjigaDto.DatumIzdaje,
        AvtorId = knjigaDto.AvtorId,
        KategorijaId = knjigaDto.KategorijaId
    };

    db.Knjige.Add(knjiga);
    await db.SaveChangesAsync();

    // Load the created book with related data
    var createdKnjiga = await db.Knjige
        .Include(k => k.Avtor)
        .Include(k => k.Kategorija)
        .FirstAsync(k => k.Id == knjiga.Id);

    var response = new KnjigaResponseDto
    {
        Id = createdKnjiga.Id,
        Naslov = createdKnjiga.Naslov,
        ISBN = createdKnjiga.ISBN,
        DatumIzdaje = createdKnjiga.DatumIzdaje,
        AvtorId = createdKnjiga.AvtorId,
        AvtorIme = createdKnjiga.Avtor.Ime + " " + createdKnjiga.Avtor.Priimek,
        KategorijaId = createdKnjiga.KategorijaId,
        KategorijaNaziv = createdKnjiga.Kategorija.Ime
    };

    return Results.Created($"/api/knjige/{knjiga.Id}", response);
})
.WithName("CreateKnjiga")
.WithTags("Knjige");

app.MapPut("/api/knjige/{id}", async (int id, KnjigaUpdateDto knjigaDto, KnjiznicaDbContext db) =>
{
    var knjiga = await db.Knjige.FindAsync(id);
    if (knjiga == null)
        return Results.NotFound($"Knjiga z ID {id} ni bila najdena.");

    // Validation
    var validationResults = new List<ValidationResult>();
    var validationContext = new ValidationContext(knjigaDto);

    if (!Validator.TryValidateObject(knjigaDto, validationContext, validationResults, true))
    {
        return Results.BadRequest(validationResults.Select(v => v.ErrorMessage));
    }

    // Check if another book with same ISBN exists
    var existingKnjiga = await db.Knjige
        .FirstOrDefaultAsync(k => k.ISBN == knjigaDto.ISBN && k.Id != id);

    if (existingKnjiga != null)
        return Results.BadRequest("Knjiga s tem ISBN že obstaja.");

    // Check if author exists
    var avtorExists = await db.Avtorji.AnyAsync(a => a.Id == knjigaDto.AvtorId);
    if (!avtorExists)
        return Results.BadRequest("Avtor ne obstaja.");

    // Check if category exists
    var kategorijaExists = await db.Kategorije.AnyAsync(k => k.Id == knjigaDto.KategorijaId);
    if (!kategorijaExists)
        return Results.BadRequest("Kategorija ne obstaja.");

    knjiga.Naslov = knjigaDto.Naslov;
    knjiga.ISBN = knjigaDto.ISBN;
    knjiga.DatumIzdaje = knjigaDto.DatumIzdaje;
    knjiga.AvtorId = knjigaDto.AvtorId;
    knjiga.KategorijaId = knjigaDto.KategorijaId;

    await db.SaveChangesAsync();

    // Load the updated book with related data
    var updatedKnjiga = await db.Knjige
        .Include(k => k.Avtor)
        .Include(k => k.Kategorija)
        .FirstAsync(k => k.Id == knjiga.Id);

    var response = new KnjigaResponseDto
    {
        Id = updatedKnjiga.Id,
        Naslov = updatedKnjiga.Naslov,
        ISBN = updatedKnjiga.ISBN,
        DatumIzdaje = updatedKnjiga.DatumIzdaje,
        AvtorId = updatedKnjiga.AvtorId,
        AvtorIme = updatedKnjiga.Avtor.Ime + " " + updatedKnjiga.Avtor.Priimek,
        KategorijaId = updatedKnjiga.KategorijaId,
        KategorijaNaziv = updatedKnjiga.Kategorija.Ime
    };

    return Results.Ok(response);
})
.WithName("UpdateKnjiga")
.WithTags("Knjige");

app.MapDelete("/api/knjige/{id}", async (int id, KnjiznicaDbContext db) =>
{
    var knjiga = await db.Knjige.FindAsync(id);
    if (knjiga == null)
        return Results.NotFound($"Knjiga z ID {id} ni bila najdena.");

    db.Knjige.Remove(knjiga);
    await db.SaveChangesAsync();

    return Results.NoContent();
})
.WithName("DeleteKnjiga")
.WithTags("Knjige");

#endregion

#region Statistics Endpoints

app.MapGet("/api/statistike", async (KnjiznicaDbContext db) =>
{
    var skupajKnjig = await db.Knjige.CountAsync();
    var skupajAvtorjev = await db.Avtorji.CountAsync();
    var skupajKategorij = await db.Kategorije.CountAsync();

    var najKnjigKategorija = await db.Kategorije
        .Include(k => k.Knjige)
        .OrderByDescending(k => k.Knjige.Count)
        .Select(k => new { k.Ime, BrojKnjig = k.Knjige.Count })
        .FirstOrDefaultAsync();

    var najKnjigAvtor = await db.Avtorji
        .Include(a => a.Knjige)
        .OrderByDescending(a => a.Knjige.Count)
        .Select(a => new { ImeAvtorja = a.Ime + " " + a.Priimek, BrojKnjig = a.Knjige.Count })
        .FirstOrDefaultAsync();

    var kategorijeStatistike = await db.Kategorije
        .Select(k => new { k.Ime, BrojKnjig = k.Knjige.Count })
        .OrderByDescending(k => k.BrojKnjig)
        .ToListAsync();

    var avtorjiStatistike = await db.Avtorji
        .Select(a => new { ImeAvtorja = a.Ime + " " + a.Priimek, BrojKnjig = a.Knjige.Count })
        .OrderByDescending(a => a.BrojKnjig)
        .ToListAsync();

    var statistike = new
    {
        SkupajKnjig = skupajKnjig,
        SkupajAvtorjev = skupajAvtorjev,
        SkupajKategorij = skupajKategorij,
        NajKnjigKategorija = najKnjigKategorija,
        NajKnjigAvtor = najKnjigAvtor,
        KategorijeStatistike = kategorijeStatistike,
        AvtorjiStatistike = avtorjiStatistike
    };

    return Results.Ok(statistike);
})
.WithName("GetStatistike")
.WithTags("Statistike");

#endregion

app.Run();