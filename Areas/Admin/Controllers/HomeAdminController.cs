using System.Data.SqlClient;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using shopflowerproject.Filters;
using shopflowerproject.Models;

namespace shopflowerproject.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    [Area("Admin")]
    public class HomeAdminController : Controller
    {
        private readonly ILogger<HomeAdminController> _logger;
        private readonly IConfiguration _configuration;
        public HomeAdminController(ILogger<HomeAdminController> logger, IConfiguration configuration)
        {
            _configuration = configuration;
            _logger = logger;
        }
        public async Task<string> TotalEarnInYear()
        {
            string? Total = string.Empty;
            var connecString = _configuration.GetConnectionString("Default");
            using (SqlConnection connec = new SqlConnection(connecString))
            {
                await connec.OpenAsync();
                SqlCommand cmd = new SqlCommand("Select dbo.TongTienHangNam()", connec);
                var result = await cmd.ExecuteScalarAsync();
                if (result != null)
                {
                    Total = result.ToString();
                }
            }
            return Total;
        }
        public async Task<string> TotalEarnInMonth()
        {
            string Total = "0";
            var connecString = _configuration.GetConnectionString("Default");

            using (SqlConnection connec = new SqlConnection(connecString))
            {
                await connec.OpenAsync();
                SqlCommand cmd = new SqlCommand("SELECT dbo.TongTienHangThang()", connec);


                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        if (!reader.IsDBNull(0))
                        {
                            Total = reader.GetDouble(0).ToString();
                        }
                    }
                }
            }
            return Total;
        }

        public async Task<string> TotalCustomer()
        {
            string? Total = string.Empty;
            var connecString = _configuration.GetConnectionString("Default");
            using (SqlConnection connec = new SqlConnection(connecString))
            {
                await connec.OpenAsync();
                SqlCommand cmd = new SqlCommand("Select dbo.TongSoKhachHang()", connec);
                var result = await cmd.ExecuteScalarAsync();
                if (result != null)
                {
                    Total = result.ToString();
                }
            }
            return Total;
        }
        public async Task<IActionResult> Index()
        {
            OverviewAdmin overview = new OverviewAdmin();
            overview.TotalEarnInYear = await TotalEarnInYear();
            overview.TotalEarnInMonth = await TotalEarnInMonth();
            overview.TotalCustomer = await TotalCustomer();
            return View(overview);
        }
        public IActionResult ManageAccountAdmin()
        {
            return View();
        }
    }
}