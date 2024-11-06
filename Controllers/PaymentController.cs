using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using shopflowerproject.Filters;
using shopflowerproject.Models;

namespace shopflowerproject.Controllers;
public class PaymentController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentController> _logger;
    public PaymentController(ILogger<PaymentController> logger, IConfiguration configuration)
    {
        _configuration = configuration;
        _logger = logger;
    }
    [HttpGet]
    public IActionResult HandleCartActions()
    {
        return View();
    }
    [HttpPost]
    public IActionResult HandleCartActions(string action, string maHoa, int soLuong)
    {
        string? maGioHang = HttpContext.Session.GetString("cartid");

        if (string.IsNullOrEmpty(maGioHang))
        {
            return Json(new { success = false, message = "Giỏ hàng không tìm thấy." });
        }

        if (action == "AddToCart")
        {
            string connectionString = _configuration.GetConnectionString("Default");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("addToCart", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@maGioHang", maGioHang);
                        command.Parameters.AddWithValue("@maHoa", maHoa);
                        command.Parameters.AddWithValue("@soLuong", soLuong);
                        command.ExecuteNonQuery();
                    }
                    return Json(new { success = true, message = "Đã thêm vào giỏ hàng!" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
                }
            }
        }
        else if (action == "BuyNow")
        {
            return Json(new { success = true, message = "Đang xử lý mua hàng." });
        }

        return Json(new { success = false, message = "Không xác định được hành động." });
    }


    // public async Task<IActionResult> addToCart()
    // {
    //     string? connecString = _configuration.GetConnectionString("Default");
    //     string cartId = FindIdShoppingCart();
    // }
    // public async Task<ShoppingCart> ShowInfo()
    // {
    //     var shoppingCart = new ShoppingCart();
    //     string? connecString = _configuration.GetConnectionString("Default");
    //     try
    //     {
    //         using (var connec = new SqlConnection(connecString))
    //         {
    //             await connec.OpenAsync();
    //             string? username = HttpContext.Session.GetString("username");
    //             using (SqlCommand cmd = new SqlCommand("select gh.MaGioHang,HoTen,h.TenHoa,h.MoTa,h.GiaBan,h.HinhAnh from GioHang as gh join Users as u on gh.MaUser = u.MaUser join ChiTietGioHang as ctgh on ctgh.MaGioHang = gh.MaGioHang join Hoa as h on ctgh.MaHoa = h.MaHoa where u.TenTaiKhoan = @username", connec))
    //             {
    //                 cmd.Parameters.AddWithValue("username", username);
    //                 using (var reader = await cmd.ExecuteReaderAsync())
    //                 {
    //                     while (await reader.ReadAsync())
    //                     {
    //                         shoppingCart.Add(new Flowers
    //                         {
    //                             MaHoa = reader.GetString(0),
    //                             TenHoa = reader.GetString(3),
    //                             MoTa = reader.GetString(7),
    //                             HinhAnh = reader.GetString(8),
    //                             GiaBan = reader.GetDecimal(5)
    //                         });
    //                     }
    //                 }
    //             }
    //         }
    //         return shoppingCart;
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex.Message);
    //     }
    // }
    public ShoppingCart showListFlowers()
    {
        ShoppingCart shoppingCart = new ShoppingCart();
        var connecString = _configuration.GetConnectionString("Default");
        var maGioHang = HttpContext.Session.GetString("cartid");

        if (string.IsNullOrEmpty(maGioHang))
        {
            return shoppingCart;
        }

        try
        {
            using (SqlConnection connection = new SqlConnection(connecString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("SELECT h.TenHoa,h.HinhAnh ,h.GiaBan, ctgh.SoLuong, ctgh.GiaSanPham FROM ChiTietGioHang AS ctgh JOIN Hoa AS h ON h.MaHoa = ctgh.MaHoa WHERE ctgh.MaGioHang = @cartid", connection))
                {
                    command.Parameters.AddWithValue("@cartid", maGioHang);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Flowers flower = new Flowers
                            {
                                TenHoa = reader["TenHoa"].ToString(),
                                GiaBan = Convert.ToDecimal(reader["GiaBan"]),
                                SoLuong = Convert.ToInt32(reader["SoLuong"]),
                                HinhAnh = reader["HinhAnh"].ToString(),
                            };
                            shoppingCart.Flowers.Add(flower);
                        }

                        shoppingCart.ThanhTien = shoppingCart.Flowers.Sum(f => f.GiaBan * f.SoLuong);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            TempData["Nhutngu"] = ex.Message;
        }

        return shoppingCart; // Trả về ShoppingCart đã có hoa
    }

    public string TaoMaHoaDon()
    {
        DateTime now = DateTime.Now;
        return "HD" + now.ToString("yyyyMMddHHmmss");
    }
    public async Task<IActionResult> FillInfo(){
        return View();
    }

    public ActionResult ShoppingCart()
    {
        var cartList = showListFlowers();
        return View(cartList);
    } 
    public ActionResult Payment()
    {
        return View();
    }
}