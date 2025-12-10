using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace SporSalonu.Models
{
    public class Antrenorler
    {
        [Key]
        public int AntId { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Antrenör Adı")]
        public string AntAd { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Antrenör Soyadı")]
        public string AntSoyad { get; set; }

        [Required]
        [MaxLength(40)]
        [Display(Name = "Uzmanlık Alanı")]
        public string AntUzmanlik { get; set; }

    }
}
