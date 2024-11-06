using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using shopflowerproject.Filters;
[Area("User")]
[AuthorizeUser]
public class HomeUserController : Controller
{
    private readonly IConfiguration _configuration;

    public HomeUserController(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    private UserModel GetUserInfo()
    {
        UserModel user = new UserModel();
        string? connectionString = _configuration.GetConnectionString("Default");
        string? username = HttpContext.Session.GetString("username");
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            using (SqlCommand command = new SqlCommand("InfoUser", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@tentaikhoan", username);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        user = new UserModel
                        {
                            id = reader.IsDBNull(0) ? null : reader.GetString(0),
                            FullName = reader.IsDBNull(1) ? null : reader.GetString(1),
                            Email = reader.IsDBNull(2) ? null : reader.GetString(2),
                            Username = reader.IsDBNull(3) ? null : reader.GetString(3),
                            PhoneNumber = reader.IsDBNull(5) ? null : reader.GetString(5),
                            Address = reader.IsDBNull(6) ? null : reader.GetString(6),
                            Gender = reader.IsDBNull(7) ? null : reader.GetString(7),
                            BirthDate = reader.IsDBNull(8) ? DateTime.MinValue : reader.GetDateTime(8),
                            CreateAt = reader.IsDBNull(10) ? null : reader.GetDateTime(10).ToString("dd-MM-yyyy")
                        };

                    }
                }
            }
        }

        return user;
    }
    [HttpPost]
    public async Task<IActionResult> UpdateUser(UserModel user)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var error in errors)
            {
                Console.WriteLine(error.ErrorMessage);
            }
            return View(user);
        }

        var connectionString = _configuration.GetConnectionString("Default");
        string? username = HttpContext.Session.GetString("username");

        if (string.IsNullOrEmpty(username))
        {
            ModelState.AddModelError("", "User not found.");
            return View(user);
        }

        using (var connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();

            var query = "UPDATE Users SET Hoten = @FullName, Sodienthoai = @PhoneNumber, diachi = @Address, gioitinh = @Gender, ngaysinh = @DateOfBirth, Email = @Email WHERE tentaikhoan = @tentaikhoan";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@FullName", SqlDbType.NVarChar).Value = (object)user.FullName ?? DBNull.Value;
                command.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar).Value = (object)user.PhoneNumber ?? DBNull.Value;
                command.Parameters.Add("@Address", SqlDbType.NVarChar).Value = (object)user.Address ?? DBNull.Value;
                command.Parameters.Add("@Gender", SqlDbType.NVarChar).Value = (object)user.Gender ?? DBNull.Value;
                command.Parameters.Add("@DateOfBirth", SqlDbType.Date).Value = user.BirthDate;
                command.Parameters.Add("@Email", SqlDbType.NVarChar).Value = (object)user.Email ?? DBNull.Value;
                command.Parameters.Add("@tentaikhoan", SqlDbType.NVarChar).Value = username;

                await command.ExecuteNonQueryAsync();
            }
        }

        return RedirectToAction("Index");
    }

    public ActionResult Index()
    {
        var users = GetUserInfo();
        return View(users);
    }

}