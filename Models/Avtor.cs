using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KnjiznicaAPI.Models
{
    public class Avtor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Ime { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Priimek { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime DatumRojstva { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Biografija { get; set; }

        // Navigation property
        public virtual ICollection<Knjiga> Knjige { get; set; } = new List<Knjiga>();
    }
}