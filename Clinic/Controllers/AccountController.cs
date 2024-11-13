using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using System.Threading.Tasks;
using Clinic.Controllers;
using Clinic.Models;

public class AccountController : Controller
{
    private readonly IConfiguration _configuration;

    public AccountController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Login(string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
    {
        if (ModelState.IsValid)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = @"
                    SELECT R.RoleName as Role
                    FROM Doctors D
                    JOIN Roles R ON D.RoleID = R.RoleID
                    WHERE D.Login = @Login AND D.Password = @Password";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Login", model.Username);
                    command.Parameters.AddWithValue("@Password", model.Password);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            string role = reader["Role"].ToString();
                            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.Name, model.Username),
                                new Claim(ClaimTypes.Role, role)
                            };
                            var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
                            await HttpContext.SignInAsync("CookieAuth", new ClaimsPrincipal(claimsIdentity));
                            HttpContext.Session.SetString("Username", model.Username);
                            return RedirectToLocal(returnUrl);
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                            return View(model);
                        }
                    }
                }
            }
        }

        return View(model);
    }

    private IActionResult RedirectToLocal(string returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        else
        {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("CookieAuth");
        return RedirectToAction(nameof(Login), "Account");
    }

    public IActionResult AccessDenied()
    {
        return View();
    }
}
