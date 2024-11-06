using System.Data.SqlClient;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using shopflowerproject.Models;

namespace shopflowerproject.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _configuration;
    public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
    {
        _configuration = configuration;
        _logger = logger;
    }
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
    [HttpGet]
    public IActionResult AboutUs()
    {
        return View();
    }
    public IActionResult Contact()
    {
        return View();
    }
    public IActionResult Privacy()
    {
        return View();
    }
    public IActionResult News()
    {
        return View();
    }
    public async Task<List<Flowers>> ShowTheLoai()
    {
        string? connecString = _configuration.GetConnectionString("Default");
        List<Flowers> theloai = new List<Flowers>();
        try
        {
            using (SqlConnection connect = new SqlConnection(connecString))
            {
                await connect.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("Select TenHoaTuoi from theloai", connect))
                {
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Flowers flower = new Flowers
                            {
                                TheLoai = reader.GetString(0)
                            };
                            theloai.Add(flower);
                        }

                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return theloai;
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
