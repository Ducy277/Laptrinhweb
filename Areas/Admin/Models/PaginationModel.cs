namespace shopflowerproject.Areas.Admin.Models
{
	public class PaginationModel
	{
		public int TotalPages { get; set; }
		public int CurrentPage { get; set; }
		public int PageSize { get; set; }
		public int MaxPagesToShow { get; set; } = 5; // Số trang tối đa muốn hiển thị liền kề
		public List<int> PagesToDisplay => GetPagesToDisplay();

		private List<int> GetPagesToDisplay()
		{
			var pages = new List<int>();
			if (TotalPages <= MaxPagesToShow)
			{
				// Hiển thị tất cả các trang nếu số trang ít hơn hoặc bằng số trang tối đa hiển thị
				for (int i = 1; i <= TotalPages; i++)
				{
					pages.Add(i);
				}
			}
			else
			{
				// Hiển thị trang đầu, trang cuối và các trang xung quanh trang hiện tại
				pages.Add(1); // Trang đầu
				if (CurrentPage > 2)
				{
					pages.Add(-1); // Dấu "..." phía trước
				}

				int start = Math.Max(2, CurrentPage - 1); // Giới hạn trang đầu gần
				int end = Math.Min(TotalPages - 1, CurrentPage + 1); // Giới hạn trang cuối gần

				for (int i = start; i <= end; i++)
				{
					pages.Add(i);
				}

				if (CurrentPage < TotalPages - 1)
				{
					pages.Add(-1); // Dấu "..." phía sau
				}
				pages.Add(TotalPages); // Trang cuối
			}
			return pages;
		}
	}

}
