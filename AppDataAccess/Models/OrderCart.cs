using System.ComponentModel.DataAnnotations.Schema;

namespace AppDataAccess.Models
{
    [Table("order_carts")]
    public class OrderCart
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("price")]
        public int Price { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("order_id")]
        public int OrderId { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        public Order Order { get; set; }

        public Product Product { get; set; }
    }
}