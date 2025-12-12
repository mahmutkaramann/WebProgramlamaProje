using System.ComponentModel.DataAnnotations;

namespace YeniSalon.Models
{
    public class Salon
    {
        [Key]
        public int SalonId { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Salon Adı")]
        public string SalonAdi { get; set; } = string.Empty;

        [MaxLength(500)]
        [Display(Name = "Adres")]
        public string? Adres { get; set; }

        [Phone]
        [Display(Name = "Telefon")]
        public string? Telefon { get; set; }

        [EmailAddress]
        [Display(Name = "E-posta")]
        public string? Email { get; set; }

        [Display(Name = "Açılış Saati")]
        [DataType(DataType.Time)]
        public TimeSpan AcilisSaati { get; set; }

        [Display(Name = "Kapanış Saati")]
        [DataType(DataType.Time)]
        public TimeSpan KapanisSaati { get; set; }

        public virtual ICollection<Antrenor>? Antrenorler { get; set; }
        public virtual ICollection<Hizmet>? HizmetTurleri { get; set; }
    }
}
