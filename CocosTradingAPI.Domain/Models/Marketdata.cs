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
        [Column("id")]
        public int Id { get; set; }

        [ForeignKey("Instrument")]
        [Column("instrumentid")]
        public int InstrumentId { get; set; }

        [Column("high")]
        public decimal High { get; set; }
        
        [Column("low")]
        public decimal Low { get; set; }
        
        [Column("open")]
        public decimal Open { get; set; }
        
        [Column("close")]
        public decimal Close { get; set; }
        
        [Column("previousclose")]
        public decimal PreviousClose { get; set; }

        [Column("date")]
        public DateOnly Date { get; set; }

        public virtual Instrument? Instrument { get; set; }
    }
}
