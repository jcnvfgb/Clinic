using Clinic.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Clinic.Controllers
{
    public class RolesController : Controller
    {
        private readonly GenericRepository _repository;
        private readonly IConfiguration _configuration;

        public RolesController(IConfiguration configuration)
        {
            _configuration = configuration;
            _repository = new GenericRepository(configuration);
        }

        public async Task<IActionResult> Index()
        {
            var data = await _repository.GetAllAsync<Role>("Roles");
            return View(data);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Role role)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await AddRoleAsync(role);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Ошибка при добавлении роли: " + ex.Message);
                    TempData["ErrorMessage"] = "Ошибка при добавлении роли: " + ex.Message;
                }
            }
            return View(role);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await GetRoleByIdAsync(id.Value);
            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int? id, Role role)
        {
            if (id != role.RoleID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string query = $"UPDATE Roles SET RoleName = @RoleName WHERE RoleID = @RoleID";

                    using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                    {
                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@RoleName", role.RoleName);
                        command.Parameters.AddWithValue("@RoleID", role.RoleID);
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Ошибка: " + ex.Message;
                    return View(role);
                }
            }
            return View(role);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await GetRoleByIdAsync(id.Value);
            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await DeleteRoleAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ошибка при удалении роли: " + ex.Message;
                return RedirectToAction(nameof(Delete), new { id = id });
            }
        }

        private async Task DeleteRoleAsync(int id)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("DELETE FROM Roles WHERE RoleID = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        private async Task<Role> GetRoleByIdAsync(int id)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT * FROM Roles WHERE RoleID = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Role
                            {
                                RoleID = reader.GetInt32(reader.GetOrdinal("RoleID")),
                                RoleName = reader.GetString(reader.GetOrdinal("RoleName"))
                            };
                        }
                    }
                }
            }

            return null;
        }

        private async Task AddRoleAsync(Role role)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("INSERT INTO Roles (RoleID, RoleName) VALUES (@RoleID, @RoleName)", connection))
                {
                    command.Parameters.AddWithValue("@RoleID", role.RoleID);
                    command.Parameters.AddWithValue("@RoleName", role.RoleName);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

    }
}
