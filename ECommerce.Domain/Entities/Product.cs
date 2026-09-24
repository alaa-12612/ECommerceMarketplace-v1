using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int AvailableQuantity { get; set; }
        public string ImageUrl { get; set; }

       
        public int CategoryId { get; set; }
        public Category Category { get; set; }

       
        public string SellerId { get; set; }
        public ApplicationUser Seller { get; set; }
    }
}
