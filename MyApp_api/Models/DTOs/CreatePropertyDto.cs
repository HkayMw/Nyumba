using System.Data.Common;

namespace MyApp_api.Models.DTOs;

    public class CreatePropertyDto
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
    }