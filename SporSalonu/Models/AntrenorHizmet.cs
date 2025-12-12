using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YeniSalon.Models
{
    public class AntrenorHizmet
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AntrenorId { get; set; }

        [ForeignKey("AntrenorId")]
        public virtual Antrenor? Antrenor { get; set; }

        [Required]
        public int HizmetId { get; set; }

        [ForeignKey("HizmetId")]
        public virtual Hizmet? Hizmet { get; set; }
    }
}