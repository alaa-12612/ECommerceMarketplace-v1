using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // 0. Ensure Migrations are Applied
            await context.Database.MigrateAsync();

            // 1. Seed Roles
            string[] roleNames = { "Admin", "Seller", "Customer" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Seed Default Admin
            string adminEmail = "admin@store.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true,
                    IsApprovedSeller = false
                };

                await userManager.CreateAsync(adminUser, "Admin@123");
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // 3. Seed Demo Seller
            string sellerEmail = "seller@store.com";
            var seller = await userManager.FindByEmailAsync(sellerEmail);
            if (seller == null)
            {
                seller = new ApplicationUser
                {
                    UserName = sellerEmail,
                    Email = sellerEmail,
                    FullName = "Demo Seller",
                    EmailConfirmed = true,
                    IsApprovedSeller = true,
                    HasRequestedToBecomeSeller = false
                };

                var result = await userManager.CreateAsync(seller, "Seller@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(seller, "Seller");
                }
            }
            else
            {
                if (!seller.IsApprovedSeller)
                {
                    seller.IsApprovedSeller = true;
                    await userManager.UpdateAsync(seller);
                }

                if (!await userManager.IsInRoleAsync(seller, "Seller"))
                {
                    await userManager.AddToRoleAsync(seller, "Seller");
                }
            }

            // 4. Seed Categories 
            var categoryNames = new[]
            {
                "Electronics",
                "Fashion",
                "Beauty",
                "Home",
                "Sports",
                "Kids",
                "Food"
            };

            foreach (var categoryName in categoryNames)
            {
                if (!await context.Categories.AnyAsync(c => c.Name == categoryName))
                {
                    await context.Categories.AddAsync(new Category
                    {
                        Name = categoryName
                    });
                }
            }

            await context.SaveChangesAsync();

            // Load categories into a dictionary for quick and safe lookups
            var categories = await context.Categories
                .ToDictionaryAsync(c => c.Name, c => c.Id, StringComparer.OrdinalIgnoreCase);

            // 5. Seed Products 
            if (!await context.Products.AnyAsync() && seller != null)
            {
                var products = new List<Product>
                {
                    // =====================================================
                    // ELECTRONICS
                    // =====================================================
                    new Product
                    {
                        Name = "Wireless Headphones",
                        Description = "High quality wireless headphones with clear sound and comfortable ear cushions.",
                        Price = 149.99m,
                        AvailableQuantity = 25,
                        CategoryId = categories["Electronics"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e"
                    },
                    new Product
                    {
                        Name = "Smart Watch",
                        Description = "Modern smart watch with fitness tracking, notifications and heart rate monitoring.",
                        Price = 249.99m,
                        AvailableQuantity = 18,
                        CategoryId = categories["Electronics"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1523275335684-37898b6baf30"
                    },
                    new Product
                    {
                        Name = "Bluetooth Speaker",
                        Description = "Portable Bluetooth speaker with powerful sound and long battery life.",
                        Price = 199.99m,
                        AvailableQuantity = 30,
                        CategoryId = categories["Electronics"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1608043152269-423dbba4e7e1"
                    },
                    new Product
                    {
                        Name = "USB-C Fast Charger",
                        Description = "Fast charging USB-C wall charger suitable for phones, tablets and other devices.",
                        Price = 99.99m,
                        AvailableQuantity = 50,
                        CategoryId = categories["Electronics"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1583863788434-e58a36330cf0"
                    },
                    new Product
                    {
                        Name = "Wireless Mouse",
                        Description = "Ergonomic wireless mouse designed for comfortable everyday use.",
                        Price = 49.99m,
                        AvailableQuantity = 40,
                        CategoryId = categories["Electronics"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1527814050087-3793815479db"
                    },
                    new Product
                    {
                        Name = "Mechanical Keyboard",
                        Description = "Mechanical keyboard with responsive keys and a durable design.",
                        Price = 129.99m,
                        AvailableQuantity = 20,
                        CategoryId = categories["Electronics"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1587829741301-dc798b83add3"
                    },
                    new Product
                    {
                        Name = "Laptop Backpack",
                        Description = "Water-resistant backpack with dedicated laptop compartment and multiple pockets.",
                        Price = 79.99m,
                        AvailableQuantity = 35,
                        CategoryId = categories["Electronics"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1553062407-98eeb64c6a62"
                    },
                    new Product
                    {
                        Name = "Smartphone Stand",
                        Description = "Adjustable desktop smartphone stand suitable for home and office.",
                        Price = 10.9m,
                        AvailableQuantity = 45,
                        CategoryId = categories["Electronics"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1586953208448-b95a79798f07"
                    },

                    // =====================================================
                    // FASHION
                    // =====================================================
                    new Product
                    {
                        Name = "Men's Casual Shirt",
                        Description = "Comfortable casual shirt made from lightweight fabric for everyday wear.",
                        Price = 100m,
                        AvailableQuantity = 30,
                        CategoryId = categories["Fashion"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1602810318383-e386cc2a3ccf"
                    },
                    new Product
                    {
                        Name = "Women's Handbag",
                        Description = "Elegant everyday handbag with spacious interior and stylish design.",
                        Price = 99.9m,
                        AvailableQuantity = 22,
                        CategoryId = categories["Fashion"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1584917865442-de89df76afd3"
                    },
                    new Product
                    {
                        Name = "Classic Denim Jacket",
                        Description = "Classic denim jacket that works perfectly with casual outfits.",
                        Price = 70.9m,
                        AvailableQuantity = 16,
                        CategoryId = categories["Fashion"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1551028719-00167b16eac5"
                    },
                    new Product
                    {
                        Name = "Men's Running Shoes",
                        Description = "Lightweight running shoes designed for daily workouts and comfortable walking.",
                        Price = 75.9m,
                        AvailableQuantity = 24,
                        CategoryId = categories["Fashion"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1542291026-7eec264c27ff"
                    },
                    new Product
                    {
                        Name = "Women's Sunglasses",
                        Description = "Stylish sunglasses with a modern frame suitable for everyday use.",
                        Price = 50.99m,
                        AvailableQuantity = 28,
                        CategoryId = categories["Fashion"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1511499767150-a48a237f0083"
                    },
                    new Product
                    {
                        Name = "Leather Wallet",
                        Description = "Compact leather wallet with multiple card slots and a classic design.",
                        Price = 20.99m,
                        AvailableQuantity = 40,
                        CategoryId = categories["Fashion"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1627123424574-724758594e93"
                    },
                    new Product
                    {
                        Name = "Women's Casual Dress",
                        Description = "Comfortable casual dress with a simple modern style for everyday occasions.",
                        Price = 99.99m,
                        AvailableQuantity = 20,
                        CategoryId = categories["Fashion"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1595777457583-95e059d581b8"
                    },
                    new Product
                    {
                        Name = "Cotton Baseball Cap",
                        Description = "Comfortable cotton baseball cap with an adjustable back strap.",
                        Price = 20.99m,
                        AvailableQuantity = 50,
                        CategoryId = categories["Fashion"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1521369909029-2afed882baee"
                    },

                    // =====================================================
                    // BEAUTY
                    // =====================================================
                    new Product
                    {
                        Name = "Skincare Set",
                        Description = "Complete skincare set designed for a simple daily skincare routine.",
                        Price = 70m,
                        AvailableQuantity = 25,
                        CategoryId = categories["Beauty"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1556228720-195a672e8a03"
                    },
                    new Product
                    {
                        Name = "Face Moisturizer",
                        Description = "Lightweight daily moisturizer suitable for maintaining soft and hydrated skin.",
                        Price = 44.99m,
                        AvailableQuantity = 35,
                        CategoryId = categories["Beauty"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1611930022073-b7a4ba5fcccd"
                    },
                    new Product
                    {
                        Name = "Perfume Bottle",
                        Description = "Elegant fragrance with a fresh and sophisticated scent.",
                        Price = 12.99m,
                        AvailableQuantity = 18,
                        CategoryId = categories["Beauty"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1541643600914-78b084683601"
                    },
                    new Product
                    {
                        Name = "Makeup Brush Set",
                        Description = "Professional-style makeup brush set for everyday beauty routines.",
                        Price = 59.99m,
                        AvailableQuantity = 30,
                        CategoryId = categories["Beauty"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1522335789203-aabd1fc54bc9"
                    },
                    new Product
                    {
                        Name = "Lipstick Collection",
                        Description = "Collection of beautiful lipstick shades suitable for different occasions.",
                        Price = 69.99m,
                        AvailableQuantity = 25,
                        CategoryId = categories["Beauty"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1586495777744-4413f21062fa"
                    },
                    new Product
                    {
                        Name = "Hair Care Set",
                        Description = "Daily hair care products designed to keep hair clean and manageable.",
                        Price = 79.99m,
                        AvailableQuantity = 22,
                        CategoryId = categories["Beauty"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1522337360788-8b13dee7a37e"
                    },

                    // =====================================================
                    // HOME
                    // =====================================================
                    new Product
                    {
                        Name = "Modern Table Lamp",
                        Description = "Modern table lamp with a minimalist design suitable for bedrooms and offices.",
                        Price = 79.9m,
                        AvailableQuantity = 20,
                        CategoryId = categories["Home"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1507473885765-e6ed057f782c"
                    },
                    new Product
                    {
                        Name = "Decorative Plant Pot",
                        Description = "Minimalist decorative plant pot perfect for indoor plants and home decoration.",
                        Price = 29.59m,
                        AvailableQuantity = 35,
                        CategoryId = categories["Home"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1485955900006-10f4d324d411"
                    },
                    new Product
                    {
                        Name = "Modern Wall Clock",
                        Description = "Simple modern wall clock that adds a stylish touch to any room.",
                        Price = 49.9m,
                        AvailableQuantity = 18,
                        CategoryId = categories["Home"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1563861826100-9cb868fdbe1c"
                    },
                    new Product
                    {
                        Name = "Ceramic Coffee Mug",
                        Description = "Durable ceramic coffee mug with a clean and modern appearance.",
                        Price = 19.99m,
                        AvailableQuantity = 60,
                        CategoryId = categories["Home"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1514228742587-6b1558fcca3d"
                    },
                    new Product
                    {
                        Name = "Cotton Bed Sheets",
                        Description = "Soft and comfortable cotton bed sheet set for a relaxing night sleep.",
                        Price = 99.99m,
                        AvailableQuantity = 25,
                        CategoryId = categories["Home"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1618221195710-dd6b41faaea6"
                    },
                    new Product
                    {
                        Name = "Decorative Cushion",
                        Description = "Soft decorative cushion designed to add comfort and style to your living room.",
                        Price = 29.99m,
                        AvailableQuantity = 40,
                        CategoryId = categories["Home"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1584100936595-c0654b55a2e2"
                    },
                    new Product
                    {
                        Name = "Kitchen Storage Set",
                        Description = "Practical storage containers for keeping kitchen ingredients organized.",
                        Price = 59.99m,
                        AvailableQuantity = 30,
                        CategoryId = categories["Home"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1556911220-bff31c812dba"
                    },
                    new Product
                    {
                        Name = "Wooden Serving Tray",
                        Description = "Elegant wooden serving tray suitable for breakfast, coffee and entertaining guests.",
                        Price = 49.99m,
                        AvailableQuantity = 20,
                        CategoryId = categories["Home"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1603199506016-b9a594b593c0"
                    },

                    // =====================================================
                    // SPORTS
                    // =====================================================
                    new Product
                    {
                        Name = "Yoga Mat",
                        Description = "Non-slip yoga mat providing comfortable support during workouts and stretching.",
                        Price = 49.99m,
                        AvailableQuantity = 35,
                        CategoryId = categories["Sports"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1601925260368-ae2f83cf8b7f"
                    },
                    new Product
                    {
                        Name = "Fitness Dumbbells",
                        Description = "Compact dumbbells suitable for home strength training workouts.",
                        Price = 89.99m,
                        AvailableQuantity = 20,
                        CategoryId = categories["Sports"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1583454110551-21f2fa2afe61"
                    },
                    new Product
                    {
                        Name = "Sports Water Bottle",
                        Description = "Reusable sports water bottle designed for workouts and outdoor activities.",
                        Price = 299.99m,
                        AvailableQuantity = 45,
                        CategoryId = categories["Sports"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1602143407151-7111542de6e8"
                    },
                    new Product
                    {
                        Name = "Football",
                        Description = "Durable football suitable for training, practice and recreational games.",
                        Price = 9.99m,
                        AvailableQuantity = 25,
                        CategoryId = categories["Sports"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1553778263-73a83bab9b0c"
                    },
                    new Product
                    {
                        Name = "Tennis Racket",
                        Description = "Lightweight tennis racket designed for beginners and recreational players.",
                        Price = 19.99m,
                        AvailableQuantity = 15,
                        CategoryId = categories["Sports"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1617083934555-ac7a7d5f8e0b"
                    },
                    new Product
                    {
                        Name = "Gym Gloves",
                        Description = "Comfortable training gloves providing grip and protection during workouts.",
                        Price = 39.99m,
                        AvailableQuantity = 30,
                        CategoryId = categories["Sports"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1584863231364-2edc166de4e2"
                    },

                    // =====================================================
                    // KIDS
                    // =====================================================
                 
                    new Product
                    {
                        Name = "Kids Backpack",
                        Description = "Colorful and lightweight backpack suitable for school and everyday activities.",
                        Price = 49.99m,
                        AvailableQuantity = 35,
                        CategoryId = categories["Kids"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1622560480605-d83c853bc5c3"
                    },
                    new Product
                    {
                        Name = "Building Blocks Set",
                        Description = "Creative building blocks set that encourages imagination and problem solving.",
                        Price = 69.99m,
                        AvailableQuantity = 20,
                        CategoryId = categories["Kids"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1587654780291-39c9404d746b"
                    },
                    new Product
                    {
                        Name = "Kids Toy Car",
                        Description = "Fun toy car with a colorful design for children's playtime.",
                        Price = 29.99m,
                        AvailableQuantity = 40,
                        CategoryId = categories["Kids"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1594787318286-3d835c1d207f"
                    },
                    new Product
                    {
                        Name = "Children's Story Book",
                        Description = "Illustrated children's story book designed for fun and educational reading.",
                        Price = 19.99m,
                        AvailableQuantity = 50,
                        CategoryId = categories["Kids"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1544947950-fa07a98d237f"
                    },
                    new Product
                    {
                        Name = "Kids Drawing Set",
                        Description = "Creative drawing set containing colorful supplies for children's artwork.",
                        Price = 20.99m,
                        AvailableQuantity = 30,
                        CategoryId = categories["Kids"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1513364776144-60967b0f800f"
                    },

                    // =====================================================
                    // FOOD
                    // =====================================================
                    new Product
                    {
                        Name = "Organic Coffee",
                        Description = "Premium organic coffee beans with a rich aroma and smooth flavor.",
                        Price = 49.99m,
                        AvailableQuantity = 35,
                        CategoryId = categories["Food"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1495474472287-4d71bcdd2085"
                    },
                    new Product
                    {
                        Name = "Premium Green Tea",
                        Description = "Premium green tea leaves with a fresh and delicate flavor.",
                        Price = 29.99m,
                        AvailableQuantity = 40,
                        CategoryId = categories["Food"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1594631252845-29fc4cc8cde9"
                    },
                    new Product
                    {
                        Name = "Dark Chocolate Bar",
                        Description = "Rich dark chocolate made with high-quality cocoa for a luxurious taste experience.",
                        Price = 19.99m,
                        AvailableQuantity = 60,
                        CategoryId = categories["Food"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1549007994-cb92caebd54b"
                    },
                    new Product
                    {
                        Name = "Mixed Nuts Pack",
                        Description = "Healthy mix of roasted nuts perfect for daily snacking.",
                        Price = 39.99m,
                        AvailableQuantity = 45,
                        CategoryId = categories["Food"],
                        SellerId = seller.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1536591375315-1989938b813d"
                    }
                };

                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
            }
        }
    }
}