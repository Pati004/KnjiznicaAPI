using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KnjiznicaAPI.Models
{
    public class Knjiga
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Naslov { get; set; } = string.Empty;

        [Required]
        [MaxLength(13)]
        public string ISBN { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime DatumIzdaje { get; set; }

        // Foreign Keys
        [Required]
        public int AvtorId { get; set; }

        [Required]
        public int KategorijaId { get; set; }

        // Navigation properties
        [ForeignKey("AvtorId")]
        public virtual Avtor Avtor { get; set; } = null!;

        [ForeignKey("KategorijaId")]
        public virtual Kategorija Kategorija { get; set; } = null!;
    }
}
