using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CocosTradingAPI.Domain.Models
{
    [Table("marketdata", Schema = "public")]
    public class MarketData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("Instrument")]
        public int InstrumentId { get; set; }

        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Open { get; set; }
        public decimal Close { get; set; }
        public decimal PreviousClose { get; set; }

        public DateOnly Date { get; set; }

        public virtual Instrument? Instrument { get; set; }
    }
}
