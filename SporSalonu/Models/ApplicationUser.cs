using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace YeniSalon.Models
{
    public class ApplicationUser : IdentityUser
    {
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

        [Range(100, 250)]
        public int? Boy { get; set; }

        [Range(30, 200)]
        public decimal? Kilo { get; set; }

        [DataType(DataType.Date)]
        public DateTime? UyelikBaslangic { get; set; }

        [DataType(DataType.Date)]
        public DateTime? UyelikBitis { get; set; }

        public UyelikTuru? UyelikTuru { get; set; }

        public bool AktifMi { get; set; } = true;

        public string? ProfilFotoUrl { get; set; }

        // Navigation Properties
        public virtual ICollection<Randevu>? Randevular { get; set; }
        public virtual ICollection<AIEgzersizOneri>? AIOnerileri { get; set; }
    }

    public enum Cinsiyet
    {
        Erkek,
        Kadın
    }

    public enum UyelikTuru
    {
        Aylik,
        UcAylik,
        AltıAylik,
        Yillik
    }
}