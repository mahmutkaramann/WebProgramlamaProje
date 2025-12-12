using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YeniSalon.Models
{
    public class Antrenor
    {
        [Key]
        public int AntrenorId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Ad { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Soyad { get; set; } = string.Empty;

        [Required]
        [StringLength(11, MinimumLength = 11)]
        public string TCKimlikNo { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime DogumTarihi { get; set; }

        [Required]
        public Cinsiyet Cinsiyet { get; set; }

        [Required]
        [MaxLength(500)]
        public string Adres { get; set; } = string.Empty;

        [Range(0, 50)]
        public int DeneyimYili { get; set; }

        public string? UzmanlikAlanlari { get; set; }

        [Range(0, 168)]
        public int? HaftalikCalismaSaati { get; set; }

        public decimal? SaatlikUcret { get; set; }

        public string? ProfilFotoUrl { get; set; }

        public string? Aciklama { get; set; }

        public bool AktifMi { get; set; } = true;

        // Salon ilişkisi
        public int? SalonId { get; set; }
        
        [ForeignKey("SalonId")]
        public virtual Salon? Salon { get; set; }

        // Navigation Properties
        public virtual ICollection<AntrenorUzmanlik>? AntrenorUzmanliklar { get; set; }
        public virtual ICollection<AntrenorHizmet>? AntrenorHizmetler { get; set; }
        public virtual ICollection<MusaitlikSaati>? MusaitlikSaatleri { get; set; }
        public virtual ICollection<Randevu>? Randevular { get; set; }
    }
}