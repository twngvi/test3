using System.Text;

namespace test3.Helpers
{
    public static class SearchHelper
    {
        /// <summary>
        /// Tính toán khoảng cách Levenshtein giữa hai chuỗi
        /// </summary>
        /// <param name="source">Chuỗi nguồn</param>
        /// <param name="target">Chuỗi đích</param>
        /// <returns>Khoảng cách Levenshtein</returns>
        public static int LevenshteinDistance(string source, string target)
        {
            if (string.IsNullOrEmpty(source))
                return string.IsNullOrEmpty(target) ? 0 : target.Length;
            
            if (string.IsNullOrEmpty(target))
                return source.Length;

            var sourceLength = source.Length;
            var targetLength = target.Length;
            var distance = new int[sourceLength + 1, targetLength + 1];

            // Khởi tạo hàng đầu và cột đầu
            for (var i = 0; i <= sourceLength; i++)
                distance[i, 0] = i;
            
            for (var j = 0; j <= targetLength; j++)
                distance[0, j] = j;

            // Tính toán khoảng cách
            for (var i = 1; i <= sourceLength; i++)
            {
                for (var j = 1; j <= targetLength; j++)
                {
                    var cost = target[j - 1] == source[i - 1] ? 0 : 1;
                    distance[i, j] = Math.Min(
                        Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                        distance[i - 1, j - 1] + cost);
                }
            }

            return distance[sourceLength, targetLength];
        }

        /// <summary>
        /// Tính toán điểm tương đồng giữa hai chuỗi (0-1, càng cao càng giống)
        /// </summary>
        /// <param name="source">Chuỗi nguồn</param>
        /// <param name="target">Chuỗi đích</param>
        /// <returns>Điểm tương đồng từ 0 đến 1</returns>
        public static double CalculateSimilarity(string source, string target)
        {
            if (string.IsNullOrEmpty(source) && string.IsNullOrEmpty(target))
                return 1.0;
            
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
                return 0.0;

            var maxLength = Math.Max(source.Length, target.Length);
            var distance = LevenshteinDistance(source.ToLower(), target.ToLower());
            
            return 1.0 - (double)distance / maxLength;
        }

        /// <summary>
        /// Kiểm tra xem chuỗi có chứa từ khóa tìm kiếm với độ tương đồng tối thiểu
        /// </summary>
        /// <param name="text">Văn bản cần kiểm tra</param>
        /// <param name="searchTerm">Từ khóa tìm kiếm</param>
        /// <param name="threshold">Ngưỡng tương đồng tối thiểu (0-1)</param>
        /// <returns>True nếu tìm thấy kết quả phù hợp</returns>
        public static bool FuzzyContains(string text, string searchTerm, double threshold = 0.6)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(searchTerm))
                return false;

            text = text.ToLower();
            searchTerm = searchTerm.ToLower();

            // Tìm kiếm chính xác trước
            if (text.Contains(searchTerm))
                return true;

            // Tách từ và kiểm tra từng từ
            var words = text.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var searchWords = searchTerm.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var searchWord in searchWords)
            {
                foreach (var word in words)
                {
                    var similarity = CalculateSimilarity(word, searchWord);
                    if (similarity >= threshold)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Loại bỏ dấu tiếng Việt để tìm kiếm linh hoạt hơn
        /// </summary>
        /// <param name="text">Văn bản có dấu</param>
        /// <returns>Văn bản không dấu</returns>
        public static string RemoveVietnameseDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var vietnameseChars = new Dictionary<char, char>
            {
                {'à', 'a'}, {'á', 'a'}, {'ạ', 'a'}, {'ả', 'a'}, {'ã', 'a'},
                {'â', 'a'}, {'ầ', 'a'}, {'ấ', 'a'}, {'ậ', 'a'}, {'ẩ', 'a'}, {'ẫ', 'a'},
                {'ă', 'a'}, {'ằ', 'a'}, {'ắ', 'a'}, {'ặ', 'a'}, {'ẳ', 'a'}, {'ẵ', 'a'},
                {'è', 'e'}, {'é', 'e'}, {'ẹ', 'e'}, {'ẻ', 'e'}, {'ẽ', 'e'},
                {'ê', 'e'}, {'ề', 'e'}, {'ế', 'e'}, {'ệ', 'e'}, {'ể', 'e'}, {'ễ', 'e'},
                {'ì', 'i'}, {'í', 'i'}, {'ị', 'i'}, {'ỉ', 'i'}, {'ĩ', 'i'},
                {'ò', 'o'}, {'ó', 'o'}, {'ọ', 'o'}, {'ỏ', 'o'}, {'õ', 'o'},
                {'ô', 'o'}, {'ồ', 'o'}, {'ố', 'o'}, {'ộ', 'o'}, {'ổ', 'o'}, {'ỗ', 'o'},
                {'ơ', 'o'}, {'ờ', 'o'}, {'ớ', 'o'}, {'ợ', 'o'}, {'ở', 'o'}, {'ỡ', 'o'},
                {'ù', 'u'}, {'ú', 'u'}, {'ụ', 'u'}, {'ủ', 'u'}, {'ũ', 'u'},
                {'ư', 'u'}, {'ừ', 'u'}, {'ứ', 'u'}, {'ự', 'u'}, {'ử', 'u'}, {'ữ', 'u'},
                {'ỳ', 'y'}, {'ý', 'y'}, {'ỵ', 'y'}, {'ỷ', 'y'}, {'ỹ', 'y'},
                {'đ', 'd'},
                {'À', 'A'}, {'Á', 'A'}, {'Ạ', 'A'}, {'Ả', 'A'}, {'Ã', 'A'},
                {'Â', 'A'}, {'Ầ', 'A'}, {'Ấ', 'A'}, {'Ậ', 'A'}, {'Ẩ', 'A'}, {'Ẫ', 'A'},
                {'Ă', 'A'}, {'Ằ', 'A'}, {'Ắ', 'A'}, {'Ặ', 'A'}, {'Ẳ', 'A'}, {'Ẵ', 'A'},
                {'È', 'E'}, {'É', 'E'}, {'Ẹ', 'E'}, {'Ẻ', 'E'}, {'Ẽ', 'E'},
                {'Ê', 'E'}, {'Ề', 'E'}, {'Ế', 'E'}, {'Ệ', 'E'}, {'Ể', 'E'}, {'Ễ', 'E'},
                {'Ì', 'I'}, {'Í', 'I'}, {'Ị', 'I'}, {'Ỉ', 'I'}, {'Ĩ', 'I'},
                {'Ò', 'O'}, {'Ó', 'O'}, {'Ọ', 'O'}, {'Ỏ', 'O'}, {'Õ', 'O'},
                {'Ô', 'O'}, {'Ồ', 'O'}, {'Ố', 'O'}, {'Ộ', 'O'}, {'Ổ', 'O'}, {'Ỗ', 'O'},
                {'Ơ', 'O'}, {'Ờ', 'O'}, {'Ớ', 'O'}, {'Ợ', 'O'}, {'Ở', 'O'}, {'Ỡ', 'O'},
                {'Ù', 'U'}, {'Ú', 'U'}, {'Ụ', 'U'}, {'Ủ', 'U'}, {'Ũ', 'U'},
                {'Ư', 'U'}, {'Ừ', 'U'}, {'Ứ', 'U'}, {'Ự', 'U'}, {'Ử', 'U'}, {'Ữ', 'U'},
                {'Ỳ', 'Y'}, {'Ý', 'Y'}, {'Ỵ', 'Y'}, {'Ỷ', 'Y'}, {'Ỹ', 'Y'},
                {'Đ', 'D'}
            };

            var result = new StringBuilder();
            foreach (var c in text)
            {
                if (vietnameseChars.ContainsKey(c))
                    result.Append(vietnameseChars[c]);
                else
                    result.Append(c);
            }

            return result.ToString();
        }
    }
}
