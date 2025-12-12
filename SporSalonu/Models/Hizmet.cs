using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YeniSalon.Models
{
    public class Hizmet
    {
        [Key]
        public int HizmetId { get; set; }

        [Required]
        [MaxLength(100)]
        public string HizmetAdi { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Aciklama { get; set; } = string.Empty;

        [Required]
        [Range(15, 240)]
        public int SureDakika { get; set; }

        [Required]
        [Range(0, 10000)]
        public decimal Ucret { get; set; }

        [MaxLength(50)]
        public string? Kategori { get; set; }

        public bool AktifMi { get; set; } = true;

        // Salon ilişkisi
        public int? SalonId { get; set; }

        [ForeignKey("SalonId")]
        public virtual Salon? Salon { get; set; }

        // Navigation Properties
        public virtual ICollection<AntrenorHizmet>? AntrenorHizmetler { get; set; }
        public virtual ICollection<Randevu>? Randevular { get; set; }
    }
}