using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using test3.Data;
using test3.Models;
using test3.Helpers;

namespace test3.Controllers
{
    public class BookController : Controller
    {
        private readonly Test1Context _context;

        public BookController(Test1Context context)
        {
            _context = context;
        }

        // GET: Book
        public async Task<IActionResult> Index(string searchString)
        {
            var booksQuery = _context.Books
                .Include(b => b.Author)
                .AsQueryable();

            // Áp dụng bộ lọc tìm kiếm nếu có
            if (!string.IsNullOrEmpty(searchString))
            {
                var searchTerm = searchString.Trim();
                
                // Lấy tất cả sách để áp dụng tìm kiếm fuzzy
                var allBooks = await booksQuery.ToListAsync();
                
                var filteredBooks = allBooks.Where(b => 
                    // Tìm kiếm chính xác trước
                    b.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    b.Author.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (b.Description != null && b.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    
                    // Tìm kiếm không dấu
                    SearchHelper.RemoveVietnameseDiacritics(b.Title).Contains(
                        SearchHelper.RemoveVietnameseDiacritics(searchTerm), StringComparison.OrdinalIgnoreCase) ||
                    SearchHelper.RemoveVietnameseDiacritics(b.Author.Name).Contains(
                        SearchHelper.RemoveVietnameseDiacritics(searchTerm), StringComparison.OrdinalIgnoreCase) ||
                    
                    // Tìm kiếm fuzzy với ngưỡng 0.7
                    SearchHelper.FuzzyContains(b.Title, searchTerm, 0.7) ||
                    SearchHelper.FuzzyContains(b.Author.Name, searchTerm, 0.7) ||
                    (b.Description != null && SearchHelper.FuzzyContains(b.Description, searchTerm, 0.6))
                ).ToList();

                var books = filteredBooks.Select(b => new BookViewModel
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    Description = b.Description,
                    CoverImagePath = b.CoverImagePath,
                    AuthorId = b.AuthorId,
                    AuthorName = b.Author.Name
                }).ToList();

                // Truyền từ khóa tìm kiếm để hiển thị lại trong form
                ViewData["SearchString"] = searchString;
                ViewData["ResultCount"] = books.Count;

                return View(books);
            }
            else
            {
                var books = await booksQuery
                    .Select(b => new BookViewModel
                    {
                        BookId = b.BookId,
                        Title = b.Title,
                        Description = b.Description,
                        CoverImagePath = b.CoverImagePath,
                        AuthorId = b.AuthorId,
                        AuthorName = b.Author.Name
                    })
                    .ToListAsync();

                ViewData["ResultCount"] = books.Count;
                return View(books);
            }
        }

        // GET: Book/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Author)
                .Where(b => b.BookId == id)
                .Select(b => new BookDetailsViewModel
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    Description = b.Description,
                    CoverImagePath = b.CoverImagePath,
                    AuthorId = b.AuthorId,
                    AuthorName = b.Author.Name,
                    AuthorBio = b.Author.Bio
                })
                .FirstOrDefaultAsync();

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: Book/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new BookViewModel
            {
                Authors = await GetAuthorsSelectList()
            };
            return View(viewModel);
        }

        // POST: Book/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var book = new Book
                {
                    Title = viewModel.Title,
                    Description = viewModel.Description,
                    CoverImagePath = viewModel.CoverImagePath,
                    AuthorId = viewModel.AuthorId
                };

                _context.Add(book);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Sách đã được tạo thành công!";
                return RedirectToAction(nameof(Index));
            }

            viewModel.Authors = await GetAuthorsSelectList();
            return View(viewModel);
        }

        // GET: Book/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            var viewModel = new BookViewModel
            {
                BookId = book.BookId,
                Title = book.Title,
                Description = book.Description,
                CoverImagePath = book.CoverImagePath,
                AuthorId = book.AuthorId,
                Authors = await GetAuthorsSelectList()
            };

            return View(viewModel);
        }

        // POST: Book/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookViewModel viewModel)
        {
            if (id != viewModel.BookId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var book = await _context.Books.FindAsync(id);
                    if (book == null)
                    {
                        return NotFound();
                    }

                    book.Title = viewModel.Title;
                    book.Description = viewModel.Description;
                    book.CoverImagePath = viewModel.CoverImagePath;
                    book.AuthorId = viewModel.AuthorId;

                    _context.Update(book);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Sách đã được cập nhật thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(viewModel.BookId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            viewModel.Authors = await GetAuthorsSelectList();
            return View(viewModel);
        }

        // GET: Book/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Author)
                .Where(b => b.BookId == id)
                .Select(b => new BookDetailsViewModel
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    Description = b.Description,
                    CoverImagePath = b.CoverImagePath,
                    AuthorId = b.AuthorId,
                    AuthorName = b.Author.Name,
                    AuthorBio = b.Author.Bio
                })
                .FirstOrDefaultAsync();

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Book/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Sách đã được xóa thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.BookId == id);
        }

        private async Task<IEnumerable<SelectListItem>> GetAuthorsSelectList()
        {
            return await _context.Authors
                .Select(a => new SelectListItem
                {
                    Value = a.AuthorId.ToString(),
                    Text = a.Name
                })
                .ToListAsync();
        }
    }
}
