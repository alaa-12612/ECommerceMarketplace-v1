using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// =========================================================
// 1. DATABASE CONFIGURATION
// =========================================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// =========================================================
// 2. DEPENDENCY INJECTION (REPOSITORIES & SERVICES)
// =========================================================

// --- Category ---
builder.Services.AddScoped<
    ECommerce.Application.Interfaces.ICategoryRepository,
    ECommerce.Infrastructure.Repositories.CategoryRepository>();

// --- Product ---
builder.Services.AddScoped<
    ECommerce.Application.Interfaces.IProductRepository,
    ECommerce.Infrastructure.Repositories.ProductRepository>();
builder.Services.AddScoped<
    ECommerce.Application.Interfaces.IProductService,
    ECommerce.Application.Services.ProductService>();

// --- Cart ---
builder.Services.AddScoped<
    ECommerce.Application.Interfaces.ICartRepository,
    ECommerce.Infrastructure.Repositories.CartRepository>();
builder.Services.AddScoped<
    ECommerce.Application.Interfaces.ICartService,
    ECommerce.Application.Services.CartService>();

// --- Order ---
builder.Services.AddScoped<
    ECommerce.Application.Interfaces.IOrderRepository,
    ECommerce.Infrastructure.Repositories.OrderRepository>();
builder.Services.AddScoped<
    ECommerce.Application.Interfaces.IOrderService,
    ECommerce.Application.Services.OrderService>();

// --- Wishlist ---
builder.Services.AddScoped<
    ECommerce.Application.Interfaces.IWishlistRepository,
    ECommerce.Infrastructure.Repositories.WishlistRepository>();
builder.Services.AddScoped<
    ECommerce.Application.Interfaces.IWishlistService,
    ECommerce.Application.Services.WishlistService>();

// --- Review ---
builder.Services.AddScoped<
    ECommerce.Application.Interfaces.IReviewRepository,
    ECommerce.Infrastructure.Repositories.ReviewRepository>();
builder.Services.AddScoped<
    ECommerce.Application.Interfaces.IReviewService,
    ECommerce.Application.Services.ReviewService>();

// --- AI Assistant ---
builder.Services.AddScoped<
    ECommerce.Application.Interfaces.IAiAssistantService,
    ECommerce.Infrastructure.Services.AiAssistantService>();

// =========================================================
// 3. IDENTITY CONFIGURATION
// =========================================================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// =========================================================
// 4. COOKIE CONFIGURATION
// =========================================================
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// =========================================================
// 5. MVC CONFIGURATION
// =========================================================
builder.Services.AddControllersWithViews();

var app = builder.Build();

// =========================================================
// 6. DATABASE SEEDING (BEST PRACTICE)
// =========================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        await DbInitializer.SeedAsync(services);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while seeding the database: {ex.Message}");
    }
}

// =========================================================
// 7. HTTP REQUEST PIPELINE
// =========================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
)
.WithStaticAssets();

app.Run();