using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.WebUI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerce.WebUI.Controllers
{
    [Authorize(Roles = "Seller,Admin")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryRepository _categoryRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(
            IProductService productService,
            ICategoryRepository categoryRepository,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _productService = productService;
            _categoryRepository = categoryRepository;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // =========================================================
        // INDEX (With Search, Filter & Sort)
        // =========================================================
        public async Task<IActionResult> Index(string? searchQuery, int? categoryId, string? sortOrder)
        {
            var userId = _userManager.GetUserId(User);

            // الاعتماد بالكامل على الـ Service لتنفيذ الفلترة، البحث، والترتيب
            var products = await _productService.GetSellerProductsAsync(
                sellerId: userId!,
                searchQuery: searchQuery,
                categoryId: categoryId,
                sortOrder: sortOrder
            );

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", categoryId);

            return View(products);
        }

        // =========================================================
        // CREATE - GET
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _categoryRepository.GetAllAsync();

            var viewModel = new ProductViewModel
            {
                Categories = categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
            };

            return View(viewModel);
        }

        // =========================================================
        // CREATE - POST
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategories(model);
                return View(model);
            }

            string? uniqueFileName = null;

            // Upload product image
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    "images",
                    "products"
                );

                Directory.CreateDirectory(uploadsFolder);

                uniqueFileName =
                    Guid.NewGuid().ToString() +
                    "_" +
                    Path.GetFileName(model.ImageFile.FileName);

                string filePath = Path.Combine(
                    uploadsFolder,
                    uniqueFileName
                );

                using (var fileStream = new FileStream(
                    filePath,
                    FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(fileStream);
                }
            }

            var product = new Product
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                AvailableQuantity = model.AvailableQuantity,
                CategoryId = model.CategoryId,
                ImageUrl = uniqueFileName,
                SellerId = _userManager.GetUserId(User)!
            };

            // استدعاء الـ Service لحفظ المنتج
            await _productService.AddProductAsync(product);

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // EDIT - GET
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            // Seller can only edit his own products.
            // Admin can edit any product.
            if (!User.IsInRole("Admin") && product.SellerId != userId)
            {
                return Forbid();
            }

            var categories = await _categoryRepository.GetAllAsync();

            var viewModel = new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                AvailableQuantity = product.AvailableQuantity,
                CategoryId = product.CategoryId,
                ExistingImageUrl = product.ImageUrl,

                Categories = categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name,
                    Selected = c.Id == product.CategoryId
                })
            };

            return View(viewModel);
        }

        // =========================================================
        // EDIT - POST
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            // Seller can only edit his own products.
            // Admin can edit any product.
            if (!User.IsInRole("Admin") && product.SellerId != userId)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                await LoadCategories(model);
                return View(model);
            }

            // Keep the old image by default
            string? imageFileName = product.ImageUrl;

            // If a new image was uploaded
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    "images",
                    "products"
                );

                Directory.CreateDirectory(uploadsFolder);

                // Delete old local image
                if (!string.IsNullOrWhiteSpace(product.ImageUrl))
                {
                    string oldImagePath = Path.Combine(
                        uploadsFolder,
                        product.ImageUrl
                    );

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Create new file name
                imageFileName =
                    Guid.NewGuid().ToString() +
                    "_" +
                    Path.GetFileName(model.ImageFile.FileName);

                string newImagePath = Path.Combine(
                    uploadsFolder,
                    imageFileName
                );

                using (var fileStream = new FileStream(
                    newImagePath,
                    FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(fileStream);
                }
            }

            // Update product information
            product.Name = model.Name;
            product.Description = model.Description;
            product.Price = model.Price;
            product.AvailableQuantity = model.AvailableQuantity;
            product.CategoryId = model.CategoryId;
            product.ImageUrl = imageFileName;

            // استدعاء الـ Service للتحديث
            await _productService.UpdateProductAsync(product);

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // DELETE - GET
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            if (!User.IsInRole("Admin") && product.SellerId != userId)
            {
                return Forbid();
            }

            return View(product);
        }

        // =========================================================
        // DELETE - POST
        // =========================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            if (!User.IsInRole("Admin") && product.SellerId != userId)
            {
                return Forbid();
            }

            // Delete local image
            if (!string.IsNullOrWhiteSpace(product.ImageUrl))
            {
                string imagePath = Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    "images",
                    "products",
                    product.ImageUrl
                );

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            // استدعاء الـ Service للحذف
            await _productService.DeleteProductAsync(id);

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // LOAD CATEGORIES
        // =========================================================
        private async Task LoadCategories(ProductViewModel model)
        {
            var categories = await _categoryRepository.GetAllAsync();

            model.Categories = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name,
                Selected = c.Id == model.CategoryId
            });
        }
    }
}