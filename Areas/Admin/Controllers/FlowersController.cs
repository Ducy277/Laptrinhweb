using Microsoft.AspNetCore.Mvc;
using shopflowerproject.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Threading.Tasks;

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

        // Action Index để hiển thị danh sách hoa
        public async Task<IActionResult> Index()
        {
            List<Flowers> dshoa = new List<Flowers>();
            var connectionString = _configuration.GetConnectionString("Default");

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(@"
                    SELECT h.MaHoa, h.TenHoa, h.MoTa, h.GiaBan, h.HinhAnh, dmh.TenDanhMuc as TenDanhMuc, tl.TenHoaTuoi as LoaiHoa 
                    FROM Hoa h
                    JOIN DanhMucHoa dmh ON h.MaDanhMuc = dmh.MaDanhMuc
                    JOIN TheLoai tl ON h.MaHoaTuoi = tl.MaHoaTuoi
                    ", connection);
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
            return View(dshoa);
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

        // Action Create (GET)
        public async Task<IActionResult> Create()
        {
            ViewBag.FlowerCategory = await GetFlowerCategoryAsync();
            ViewBag.FlowerType = await GetFlowerTypeAsync();
            return View();
        }

        //Action Create(POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Flowers Flowers)
        {
            var connectionString = _configuration.GetConnectionString("Default");

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand("INSERT INTO Hoa (MaHoa, MaDanhMuc, MaHoaTuoi, TenHoa, GiaBan, MoTa, HinhAnh) " +
                                             "VALUES (@MaHoa, @MaDanhMuc, @MaHoaTuoi, @TenHoa, @GiaBan, @MoTa, @HinhAnh)", connection);
                command.Parameters.AddWithValue("@MaHoa", Flowers.MaHoa);
                command.Parameters.AddWithValue("@MaDanhMuc", Flowers.MaDanhMuc);
                command.Parameters.AddWithValue("@MaHoaTuoi", Flowers.MaHoaTuoi);
                command.Parameters.AddWithValue("@TenHoa", Flowers.TenHoa);
                command.Parameters.AddWithValue("@MoTa", Flowers.MoTa);
                command.Parameters.AddWithValue("@GiaBan", Flowers.GiaBan);
                command.Parameters.AddWithValue("@HinhAnh", Flowers.HinhAnh);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
            return RedirectToAction("Index");
        }

    }
}
