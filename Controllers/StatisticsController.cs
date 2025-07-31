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

        // Action th?ng kê s? lý?ng sách theo t?ng tác gi?
        public async Task<IActionResult> AuthorBookCount()
        {
            var data = await _context.Authors
                .Include(a => a.Books) // Ð?m b?o l?y ðý?c danh sách sách n?u Lazy Loading không b?t
                .Select(a => new AuthorBookCountViewModel
                {
                    AuthorId = a.AuthorId,
                    Name = a.Name,
                    BookCount = a.Books.Count() // Count() ch? không ph?i Count (v? Count là delegate)
                })
                .ToListAsync();

            return View(data);
        }
    }

    // ViewModel n?i b? cho th?ng kê tác gi? và s? sách
    public class AuthorBookCountViewModel
    {
        public int AuthorId { get; set; }
        public string Name { get; set; }
        public int BookCount { get; set; }
    }
}
