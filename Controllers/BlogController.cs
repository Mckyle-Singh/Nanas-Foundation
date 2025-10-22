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
            var blogPosts = _context.BlogPosts
                .OrderByDescending(b => b.CreatedAt)
                .ToList();

            return View(blogPosts);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(
           [Bind("AuthorName,AuthorEmail,WebsiteLink,Title")] BlogPost model,
           IFormFile PdfFile,
           IFormFile? ProfilePhoto) // make optional
        {
            if (!ModelState.IsValid || PdfFile == null)
            {
                ModelState.AddModelError("", "Please fill all required fields and upload a PDF.");
                return View(model);
            }

            // ------------------ Save PDF ------------------
            var pdfPath = Path.Combine("uploads/blogs", Guid.NewGuid() + Path.GetExtension(PdfFile.FileName));
            var fullPdfPath = Path.Combine(_env.WebRootPath, pdfPath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPdfPath)!);

            using (var stream = new FileStream(fullPdfPath, FileMode.Create))
            {
                await PdfFile.CopyToAsync(stream);
            }

            model.PdfFilePath = "/" + pdfPath.Replace("\\", "/");
            if (ProfilePhoto != null)
            {
                var photoPath = Path.Combine("uploads/blogs", Guid.NewGuid() + Path.GetExtension(ProfilePhoto.FileName));
                var fullPhotoPath = Path.Combine(_env.WebRootPath, photoPath);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPhotoPath)!);

                using (var stream = new FileStream(fullPhotoPath, FileMode.Create))
                {
                    await ProfilePhoto.CopyToAsync(stream);
                }

                model.ProfilePhotoPath = "/" + photoPath.Replace("\\", "/");
            }
            else
            {
                model.ProfilePhotoPath = "https://i.pravatar.cc/300";
            }
            _context.BlogPosts.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Event");
        }


        public IActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blogPost = _context.BlogPosts.FirstOrDefault(b => b.Id == id);
            if (blogPost == null)
            {
                return NotFound();
            }

            return View(blogPost);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, BlogPost updatedPost, IFormFile? PdfFile, IFormFile? ProfilePhoto)
        {
            if (id != updatedPost.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingPost = await _context.BlogPosts.FindAsync(id);
                if (existingPost == null)
                {
                    return NotFound();
                }

                existingPost.Title = updatedPost.Title;
                existingPost.AuthorName = updatedPost.AuthorName;
                existingPost.AuthorEmail = updatedPost.AuthorEmail;
                existingPost.WebsiteLink = updatedPost.WebsiteLink;
                existingPost.CreatedAt = updatedPost.CreatedAt;

                if (PdfFile != null && PdfFile.Length > 0)
                {
                    var pdfFileName = Path.GetFileName(PdfFile.FileName);
                    var pdfPath = Path.Combine("wwwroot/uploads/blogs", pdfFileName);
                    using var stream = new FileStream(pdfPath, FileMode.Create);
                    await PdfFile.CopyToAsync(stream);
                    existingPost.PdfFilePath = "/uploads/blogs/" + pdfFileName;
                }

                if (ProfilePhoto != null && ProfilePhoto.Length > 0)
                {
                    var photoFileName = Path.GetFileName(ProfilePhoto.FileName);
                    var photoPath = Path.Combine("wwwroot/uploads/blogs", photoFileName);
                    using var stream = new FileStream(photoPath, FileMode.Create);
                    await ProfilePhoto.CopyToAsync(stream);
                    existingPost.ProfilePhotoPath = "/uploads/blogs/" + photoFileName;
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(updatedPost);
        }

        public IActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blogPost = _context.BlogPosts.FirstOrDefault(b => b.Id == id);
            if (blogPost == null)
            {
                return NotFound();
            }

            return View(blogPost);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            var blogPost = _context.BlogPosts.Find(id);
            if (blogPost != null)
            {
                _context.BlogPosts.Remove(blogPost);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
