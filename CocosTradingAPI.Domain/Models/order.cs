using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CocosTradingAPI.Domain.Enums;

namespace CocosTradingAPI.Domain.Models
{
    [Table("orders", Schema = "public")]
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [ForeignKey("Instrument")]
        [Column("instrumentid")]
        public int InstrumentId { get; set; }
        public virtual Instrument? Instrument { get; set; }

        [ForeignKey("User")]
        [Column("userid")]
        public int UserId { get; set; }
        public virtual User? User { get; set; }

        [Column("size")]
        public int Size { get; set; }

        [Column("price", TypeName = "numeric(10,2)")]
        public decimal Price { get; set; }

        [Column("type")]
        public OrderType Type { get; set; }

        [Column("side")]
        public OrderSide Side { get; set; }  

        [Column("status")]
        public OrderStatus Status { get; set; } 

        [Column("datetime")]
        public DateTime DateTime { get; set; }
    }
}
