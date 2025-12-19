using System.ComponentModel.DataAnnotations;

namespace YeniSalon.Models.DTOs
{
    public class SalonCreateDto
    {
        [Required(ErrorMessage = "Salon adı zorunludur.")]
        [MaxLength(100, ErrorMessage = "Salon adı en fazla 100 karakter olabilir.")]
        public string SalonAdi { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Adres en fazla 500 karakter olabilir.")]
        public string? Adres { get; set; }

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        public string? Telefon { get; set; }

        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Açılış saati zorunludur.")]
        public TimeSpan AcilisSaati { get; set; }

        [Required(ErrorMessage = "Kapanış saati zorunludur.")]
        public TimeSpan KapanisSaati { get; set; }
    }
}