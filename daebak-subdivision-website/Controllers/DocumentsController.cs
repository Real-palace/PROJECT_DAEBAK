using daebak_subdivision_website.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace daebak_subdivision_website.Controllers
{
    public class DocumentsController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<DocumentsController> _logger;

        public DocumentsController(ApplicationDbContext dbContext, ILogger<DocumentsController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        // GET: Documents
        public async Task<IActionResult> Index()
        {
            var documents = await _dbContext.Documents
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            return View(documents);
        }

        // GET: Documents/Admin
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> AdminIndex()
        {
            var documents = await _dbContext.Documents
                .Include(d => d.CreatedBy)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            // Calculate file sizes for display
            foreach (var doc in documents)
            {
                string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", doc.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    var fileInfo = new FileInfo(fullPath);
                    doc.FormattedFileSize = FormatFileSize(fileInfo.Length);
                }
                else
                {
                    doc.FormattedFileSize = "N/A";
                }
            }

            // Create an AdminPageModel to satisfy the view's requirements
            var adminPageModel = new AdminPageModel();
            
            // Pass the documents as ViewData so the view can access them
            ViewData["Documents"] = documents;

            // Return the view from the Admin folder with the correct model type
            return View("~/Views/Admin/Documents.cshtml", adminPageModel);
        }

        // GET: Documents/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _dbContext.Documents
                .Include(d => d.CreatedBy)
                .FirstOrDefaultAsync(m => m.DocumentId == id);

            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }

        // GET: Documents/Create
        [Authorize(Roles = "ADMIN")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Documents/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create(Document document)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(document.Title))
            {
                ModelState.AddModelError("Title", "Document title is required.");
                return View(document);
            }

            // Validate file
            if (document.DocumentFile == null || document.DocumentFile.Length == 0)
            {
                ModelState.AddModelError("DocumentFile", "Please select a file to upload.");
                return View(document);
            }

            // Validate file size (10MB max)
            if (document.DocumentFile.Length > 10 * 1024 * 1024)
            {
                ModelState.AddModelError("DocumentFile", "File size must be less than 10MB.");
                return View(document);
            }

            // Validate file type
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg" };
            var fileExtension = Path.GetExtension(document.DocumentFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError("DocumentFile", "Invalid file type. Allowed types: PDF, DOC, DOCX, XLS, XLSX, PNG, JPG");
                return View(document);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Generate unique filename
                    var fileName = Guid.NewGuid().ToString() + fileExtension;
                    var relativePath = Path.Combine("documents", fileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);

                    // Ensure directory exists
                    Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documents"));

                    // Save file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await document.DocumentFile.CopyToAsync(stream);
                    }

                    // Update document properties
                    document.FilePath = "/" + relativePath.Replace("\\", "/");

                    // Set current user as creator if authenticated
                    if (User.Identity.IsAuthenticated)
                    {
                        var currentUser = await _dbContext.Users.FirstOrDefaultAsync(u => 
                            u.Username == User.Identity.Name);
                        if (currentUser != null)
                        {
                            document.CreatedById = currentUser.UserId;
                        }
                    }

                    // Set creation and update dates
                    document.CreatedAt = DateTime.Now;
                    document.UpdatedAt = DateTime.Now;

                    _dbContext.Add(document);
                    await _dbContext.SaveChangesAsync();
                    
                    _logger.LogInformation("Document created: {Title}", document.Title);
                    
                    TempData["SuccessMessage"] = "Document uploaded successfully!";
                    return RedirectToAction(nameof(AdminIndex));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading document");
                    ModelState.AddModelError("", "An error occurred while uploading the document. Please try again.");
                    return View(document);
                }
            }
            return View(document);
        }

        // GET: Documents/Edit/5
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _dbContext.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }
            return View(document);
        }

        // POST: Documents/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Edit(int id, Document document)
        {
            if (id != document.DocumentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get existing document
                    var existingDocument = await _dbContext.Documents
                        .AsNoTracking()
                        .FirstOrDefaultAsync(d => d.DocumentId == id);
                    
                    if (existingDocument == null)
                    {
                        return NotFound();
                    }

                    if (document.DocumentFile != null && document.DocumentFile.Length > 0)
                    {
                        // Delete old file if it exists
                        if (!string.IsNullOrEmpty(existingDocument.FilePath))
                        {
                            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", 
                                existingDocument.FilePath.TrimStart('/'));
                            
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // Generate unique filename
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(document.DocumentFile.FileName);
                        var relativePath = Path.Combine("documents", fileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);

                        // Save new file
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await document.DocumentFile.CopyToAsync(stream);
                        }

                        // Update document path
                        document.FilePath = "/" + relativePath.Replace("\\", "/");
                    }
                    else
                    {
                        // Keep existing file path
                        document.FilePath = existingDocument.FilePath;
                    }

                    // Preserve creation date and user
                    document.CreatedAt = existingDocument.CreatedAt;
                    document.CreatedById = existingDocument.CreatedById;
                    
                    // Update the modification date
                    document.UpdatedAt = DateTime.Now;

                    _dbContext.Update(document);
                    await _dbContext.SaveChangesAsync();
                    
                    _logger.LogInformation("Document updated: {Title}", document.Title);
                    
                    TempData["SuccessMessage"] = "Document updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DocumentExists(document.DocumentId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(AdminIndex));
            }
            return View(document);
        }

        // GET: Documents/Delete/5
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _dbContext.Documents
                .Include(d => d.CreatedBy)
                .FirstOrDefaultAsync(m => m.DocumentId == id);

            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }

        // POST: Documents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var document = await _dbContext.Documents.FindAsync(id);
            
            if (document == null)
            {
                return NotFound();
            }

            // Delete the physical file
            if (!string.IsNullOrEmpty(document.FilePath))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", 
                    document.FilePath.TrimStart('/'));
                
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _dbContext.Documents.Remove(document);
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("Document deleted: {Title}", document.Title);
            
            TempData["SuccessMessage"] = "Document deleted successfully!";
            return RedirectToAction(nameof(AdminIndex));
        }

        // GET: Documents/Download/5
        public async Task<IActionResult> Download(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _dbContext.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", 
                document.FilePath.TrimStart('/'));
            
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            _logger.LogInformation("Document downloaded: {Title}", document.Title);
            
            var contentType = GetContentType(filePath);
            var fileName = Path.GetFileName(filePath);
            
            return File(memory, contentType, fileName);
        }

        // Helper methods
        private bool DocumentExists(int id)
        {
            return _dbContext.Documents.Any(e => e.DocumentId == id);
        }

        private string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB" };
            int counter = 0;
            double size = bytes;
            
            while (size > 1024 && counter < suffixes.Length - 1)
            {
                size /= 1024;
                counter++;
            }
            
            return $"{size:0.##} {suffixes[counter]}";
        }

        private string GetContentType(string path)
        {
            var extension = Path.GetExtension(path).ToLowerInvariant();
            
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".txt" => "text/plain",
                _ => "application/octet-stream",
            };
        }
    }
}