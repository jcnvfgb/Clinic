using Clinic.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Clinic.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class CategoriesController : Controller
    {
        private readonly GenericRepository _repository;
        IConfiguration _configuration;

        public CategoriesController(IConfiguration configuration)
        {
            _configuration = configuration;
            _repository = new GenericRepository(configuration);
        }

        public async Task<IActionResult> Index()
        {
            var data = await _repository.GetAllAsync<Category>("Categories");
            return View(data);
        }

        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await AddCategoryAsync(category);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Обработка исключений
                    ModelState.AddModelError(string.Empty, "Ошибка при добавлении категории: " + ex.Message);
                    TempData["ErrorMessage"] = "Ошибка при добавлении категории: " + ex.Message;
                }
            }
            return View(category);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await GetCategoryByIdAsync(id.Value);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int? id, Category category)
        {
            if (id != category.CategoryID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string query = $"UPDATE Categories SET CategoryName = @CategoryName WHERE CategoryID = @CategoryID";

                    using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                    {
                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@CategoryName", category.CategoryName);
                        command.Parameters.AddWithValue("@CategoryID", category.CategoryID);
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Обработка исключений
                    TempData["ErrorMessage"] = "Ошибка: " + ex.Message;
                    return View(category);
                }
            }
            return View(category);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await GetCategoryByIdAsync(id.Value);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await DeleteCategoryAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Обработка исключений
                TempData["ErrorMessage"] = "Ошибка при удалении категории: " + ex.Message;
                return RedirectToAction(nameof(Delete), new { id = id });
            }
        }

        private async Task<Category> GetCategoryByIdAsync(int id)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT * FROM Categories WHERE CategoryID = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Category
                            {
                                CategoryID = reader.GetInt32(reader.GetOrdinal("CategoryID")),
                                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName"))
                            };
                        }
                    }
                }
            }

            return null;
        }

        private async Task AddCategoryAsync(Category category)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("INSERT INTO Categories (CategoryID, CategoryName) VALUES (@CategoryID, @CategoryName)", connection))
                {
                    command.Parameters.AddWithValue("@CategoryID", category.CategoryID);
                    command.Parameters.AddWithValue("@CategoryName", category.CategoryName);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        private async Task DeleteCategoryAsync(int id)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("DELETE FROM Categories WHERE CategoryID = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
