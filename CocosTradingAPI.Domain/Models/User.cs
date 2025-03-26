using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CocosTradingAPI.Domain.Models
{
    [Table("users", Schema = "public")]
    public class User
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("email")]
        [MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Column("accountnumber")]
        [MaxLength(20)]
        public string AccountNumber { get; set; } = string.Empty;

        public virtual ICollection<Order>? Orders { get; set; }
    }
}
