using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YeniSalon.Models
{
    public class AIEgzersizOneri
    {
        [Key]
        public int OneriId { get; set; }

        [Required]
        public string KullaniciId { get; set; } = string.Empty;

        [ForeignKey("KullaniciId")]
        public virtual ApplicationUser? Kullanici { get; set; }

        [Required]
        [Display(Name = "İstek Tipi")]
        public IstekTipi IstekTipi { get; set; }

        [Display(Name = "Girilen Bilgi")]
        [MaxLength(2000)]
        public string GirilenBilgi { get; set; } = string.Empty;

        [Display(Name = "Boy (cm)")]
        [Range(100, 250)]
        public int? Boy { get; set; }

        [Display(Name = "Kilo (kg)")]
        [Range(30, 200)]
        public decimal? Kilo { get; set; }

        [Display(Name = "Yaş")]
        [Range(10, 100)]
        public int? Yas { get; set; }

        [Display(Name = "Cinsiyet")]
        public Cinsiyet? Cinsiyet { get; set; } // Artık ApplicationUser'daki Cinsiyet enum'unu kullanıyor

        [Display(Name = "Hedef")]
        [MaxLength(500)]
        public string? Hedef { get; set; }

        [Display(Name = "Fotoğraf URL")]
        public string? FotoUrl { get; set; }

        [Display(Name = "AI Yanıtı")]
        public string AIYaniti { get; set; } = string.Empty;

        [Display(Name = "AI Görsel URL")]
        public string? AIGorselUrl { get; set; }

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;
    }

    // Bu enum'u ayrı tanımlayalım (ApplicationUser'daki Cinsiyet'ten farklı)
    public enum IstekTipi
    {
        [Display(Name = "Egzersiz Önerisi")]
        EgzersizOnerisi,
        [Display(Name = "Diyet Önerisi")]
        DiyetOnerisi,
        [Display(Name = "Görsel Simülasyon")]
        GorselSimulasyon,
        [Display(Name = "Vücut Analizi")]
        VucutAnalizi
    }
}