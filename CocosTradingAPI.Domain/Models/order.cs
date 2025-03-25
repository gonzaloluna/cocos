using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CocosTradingAPI.Domain.Models
{
    [Table("orders", Schema = "public")]
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("Instrument")]
        public int InstrumentId { get; set; }
        public virtual Instrument? Instrument { get; set; }  

        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User? User { get; set; }

        public int Size { get; set; }

        [Column(TypeName = "numeric(10,2)")]
        public decimal Price { get; set; }

        [MaxLength(10)]
        public string Type { get; set; } = string.Empty; 

        [MaxLength(10)]
        public string Side { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Status { get; set; } = string.Empty; 

        public DateTime DateTime { get; set; }
    }
}
