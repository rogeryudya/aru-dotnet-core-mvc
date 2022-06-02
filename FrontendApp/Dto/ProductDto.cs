using Microsoft.AspNetCore.Http;

namespace FrontendApp.Dto
{
    public class ProductDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Details { get; set; }

        public string ImageType { get; set; }

        public byte[] ImageByteArray { get; set; }

        public IFormFile ImageFile { get; set; }

        public int Price { get; set; }

        public int Category { get; set; }
    }
}