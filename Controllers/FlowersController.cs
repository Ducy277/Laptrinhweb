using System.Data.SqlClient;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using shopflowerproject.Models;
public class FlowersController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<FlowersController> _logger;
    public FlowersController(IConfiguration configuration, ILogger<FlowersController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }
    public async Task<Flowers> getFlowerById(string id)
    {
        var flower = new Flowers();
        var connecString = _configuration.GetConnectionString("Default");
        using (var connec = new SqlConnection(connecString))
        {
            await connec.OpenAsync();
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM HienThiToanBoHoa where MaHoa = @MaHoa", connec))
            {
                cmd.Parameters.AddWithValue("@MaHoa", id);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        flower = new Flowers
                        {
                            MaHoa = reader.GetString(0),
                            TenHoa = reader.IsDBNull(3) ? null : reader.GetString(3),
                            MoTa = reader.IsDBNull(6) ? null : reader.GetString(6),
                            HinhAnh = reader.IsDBNull(7) ? null : reader.GetString(7),
                            GiaBan = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4)
                        };
                    }
                }
            }
        }
        return flower;
    }
    public async Task<List<Flowers>> getAllFlowers()
    {
        var flowers = new List<Flowers>();
        var connecString = _configuration.GetConnectionString("Default");

        using (var connec = new SqlConnection(connecString))
        {
            await connec.OpenAsync();

            using (SqlCommand cmd = new SqlCommand("SELECT * FROM HienThiToanBoHoa", connec))
            {
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        flowers.Add(new Flowers
                        {
                            MaHoa = reader.GetString(0),
                            TenHoa = reader.GetString(3),
                            MoTa = reader.GetString(6),
                            HinhAnh = reader.GetString(7),
                            GiaBan = reader.GetDecimal(4)
                        });
                    }
                }
            }
        }
        return flowers;
    }
    public async Task<List<Flowers>> getAllFlowersByCategory(string Category)
    {
        var flowers = new List<Flowers>();
        var connecString = _configuration.GetConnectionString("Default");

        using (var connec = new SqlConnection(connecString))
        {
            await connec.OpenAsync();

            using (SqlCommand cmd = new SqlCommand("SELECT * FROM HienThiToanBoHoa where MaHoaTuoi = @Category", connec))
            {
                cmd.Parameters.Add(new SqlParameter("@Category", Category));
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        flowers.Add(new Flowers
                        {
                            MaHoa = reader.GetString(0),
                            TenHoa = reader.GetString(3),
                            MoTa = reader.GetString(6),
                            HinhAnh = reader.GetString(7),
                            GiaBan = reader.GetDecimal(4)
                        });
                    }
                }
            }
        }
        return flowers;
    }
    public async Task<List<Flowers>> getAllFlowersByOccasion(string Occasion)
    {
        var flowers = new List<Flowers>();
        var connecString = _configuration.GetConnectionString("Default");

        using (var connec = new SqlConnection(connecString))
        {
            await connec.OpenAsync();

            using (SqlCommand cmd = new SqlCommand("SELECT * FROM HienThiToanBoHoa where MaDanhMuc = @Occasion", connec))
            {
                cmd.Parameters.Add(new SqlParameter("@Occasion", Occasion));
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        flowers.Add(new Flowers
                        {
                            MaHoa = reader.GetString(0),
                            TenHoa = reader.GetString(3),
                            MoTa = reader.GetString(6),
                            HinhAnh = reader.GetString(7),
                            GiaBan = reader.GetDecimal(4)
                        });
                    }
                }
            }
        }
        return flowers;
    }
    public async Task<IActionResult> FlowersByCategory(string Category)
    {
        var flowers = await getAllFlowersByCategory(Category);
        return View(flowers);
    }
    public async Task<IActionResult> FlowersByOccasion(string Occasion)
    {
        var flowers = await getAllFlowersByOccasion(Occasion);
        return View(flowers);
    }

    public async Task<IActionResult> Flowers()
    {
        var flowers = await getAllFlowers();
        return View(flowers);
    }

    public async Task<IActionResult> FlowerDetails(string id)
    {
        var flower = new Flowers();
        flower = await getFlowerById(id);
        return View(flower);
    }
}
