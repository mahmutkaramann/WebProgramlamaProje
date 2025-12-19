namespace YeniSalon.Models.DTOs
{
    public class SalonResponseDto
    {
        public int SalonId { get; set; }
        public string SalonAdi { get; set; } = string.Empty;
        public string? Adres { get; set; }
        public string? Telefon { get; set; }
        public string? Email { get; set; }
        public TimeSpan AcilisSaati { get; set; }
        public TimeSpan KapanisSaati { get; set; }
        public int AntrenorSayisi { get; set; }
        public int HizmetSayisi { get; set; }
        public DateTime OlusturulmaTarihi { get; set; }
        public DateTime? GuncellemeTarihi { get; set; }
    }
}