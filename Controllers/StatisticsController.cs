using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using test3.Data;

namespace test3.Controllers
{
    public class StatisticsController : Controller
    {
        private readonly Test1Context _context;

        public StatisticsController(Test1Context context)
        {
            _context = context;
        }

        // Action th?ng k� s? l�?ng s�ch theo t?ng t�c gi?
        public async Task<IActionResult> AuthorBookCount()
        {
            var data = await _context.Authors
                .Include(a => a.Books) // �?m b?o l?y ��?c danh s�ch s�ch n?u Lazy Loading kh�ng b?t
                .Select(a => new AuthorBookCountViewModel
                {
                    AuthorId = a.AuthorId,
                    Name = a.Name,
                    BookCount = a.Books.Count() // Count() ch? kh�ng ph?i Count (v? Count l� delegate)
                })
                .ToListAsync();

            return View(data);
        }
    }

    // ViewModel n?i b? cho th?ng k� t�c gi? v� s? s�ch
    public class AuthorBookCountViewModel
    {
        public int AuthorId { get; set; }
        public string Name { get; set; }
        public int BookCount { get; set; }
    }
}
