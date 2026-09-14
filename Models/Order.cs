using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS_DotNETCore_MVC.Models
{
    public class Order
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual Data.ApplicationUser User { get; set; } = null!;

        [Required]
        public decimal TotalAmount { get; set; }

        public string? OrderInfo { get; set; }

        [Required]
        public string OrderStatus { get; set; } = "Pending"; // Pending, Success, Failed

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? TransactionId { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
