using System.ComponentModel.DataAnnotations;

namespace KnjiznicaAPI.DTOs
{
    // Avtor DTOs
    public class AvtorResponseDto
    {
        public int Id { get; set; }
        public string Ime { get; set; } = string.Empty;
        public string Priimek { get; set; } = string.Empty;
        public DateTime DatumRojstva { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Biografija { get; set; }
        public int BrojKnjig { get; set; }
        public List<KnjigaResponseDto>? Knjige { get; set; }
    }

    public class AvtorCreateDto
    {
        [Required(ErrorMessage = "Ime je obvezno.")]
        [StringLength(50, ErrorMessage = "Ime ne sme biti daljše od 50 znakov.")]
        public string Ime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Priimek je obvezen.")]
        [StringLength(50, ErrorMessage = "Priimek ne sme biti daljši od 50 znakov.")]
        public string Priimek { get; set; } = string.Empty;

        [Required(ErrorMessage = "Datum rojstva je obvezen.")]
        [DataType(DataType.Date)]
        public DateTime DatumRojstva { get; set; }

        [Required(ErrorMessage = "Email je obvezen.")]
        [EmailAddress(ErrorMessage = "Email mora biti veljaven.")]
        [StringLength(100, ErrorMessage = "Email ne sme biti daljši od 100 znakov.")]
        public string Email { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Biografija ne sme biti daljša od 1000 znakov.")]
        public string? Biografija { get; set; }
    }

    public class AvtorUpdateDto
    {
        [Required(ErrorMessage = "Ime je obvezno.")]
        [StringLength(50, ErrorMessage = "Ime ne sme biti daljše od 50 znakov.")]
        public string Ime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Priimek je obvezen.")]
        [StringLength(50, ErrorMessage = "Priimek ne sme biti daljši od 50 znakov.")]
        public string Priimek { get; set; } = string.Empty;

        [Required(ErrorMessage = "Datum rojstva je obvezen.")]
        [DataType(DataType.Date)]
        public DateTime DatumRojstva { get; set; }

        [Required(ErrorMessage = "Email je obvezen.")]
        [EmailAddress(ErrorMessage = "Email mora biti veljaven.")]
        [StringLength(100, ErrorMessage = "Email ne sme biti daljši od 100 znakov.")]
        public string Email { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Biografija ne sme biti daljša od 1000 znakov.")]
        public string? Biografija { get; set; }
    }

    // Kategorija DTOs
    public class KategorijaResponseDto
    {
        public int Id { get; set; }
        public string Ime { get; set; } = string.Empty;
        public string? Opis { get; set; }
        public int BrojKnjig { get; set; }
        public List<KnjigaResponseDto>? Knjige { get; set; }
    }

    public class KategorijaCreateDto
    {
        [Required(ErrorMessage = "Ime kategorije je obvezno.")]
        [StringLength(100, ErrorMessage = "Ime kategorije ne sme biti daljše od 100 znakov.")]
        public string Ime { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Opis ne sme biti daljši od 500 znakov.")]
        public string? Opis { get; set; }
    }

    public class KategorijaUpdateDto
    {
        [Required(ErrorMessage = "Ime kategorije je obvezno.")]
        [StringLength(100, ErrorMessage = "Ime kategorije ne sme biti daljše od 100 znakov.")]
        public string Ime { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Opis ne sme biti daljši od 500 znakov.")]
        public string? Opis { get; set; }
    }

    // Knjiga DTOs
    public class KnjigaResponseDto
    {
        public int Id { get; set; }
        public string Naslov { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public DateTime DatumIzdaje { get; set; }
        public int AvtorId { get; set; }
        public string? AvtorIme { get; set; }
        public int KategorijaId { get; set; }
        public string? KategorijaNaziv { get; set; }
    }

    public class KnjigaCreateDto
    {
        [Required(ErrorMessage = "Naslov knjige je obvezen.")]
        [StringLength(200, ErrorMessage = "Naslov knjige ne sme biti daljši od 200 znakov.")]
        public string Naslov { get; set; } = string.Empty;

        [Required(ErrorMessage = "ISBN je obvezen.")]
        [StringLength(13, MinimumLength = 10, ErrorMessage = "ISBN mora imeti med 10 in 13 znakov.")]
        [RegularExpression(@"^[\d\-]+$", ErrorMessage = "ISBN lahko vsebuje samo številke in vezaje.")]
        public string ISBN { get; set; } = string.Empty;

        [Required(ErrorMessage = "Datum izdaje je obvezen.")]
        [DataType(DataType.Date)]
        public DateTime DatumIzdaje { get; set; }

        [Required(ErrorMessage = "Avtor je obvezen.")]
        [Range(1, int.MaxValue, ErrorMessage = "Izbrati morate veljavnega avtorja.")]
        public int AvtorId { get; set; }

        [Required(ErrorMessage = "Kategorija je obvezna.")]
        [Range(1, int.MaxValue, ErrorMessage = "Izbrati morate veljavno kategorijo.")]
        public int KategorijaId { get; set; }
    }

    public class KnjigaUpdateDto
    {
        [Required(ErrorMessage = "Naslov knjige je obvezen.")]
        [StringLength(200, ErrorMessage = "Naslov knjige ne sme biti daljši od 200 znakov.")]
        public string Naslov { get; set; } = string.Empty;

        [Required(ErrorMessage = "ISBN je obvezen.")]
        [StringLength(13, MinimumLength = 10, ErrorMessage = "ISBN mora imeti med 10 in 13 znakov.")]
        [RegularExpression(@"^[\d\-]+$", ErrorMessage = "ISBN lahko vsebuje samo številke in vezaje.")]
        public string ISBN { get; set; } = string.Empty;

        [Required(ErrorMessage = "Datum izdaje je obvezen.")]
        [DataType(DataType.Date)]
        public DateTime DatumIzdaje { get; set; }

        [Required(ErrorMessage = "Avtor je obvezen.")]
        [Range(1, int.MaxValue, ErrorMessage = "Izbrati morate veljavnega avtorja.")]
        public int AvtorId { get; set; }

        [Required(ErrorMessage = "Kategorija je obvezna.")]
        [Range(1, int.MaxValue, ErrorMessage = "Izbrati morate veljavno kategorijo.")]
        public int KategorijaId { get; set; }
    }
}