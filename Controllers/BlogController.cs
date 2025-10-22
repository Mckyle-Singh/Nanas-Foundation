using Microsoft.AspNetCore.Mvc;
using Nanas_Foundation.Data;
using Nanas_Foundation.Models;

namespace Nanas_Foundation.Controllers
{
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public BlogController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(BlogPost model, IFormFile PdfFile, IFormFile ProfilePhoto)
        {
            if (!ModelState.IsValid || PdfFile == null)
            {
                ModelState.AddModelError("", "Please fill all required fields and upload a PDF.");
                return View(model);
            }

            // Save PDF
            var pdfPath = Path.Combine("uploads/blogs", Guid.NewGuid() + Path.GetExtension(PdfFile.FileName));
            var fullPdfPath = Path.Combine(_env.WebRootPath, pdfPath);
            using (var stream = new FileStream(fullPdfPath, FileMode.Create))
            {
                await PdfFile.CopyToAsync(stream);
            }
            model.PdfFilePath = "/" + pdfPath.Replace("\\", "/");

            // Save Profile Photo (optional)
            if (ProfilePhoto != null)
            {
                var photoPath = Path.Combine("uploads/blogs", Guid.NewGuid() + Path.GetExtension(ProfilePhoto.FileName));
                var fullPhotoPath = Path.Combine(_env.WebRootPath, photoPath);
                using (var stream = new FileStream(fullPhotoPath, FileMode.Create))
                {
                    await ProfilePhoto.CopyToAsync(stream);
                }
                model.ProfilePhotoPath = "/" + photoPath.Replace("\\", "/");
            }

            _context.BlogPosts.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Blog");
        }
    }
}
