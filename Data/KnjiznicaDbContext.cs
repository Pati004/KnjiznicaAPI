using Microsoft.EntityFrameworkCore;
using KnjiznicaAPI.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace KnjiznicaAPI.Data
{
    public class KnjiznicaDbContext : DbContext
    {
        public KnjiznicaDbContext(DbContextOptions<KnjiznicaDbContext> options) : base(options)
        {
        }

        public DbSet<Avtor> Avtorji { get; set; }
        public DbSet<Kategorija> Kategorije { get; set; }
        public DbSet<Knjiga> Knjige { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Avtor entity
            modelBuilder.Entity<Avtor>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Ime)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Priimek)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Biografija)
                    .HasMaxLength(1000);

                entity.HasIndex(e => e.Email).IsUnique();

                // Configure relationship with Knjiga
                entity.HasMany(e => e.Knjige)
                    .WithOne(e => e.Avtor)
                    .HasForeignKey(e => e.AvtorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Kategorija entity
            modelBuilder.Entity<Kategorija>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Ime)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Opis)
                    .HasMaxLength(500);

                entity.HasIndex(e => e.Ime).IsUnique();

                // Configure relationship with Knjiga
                entity.HasMany(e => e.Knjige)
                    .WithOne(e => e.Kategorija)
                    .HasForeignKey(e => e.KategorijaId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Knjiga entity
            modelBuilder.Entity<Knjiga>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Naslov)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.ISBN)
                    .IsRequired()
                    .HasMaxLength(13);

                entity.HasIndex(e => e.ISBN).IsUnique();

                // Configure relationships
                entity.HasOne(e => e.Avtor)
                    .WithMany(e => e.Knjige)
                    .HasForeignKey(e => e.AvtorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Kategorija)
                    .WithMany(e => e.Knjige)
                    .HasForeignKey(e => e.KategorijaId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Avtorji
            var avtorji = new[]
            {
                new Avtor
                {
                    Id = 1,
                    Ime = "France",
                    Priimek = "Prešeren",
                    DatumRojstva = new DateTime(1800, 12, 3),
                    Email = "france.preseren@knjiznica.si",
                    Biografija = "Največji slovenski pesnik, avtor Zdravljice in Krsta pri Savici."
                },
                new Avtor
                {
                    Id = 2,
                    Ime = "Ivan",
                    Priimek = "Cankar",
                    DatumRojstva = new DateTime(1876, 5, 10),
                    Email = "ivan.cankar@knjiznica.si",
                    Biografija = "Slovenski pisatelj in dramatik, avtor novel Hiša Marije Pomočnice in Na klancu."
                },
                new Avtor
                {
                    Id = 3,
                    Ime = "Josip",
                    Priimek = "Jurčič",
                    DatumRojstva = new DateTime(1844, 3, 4),
                    Email = "josip.jurcic@knjiznica.si",
                    Biografija = "Prvi slovenski romanopisec, avtor romana Deseti brat."
                },
                new Avtor
                {
                    Id = 4,
                    Ime = "Dragotin",
                    Priimek = "Kette",
                    DatumRojstva = new DateTime(1876, 7, 19),
                    Email = "dragotin.kette@knjiznica.si",
                    Biografija = "Slovenski pesnik moderne, predstavnik simbolizma."
                }
            };

            // Seed Kategorije
            var kategorije = new[]
            {
                new Kategorija
                {
                    Id = 1,
                    Ime = "Poezija",
                    Opis = "Pesniška dela in zbirke pesmi"
                },
                new Kategorija
                {
                    Id = 2,
                    Ime = "Roman",
                    Opis = "Romani in daljša prozna dela"
                },
                new Kategorija
                {
                    Id = 3,
                    Ime = "Drama",
                    Opis = "Dramska dela in gledališke igre"
                },
                new Kategorija
                {
                    Id = 4,
                    Ime = "Novela",
                    Opis = "Krajša prozna dela in novele"
                },
                new Kategorija
                {
                    Id = 5,
                    Ime = "Esej",
                    Opis = "Eseji in razprave"
                }
            };

            // Seed Knjige
            var knjige = new[]
            {
                new Knjiga
                {
                    Id = 1,
                    Naslov = "Poezije",
                    ISBN = "978-961-01-1234",
                    DatumIzdaje = new DateTime(1847, 12, 29),
                    AvtorId = 1,
                    KategorijaId = 1
                },
                new Knjiga
                {
                    Id = 2,
                    Naslov = "Krst pri Savici",
                    ISBN = "978-961-01-1235",
                    DatumIzdaje = new DateTime(1836, 8, 15),
                    AvtorId = 1,
                    KategorijaId = 1
                },
                new Knjiga
                {
                    Id = 3,
                    Naslov = "Na klancu",
                    ISBN = "978-961-01-1236",
                    DatumIzdaje = new DateTime(1902, 3, 20),
                    AvtorId = 2,
                    KategorijaId = 4
                },
                new Knjiga
                {
                    Id = 4,
                    Naslov = "Hiša Marije Pomočnice",
                    ISBN = "978-961-01-1237",
                    DatumIzdaje = new DateTime(1904, 5, 15),
                    AvtorId = 2,
                    KategorijaId = 4
                },
                new Knjiga
                {
                    Id = 5,
                    Naslov = "Za narodov blagor",
                    ISBN = "978-961-01-1238",
                    DatumIzdaje = new DateTime(1901, 11, 10),
                    AvtorId = 2,
                    KategorijaId = 3
                },
                new Knjiga
                {
                    Id = 6,
                    Naslov = "Deseti brat",
                    ISBN = "978-961-01-1239",
                    DatumIzdaje = new DateTime(1866, 6, 1),
                    AvtorId = 3,
                    KategorijaId = 2
                },
                new Knjiga
                {
                    Id = 7,
                    Naslov = "Moja pomlad",
                    ISBN = "978-961-01-1240",
                    DatumIzdaje = new DateTime(1899, 4, 12),
                    AvtorId = 4,
                    KategorijaId = 1
                },
                new Knjiga
                {
                    Id = 8,
                    Naslov = "Zadnja postaja",
                    ISBN = "978-961-01-1241",
                    DatumIzdaje = new DateTime(1900, 9, 18),
                    AvtorId = 4,
                    KategorijaId = 1
                }
            };

            modelBuilder.Entity<Avtor>().HasData(avtorji);
            modelBuilder.Entity<Kategorija>().HasData(kategorije);
            modelBuilder.Entity<Knjiga>().HasData(knjige);
        }
    }
}