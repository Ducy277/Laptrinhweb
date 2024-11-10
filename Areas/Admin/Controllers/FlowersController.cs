using Microsoft.AspNetCore.Mvc;
using shopflowerproject.Models;
using shopflowerproject.Models.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace shopflowerproject.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class FlowersController : Controller
	{
		private readonly IConfiguration _configuration;
		public FlowersController(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		//Lấy danh sách danh mục và loại hoa
		private async Task<List<FlowerCategory>> GetFlowerCategoryAsync()
		{
			var category = new List<FlowerCategory>();

			var connectionString = _configuration.GetConnectionString("Default");

			using (var connection = new SqlConnection(connectionString))
			{
				await connection.OpenAsync();
				using (var command = new SqlCommand("SELECT MaDanhMuc, TenDanhMuc " +
													"FROM DanhMucHoa", connection))
				using (var reader = await command.ExecuteReaderAsync())
				{
					while (await reader.ReadAsync())
					{
						category.Add(new FlowerCategory
						{
							MaDanhMuc = reader.GetString("MaDanhMuc"),
							TenDanhMuc = reader.GetString("TenDanhMuc")
						});
					}
				}
			}
			return category;
		}
		private async Task<List<FlowerType>> GetFlowerTypeAsync()
		{
			var type = new List<FlowerType>();

			var connectionString = _configuration.GetConnectionString("Default");

			using (var connection = new SqlConnection(connectionString))
			{
				await connection.OpenAsync();
				using (var command = new SqlCommand("SELECT MaHoaTuoi, TenHoaTuoi " +
													"FROM TheLoai", connection))
				using (var reader = await command.ExecuteReaderAsync())
				{
					while (await reader.ReadAsync())
					{
						type.Add(new FlowerType
						{
							MaHoaTuoi = reader.GetString("MaHoaTuoi"),
							TenHoaTuoi = reader.GetString("TenHoaTuoi")
						});
					}
				}
			}
			return type;
		}

		// Action Index để hiển thị danh sách hoa
		public async Task<IActionResult> Index(int page = 1, string sortOrder = "mahoa_asc")
		{
			// Phân trang
			int pageSize = 5; //Số sản phẩm hiển thị
			int skip = (page - 1) * pageSize;

			List<Flowers> dshoa = new List<Flowers>();
			var connectionString = _configuration.GetConnectionString("Default");

			// Sắp xếp
			string orderBy = sortOrder switch
			{
				"tenhoa_asc" => "ORDER BY h.TenHoa ASC",
				"tenhoa_desc" => "ORDER BY h.TenHoa DESC",
				//"mahoa_asc" => "ORDER BY h.MaHoa ASC",
				"mahoa_desc" => "ORDER BY h.MaHoa DESC",
				_ => "ORDER BY h.MaHoa ASC" // Mặc định sắp xếp theo mã hoa, tăng dần
			};

			var commandText = $@"
				SELECT h.MaHoa, h.TenHoa, h.MoTa, h.GiaBan, h.HinhAnh, dmh.TenDanhMuc as TenDanhMuc, tl.TenHoaTuoi as LoaiHoa
				FROM Hoa h
				JOIN DanhMucHoa dmh ON h.MaDanhMuc = dmh.MaDanhMuc
				JOIN TheLoai tl ON h.MaHoaTuoi = tl.MaHoaTuoi
				{orderBy}
				OFFSET {skip} ROWS
				FETCH NEXT {pageSize} ROWS ONLY;
			";

			using (var connection = new SqlConnection(connectionString))
			{
				var command = new SqlCommand(commandText, connection);
				await connection.OpenAsync();

				using (var reader = await command.ExecuteReaderAsync())
				{
					while (await reader.ReadAsync())
					{
						var flower = new Flowers
						{
							MaHoa = reader.GetString("MaHoa"),
							TenHoa = reader.GetString("TenHoa"),
							TenDanhMuc = reader.GetString("TenDanhMuc"),
							LoaiHoa = reader.GetString("LoaiHoa"),
							MoTa = reader.GetString("MoTa"),
							GiaBan = reader.GetDecimal("GiaBan"),
							HinhAnh = reader.GetString("HinhAnh")
						};
						dshoa.Add(flower);
					}
				}
			}

			var totalItemsCommand = new SqlCommand(@"
				SELECT COUNT(*) 
				FROM Hoa h
				JOIN DanhMucHoa dmh ON h.MaDanhMuc = dmh.MaDanhMuc
				JOIN TheLoai tl ON h.MaHoaTuoi = tl.MaHoaTuoi
			", new SqlConnection(connectionString));

			await totalItemsCommand.Connection.OpenAsync();
			int totalItems = (int)await totalItemsCommand.ExecuteScalarAsync();
			int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

			// Set up ViewBag for pagination and sorting
			ViewBag.CurrentPage = page;
			ViewBag.TotalPages = totalPages;
			ViewBag.SortOrder = sortOrder;
			ViewBag.FlowerCategory = await GetFlowerCategoryAsync();
			ViewBag.FlowerType = await GetFlowerTypeAsync();

			var viewModel = new FlowerVM
			{
				FlowerList = dshoa,
				flw = new Flowers()
			};

			return View(viewModel);
		}

		// Action Create (GET)
		public async Task<IActionResult> Create()
		{
			ViewBag.FlowerCategory = await GetFlowerCategoryAsync();
			ViewBag.FlowerType = await GetFlowerTypeAsync();
			var viewModel = new FlowerVM
			{
				FlowerList = new List<Flowers>(),
				flw = new Flowers()
			};
			return View(viewModel);
		}

		//Action Create(POST)
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(FlowerVM viewModel)
		{
			var f = viewModel.flw;

			var connectionString = _configuration.GetConnectionString("Default");

			using (var connection = new SqlConnection(connectionString))
			{
				var command = new SqlCommand("INSERT INTO Hoa (MaHoa, MaDanhMuc, MaHoaTuoi, TenHoa, GiaBan, MoTa, HinhAnh) " +
											 "VALUES (@MaHoa, @MaDanhMuc, @MaHoaTuoi, @TenHoa, @GiaBan, @MoTa, @HinhAnh)", connection);
				command.Parameters.AddWithValue("@MaHoa", f.MaHoa);
				command.Parameters.AddWithValue("@MaDanhMuc", f.MaDanhMuc);
				command.Parameters.AddWithValue("@MaHoaTuoi", f.MaHoaTuoi);
				command.Parameters.AddWithValue("@TenHoa", f.TenHoa);
				command.Parameters.AddWithValue("@MoTa", f.MoTa);
				command.Parameters.AddWithValue("@GiaBan", f.GiaBan);
				command.Parameters.AddWithValue("@HinhAnh", f.HinhAnh);

				await connection.OpenAsync();
				await command.ExecuteNonQueryAsync();
			}

			return RedirectToAction("Index");
		}

		// Action Edit (GET)
		public async Task<IActionResult> Edit(string id)
		{
			var connectionString = _configuration.GetConnectionString("Default");
			var flower = new Flowers();

			using (var connection = new SqlConnection(connectionString))
			{
				var command = new SqlCommand("SELECT * FROM Hoa WHERE MaHoa = @MaHoa", connection);
				command.Parameters.AddWithValue("@MaHoa", id);

				await connection.OpenAsync();

				using (var reader = await command.ExecuteReaderAsync())
				{
					if (await reader.ReadAsync())
					{
						flower.MaHoa = reader["MaHoa"].ToString();
						flower.MaDanhMuc = reader["MaDanhMuc"].ToString();
						flower.MaHoaTuoi = reader["MaHoaTuoi"].ToString();
						flower.TenHoa = reader["TenHoa"].ToString();
						flower.GiaBan = Convert.ToDecimal(reader["GiaBan"]);
						flower.MoTa = reader["MoTa"].ToString();
						flower.HinhAnh = reader["HinhAnh"].ToString();
					}
				}
			}

			// Trả về JSON
			return Json(flower);
		}

		// Action Edit (POST)
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(FlowerVM viewModel)
		{
			var f = viewModel.flw;

			var connectionString = _configuration.GetConnectionString("Default");

			using (var connection = new SqlConnection(connectionString))
			{
				var command = new SqlCommand("UPDATE Hoa SET TenHoa = @TenHoa, MaDanhMuc = @MaDanhMuc, " +
											 "MaHoaTuoi = @MaHoaTuoi, GiaBan = @GiaBan, MoTa = @MoTa, HinhAnh = @HinhAnh " +
											 "WHERE MaHoa = @MaHoa", connection);

				command.Parameters.AddWithValue("@MaHoa", f.MaHoa);
				command.Parameters.AddWithValue("@MaDanhMuc", f.MaDanhMuc);
				command.Parameters.AddWithValue("@MaHoaTuoi", f.MaHoaTuoi);
				command.Parameters.AddWithValue("@TenHoa", f.TenHoa);
				command.Parameters.AddWithValue("@GiaBan", f.GiaBan);
				command.Parameters.AddWithValue("@MoTa", f.MoTa);
				command.Parameters.AddWithValue("@HinhAnh", f.HinhAnh);

				await connection.OpenAsync();
				await command.ExecuteNonQueryAsync();
			}

			return RedirectToAction("Index"); // Quay lại Index sau khi cập nhật
		}

		//Action Delete (POST)
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(string id)
		{
			try
			{
				var connectionString = _configuration.GetConnectionString("Default");

				using (var connection = new SqlConnection(connectionString))
				{
					var command = new SqlCommand("DELETE FROM Hoa WHERE MaHoa = @MaHoa", connection);
					command.Parameters.AddWithValue("@MaHoa", id);

					await connection.OpenAsync();
					await command.ExecuteNonQueryAsync();
				}

				return RedirectToAction("Index");
			}
			catch
			{
				return View();
			}
		}
		//Action DeleteSelected (POST)
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteSelected(string selectedIds)
		{
			try
			{
				var ids = JsonConvert.DeserializeObject<List<string>>(selectedIds);
				var connectionString = _configuration.GetConnectionString("Default");

				using (var connection = new SqlConnection(connectionString))
				{
					await connection.OpenAsync();
					foreach (var id in ids)
					{
						var command = new SqlCommand("DELETE FROM Hoa WHERE MaHoa = @MaHoa", connection);
						command.Parameters.AddWithValue("@MaHoa", id);
						await command.ExecuteNonQueryAsync();
					}
				}

				return RedirectToAction("Index");
			}
			catch
			{
				return View();
			}
		}
	}
}
