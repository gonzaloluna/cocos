using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CocosTradingAPI.Domain.Enums;

namespace CocosTradingAPI.Domain.Models
{
    [Table("instruments", Schema = "public")]
    public class Instrument
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("ticker")]
        [MaxLength(10)]
        public string? Ticker { get; set; }

        [Column("name")]
        [MaxLength(255)]
        public string? Name { get; set; }

        [Required]
        [Column("type")]
        public InstrumentType Type { get; set; }
    }
}
