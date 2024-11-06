using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using shopflowerproject.Models;


using System.Data.SqlClient;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
namespace shopflowerproject.Controllers;
public class AccountController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<HomeController> _logger;
    public AccountController(ILogger<HomeController> logger, IConfiguration configuration)
    {
        _configuration = configuration;
        _logger = logger;
    }
    [HttpGet]
    public IActionResult SignUp()
    {
        return View();
    }
    public async Task<int> TotalFlowers()
    {
        int totalCount = 0;
        var connecString = _configuration.GetConnectionString("Default");
        var maGioHang = HttpContext.Session.GetString("cartid");

        if (string.IsNullOrEmpty(maGioHang))
        {
            return totalCount;
        }

        try
        {
            using (SqlConnection sql = new SqlConnection(connecString))
            {
                await sql.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("SELECT SUM(ctgh.soluong) FROM ChiTietGioHang AS ctgh JOIN Hoa AS h ON h.MaHoa = ctgh.MaHoa WHERE ctgh.MaGioHang = @cartId", sql))
                {
                    cmd.Parameters.AddWithValue("@cartId", maGioHang);


                    var result = await cmd.ExecuteScalarAsync();

                    totalCount = result != DBNull.Value ? Convert.ToInt32(result) : 0;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }

        return totalCount;
    }

    [HttpPost]
    public async Task<ActionResult> SignUp(AccountRegister register)
    {
        string? connecString = _configuration.GetConnectionString("Default");
        if (!ModelState.IsValid)
        {
            return View();
        }
        try
        {
            register.Id = Guid.NewGuid().ToString();
            using (SqlConnection connec = new SqlConnection(connecString))
            {
                await connec.OpenAsync();

                using (SqlCommand command = new SqlCommand("sp_DangKiNguoiDung", connec))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@MaUser", register.Id);
                    command.Parameters.AddWithValue("@TenTaiKhoan", register.Username);
                    command.Parameters.AddWithValue("@Password", register.Password);
                    command.Parameters.AddWithValue("@Email", register.Email);
                    command.Parameters.AddWithValue("@FullName", register.FullName);

                    await command.ExecuteNonQueryAsync();
                }
            }
            TempData["SuccessMessage"] = "Đăng ký thành công. Vui lòng đăng nhập.";
        }
        catch (SqlException ex)
        {
            if (ex.Number == 50000)
            {
                ModelState.AddModelError("", ex.Message);
            }
            else
            {
                ModelState.AddModelError("", "Có lỗi xảy ra trong quá trình đăng ký.");
            }
            return View(register);
        }
        return RedirectToAction("Login");
    }
    public string FindIdShoppingCart(string username)
    {
        string cartId = string.Empty;
        string? connecString = _configuration.GetConnectionString("Default");
        try
        {
            using (SqlConnection connecConnection = new SqlConnection(connecString))
            {
                connecConnection.Open();
                using (SqlCommand cmd = new SqlCommand("select MaGioHang from GioHang as gh join Users as u on gh.MaUser = u.MaUser where TenTaiKhoan = @TenTaiKhoan ", connecConnection))
                {
                    cmd.Parameters.AddWithValue("@TenTaiKhoan", username);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            cartId = reader["MaGioHang"].ToString();
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }
        return cartId;
    }
    [HttpGet]
    public IActionResult Login()
    {

        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Login(AccountLogin login, bool rememberMe)
    {
        string? connec = _configuration.GetConnectionString("Default");
        if (!ModelState.IsValid)
        {
            return View(login);
        }
        try
        {
            using (SqlConnection connection = new SqlConnection(connec))
            {
                await connection.OpenAsync();
                string vaitro = string.Empty;
                using (SqlCommand command = new SqlCommand("sp_DangNhapNguoiDung", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Username", login.Username);
                    command.Parameters.AddWithValue("@Password", login.Password);
                    var result = await command.ExecuteScalarAsync();
                    if (result != null)
                    {
                        using (SqlCommand cmd = new SqlCommand("SELECT vaitro FROM users WHERE tentaikhoan = @username", connection))
                        {
                            cmd.Parameters.AddWithValue("@username", login.Username);
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    vaitro = reader["vaitro"].ToString();
                                }
                            }
                        }
                        if (rememberMe)
                        {
                            var cookieOptions = new CookieOptions
                            {
                                Expires = DateTime.Now.AddDays(30),
                                IsEssential = true,
                                HttpOnly = true,
                            };
                            Response.Cookies.Append("username", login.Username, cookieOptions);
                            HttpContext.Session.SetString("username", login.Username);
                            HttpContext.Session.SetString("role", vaitro);
                            string cartIdByUser = FindIdShoppingCart(login.Username);
                            HttpContext.Session.SetString("cartid", cartIdByUser);
                            int totalFlowers = await TotalFlowers();
                            HttpContext.Session.SetInt32("totalFlowers", totalFlowers);
                        }
                        else
                        {
                            HttpContext.Session.SetString("username", login.Username);
                            FindIdShoppingCart(login.Username);
                            HttpContext.Session.SetString("role", vaitro);
                            string cartIdByUser = FindIdShoppingCart(login.Username);
                            HttpContext.Session.SetString("cartid", cartIdByUser);
                            int totalFlowers = await TotalFlowers();
                            HttpContext.Session.SetInt32("totalFlowers", totalFlowers);
                        }
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
        }
        catch (SqlException ex)
        {
            if (ex.Number == 50000)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            else
            {
                ModelState.AddModelError(string.Empty + "Lỗi", ex.Message);
            }
        }

        return View("Login");
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Remove("username");
        HttpContext.Session.Remove("role");
        Response.Cookies.Delete("username");
        HttpContext.Session.Remove("totalcart");
        return RedirectToAction("Index", "Home");
    }
}