using System.ComponentModel.DataAnnotations;

namespace YeniSalon.Models
{
    public class Uzmanlik
    {
        [Key]
        public int UzmanlikId { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Uzmanlık Adı")]
        public string UzmanlikAdi { get; set; } = string.Empty;

        public virtual ICollection<AntrenorUzmanlik>? AntrenorUzmanliklar { get; set; }
    }
}
