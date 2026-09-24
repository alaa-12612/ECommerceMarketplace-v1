using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }

        public bool IsApprovedSeller { get; set; }

        public bool HasRequestedToBecomeSeller { get; set; }
    }
}
