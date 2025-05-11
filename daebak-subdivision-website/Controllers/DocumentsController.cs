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
                if (!string.IsNullOrEmpty(doc.FilePath))
                {
                    string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", doc.FilePath.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath))
                    {
                        var fileInfo = new FileInfo(fullPath);
                        doc.FileSize = fileInfo.Length;
                    }
                    else
                    {
                        doc.FileSize = 0;
                    }
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
            // Instead of looking for a Create.cshtml view, redirect to the Documents admin page
            return RedirectToAction("AdminIndex");
        }

        // POST: Documents/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create(Document document)
        {
            try
            {
                // Log the submission attempt
                _logger.LogInformation("Document create attempt: {Title}", document.Title);

                // Validate the document file
                if (document.DocumentFile == null || document.DocumentFile.Length == 0)
                {
                    ModelState.AddModelError("DocumentFile", "Please select a file to upload");
                    TempData["ErrorMessage"] = "Please select a file to upload";
                    return RedirectToAction(nameof(AdminIndex));
                }

                // Validate file size
                if (document.DocumentFile.Length > 10485760) // 10MB
                {
                    ModelState.AddModelError("DocumentFile", "File size cannot exceed 10MB");
                    TempData["ErrorMessage"] = "File size cannot exceed 10MB";
                    return RedirectToAction(nameof(AdminIndex));
                }

                // Validate file type
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg" };
                var extension = Path.GetExtension(document.DocumentFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("DocumentFile", "Invalid file type");
                    TempData["ErrorMessage"] = "Invalid file type. Only PDF, DOC, DOCX, XLS, XLSX, PNG, JPG, and JPEG files are allowed.";
                    return RedirectToAction(nameof(AdminIndex));
                }

                // Generate unique filename
                var fileName = Guid.NewGuid().ToString() + extension;
                var relativePath = Path.Combine("documents", fileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);

                // Ensure directory exists
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documents"));

                // Save file to disk
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await document.DocumentFile.CopyToAsync(stream);
                }

                // Update document properties
                document.FilePath = "/" + relativePath.Replace("\\", "/");
                document.FileSize = document.DocumentFile.Length;
                
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

                // Save to database
                _dbContext.Documents.Add(document);
                await _dbContext.SaveChangesAsync();
                
                _logger.LogInformation("Document created successfully: {Title}, ID: {Id}", document.Title, document.DocumentId);
                
                TempData["SuccessMessage"] = "Document uploaded successfully!";
                return RedirectToAction(nameof(AdminIndex));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating document: {Message}", ex.Message);
                TempData["ErrorMessage"] = "An error occurred while uploading the document: " + ex.Message;
                return RedirectToAction(nameof(AdminIndex));
            }
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

        // Add this method to your DocumentsController
        [HttpGet]
        public async Task<IActionResult> GetHomeownerDocuments()
        {
            try
            {
                // Get all public documents grouped by category
                var documents = await _dbContext.Documents
                    .Where(d => d.IsPublic == true) // Assuming you have an IsPublic flag, if not, remove this condition
                    .OrderBy(d => d.Category)
                    .ThenBy(d => d.Title)
                    .ToListAsync();

                // Group documents by category
                var groupedDocuments = documents
                    .GroupBy(d => d.Category)
                    .ToDictionary(g => g.Key, g => g.ToList());

                return Json(new { success = true, data = groupedDocuments });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting homeowner documents");
                return Json(new { success = false, message = "Error retrieving documents" });
            }
        }

        // Add this method to support direct fetching for the Dashboard view
        [HttpGet]
        public async Task<IActionResult> GetDocumentsByCategory(string category)
        {
            try
            {
                _logger.LogInformation("Fetching documents for category: {Category}", category);
                
                // Fetch documents for the specified category
                var documents = await _dbContext.Documents
                    .Where(d => d.Category == category && d.IsPublic == true)
                    .OrderBy(d => d.Title)
                    .Select(d => new {
                        d.DocumentId,
                        d.Title,
                        d.Description,
                        d.FilePath,
                        d.FileSize,
                        d.Category,
                        d.CreatedAt,
                        d.UpdatedAt,
                        FormattedFileSize = FormatFileSize(d.FileSize),
                        IsPublic = d.IsPublic  // Include IsPublic flag
                    })
                    .ToListAsync();

                _logger.LogInformation("Found {Count} documents in category {Category}", documents.Count, category);
                
                // Return proper data structure including more document details
                return Json(new { 
                    success = true, 
                    data = documents,
                    count = documents.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting {Category} documents: {Message}", category, ex.Message);
                return Json(new { success = false, message = $"Error retrieving {category} documents: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DebugDocuments(string category = null)
        {
            try
            {
                var query = _dbContext.Documents.AsQueryable();
                
                // Apply category filter if provided
                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(d => d.Category == category);
                }
                
                // Only get public documents
                query = query.Where(d => d.IsPublic == true);
                
                var documents = await query
                    .OrderBy(d => d.Category)
                    .ThenBy(d => d.Title)
                    .ToListAsync();
                
                // Return detailed info for debugging
                var result = new
                {
                    success = true,
                    totalCount = documents.Count,
                    categoryCounts = documents.GroupBy(d => d.Category)
                        .Select(g => new { category = g.Key, count = g.Count() }),
                    data = documents.Select(d => new
                    {
                        d.DocumentId,
                        d.Title,
                        d.Category,
                        d.Description,
                        d.IsPublic,
                        FilePath = d.FilePath ?? "(null)",
                        FileExists = !string.IsNullOrEmpty(d.FilePath) && 
                            System.IO.File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", d.FilePath.TrimStart('/')))
                    })
                };
                
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in debug documents");
                return Json(new { success = false, message = $"Error: {ex.Message}", stackTrace = ex.StackTrace });
            }
        }

        // Helper methods
        private bool DocumentExists(int id)
        {
            return _dbContext.Documents.Any(e => e.DocumentId == id);
        }

        private string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
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
