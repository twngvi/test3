using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace test3.Models
{
    public class BookViewModel
    {
        public int BookId { get; set; }

        [Required(ErrorMessage = "Tiêu đề sách là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = null!;

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [StringLength(255, ErrorMessage = "Đường dẫn ảnh không được vượt quá 255 ký tự")]
        [Display(Name = "Đường dẫn ảnh bìa")]
        public string? CoverImagePath { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn tác giả")]
        [Display(Name = "Tác giả")]
        public int AuthorId { get; set; }

        [Display(Name = "Tác giả")]
        public string? AuthorName { get; set; }

        // Dropdown list for authors
        public IEnumerable<SelectListItem>? Authors { get; set; }
    }

    public class BookDetailsViewModel
    {
        public int BookId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? CoverImagePath { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = null!;
        public string? AuthorBio { get; set; }
    }
}
