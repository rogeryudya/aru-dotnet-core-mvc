using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppDataAccess.Models
{
    [Table("orders")]
    public class Order 
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("order_date")]
        public DateTime OrderDate { get; set; }

        [Column("total_price")]
        public int TotalPrice { get; set; }

        [Column("customer_id")]
        public int CustomerId { get; set; }

        [Column("status")]
        public int Status { get; set; }

        public Customer Customer { get; set; }

        public List<OrderCart> Carts { get; set; }
    }
}