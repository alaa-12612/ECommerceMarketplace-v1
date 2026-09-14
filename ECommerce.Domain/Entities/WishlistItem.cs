using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Domain.Entities
{
    public class WishlistItem
    {
        public int Id { get; set; }

        public string CustomerId { get; set; }
        public ApplicationUser Customer { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public DateTime AddedOn { get; set; } = DateTime.Now;
    }
}
