using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YeniSalon.Models
{
    public class AntrenorUzmanlik
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AntrenorId { get; set; }

        [ForeignKey("AntrenorId")]
        public virtual Antrenor? Antrenor { get; set; }

        [Required]
        public int UzmanlikId { get; set; }

        [ForeignKey("UzmanlikId")]
        public virtual Uzmanlik? Uzmanlik { get; set; }
    }
}