namespace FrontendApp.Dto
{
    public class OrderCartProductDto
    {
        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public string Name { get; set; }

        public string ImageType { get; set; }

        public byte[] Image { get; set; }

        public int Price { get; set; }
    }
}