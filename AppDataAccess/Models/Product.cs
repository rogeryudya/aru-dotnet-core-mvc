using System.ComponentModel.DataAnnotations.Schema;

namespace AppDataAccess.Models
{
    [Table("products")]
    public class Product
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("details")]
        public string Details { get; set; }

        [Column("image")]
        public byte[] Image { get; set; }

        [Column("image_type")]
        public string ImageType { get; set; }

        [Column("price")]
        public int Price { get; set; }

        [Column("category")]
        public int Category { get; set; }
    }
}