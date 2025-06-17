using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KnjiznicaAPI.Models
{
    public class Kategorija
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Ime { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Opis { get; set; }

        // Navigation property
        public virtual ICollection<Knjiga> Knjige { get; set; } = new List<Knjiga>();
    }
}
