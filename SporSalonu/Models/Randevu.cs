using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YeniSalon.Models
{
    public class Randevu
    {
        [Key]
        public int RandevuId { get; set; }

        [Required]
        public string KullaniciId { get; set; } = string.Empty;

        [ForeignKey("KullaniciId")]
        public virtual ApplicationUser? Kullanici { get; set; }

        [Required]
        public int AntrenorId { get; set; }

        [ForeignKey("AntrenorId")]
        public virtual Antrenor? Antrenor { get; set; }

        [Required]
        public int HizmetId { get; set; }

        [ForeignKey("HizmetId")]
        public virtual Hizmet? Hizmet { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime RandevuTarihi { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime BitisTarihi { get; set; }

        public RandevuDurumu Durum { get; set; } = RandevuDurumu.Beklemede;

        public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;

        // OdemeId kaldırıldı

        [Display(Name = "Not")]
        [MaxLength(1000)]
        public string? Not { get; set; }
    }

    public enum RandevuDurumu
    {
        Beklemede,
        Onaylandi,
        Reddedildi,
        Tamamlandi,
        IptalEdildi,
        Gelmeyen
    }
}