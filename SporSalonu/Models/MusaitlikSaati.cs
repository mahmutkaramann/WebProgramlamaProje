using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YeniSalon.Models
{
    public class MusaitlikSaati
    {
        [Key]
        public int MusaitlikId { get; set; }

        [Required]
        public int AntrenorId { get; set; }

        [Required]
        public DayOfWeek Gun { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan BaslangicSaati { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan BitisSaati { get; set; }

        public bool AktifMi { get; set; } = true;

        [ForeignKey("AntrenorId")]
        public virtual Antrenor? Antrenor { get; set; }

    }
}