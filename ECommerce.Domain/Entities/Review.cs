using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ECommerce.Domain.Entities
{
    public class Review
    {
        public int Id { get; set; }

        public string CustomerId { get; set; }
        public ApplicationUser Customer { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; } 

        public string Comment { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
