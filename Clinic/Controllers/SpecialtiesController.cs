using Clinic.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Clinic.Controllers
{
    public class SpecialtiesController : Controller
    {
        private readonly GenericRepository _repository;
        private readonly IConfiguration _configuration;

        public SpecialtiesController(IConfiguration configuration)
        {
            _configuration = configuration;
            _repository = new GenericRepository(configuration);
        }

        public async Task<IActionResult> Index()
        {
            var data = await _repository.GetAllAsync<Specialty>("Specialties");
            return View(data);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Specialty specialty)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await AddSpecialtyAsync(specialty);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Ошибка при добавлении специальности: " + ex.Message);
                    TempData["ErrorMessage"] = "Ошибка при добавлении специальности: " + ex.Message;
                }
            }
            return View(specialty);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var specialty = await GetSpecialtyByIdAsync(id.Value);
            if (specialty == null)
            {
                return NotFound();
            }

            return View(specialty);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int? id, Specialty specialty)
        {
            if (id != specialty.SpecialtyID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string query = $"UPDATE Specialties SET SpecialtyName = @SpecialtyName WHERE SpecialtyID = @SpecialtyID";

                    using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                    {
                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@SpecialtyName", specialty.SpecialtyName);
                        command.Parameters.AddWithValue("@SpecialtyID", specialty.SpecialtyID);
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Ошибка: " + ex.Message;
                    return View(specialty);
                }
            }
            return View(specialty);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var specialty = await GetSpecialtyByIdAsync(id.Value);
            if (specialty == null)
            {
                return NotFound();
            }

            return View(specialty);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await DeleteSpecialtyAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ошибка при удалении специальности: " + ex.Message;
                return RedirectToAction(nameof(Delete), new { id = id });
            }
        }

        private async Task DeleteSpecialtyAsync(int id)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("DELETE FROM Specialties WHERE SpecialtyID = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }


        private async Task<Specialty> GetSpecialtyByIdAsync(int id)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT * FROM Specialties WHERE SpecialtyID = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Specialty
                            {
                                SpecialtyID = reader.GetInt32(reader.GetOrdinal("SpecialtyID")),
                                SpecialtyName = reader.GetString(reader.GetOrdinal("SpecialtyName"))
                            };
                        }
                    }
                }
            }

            return null;
        }

        private async Task AddSpecialtyAsync(Specialty specialty)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("INSERT INTO Specialties (SpecialtyID, SpecialtyName) VALUES (@SpecialtyID, @SpecialtyName)", connection))
                {
                    command.Parameters.AddWithValue("@SpecialtyID", specialty.SpecialtyID);
                    command.Parameters.AddWithValue("@SpecialtyName", specialty.SpecialtyName);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
