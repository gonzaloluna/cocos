using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CocosTradingAPI.Domain.Models
{
    [Table("instruments", Schema = "public")]
    public class Instrument
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [MaxLength(10)]
        public string? Ticker { get; set; }

        [MaxLength(255)]
        public string? Name { get; set; }

        [MaxLength(10)]
        public string? Type { get; set; }
    }
}
