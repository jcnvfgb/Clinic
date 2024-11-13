using Clinic.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Clinic.Controllers
{
    public class VisitTypesController : Controller
    {
        private readonly GenericRepository _repository;
        private readonly IConfiguration _configuration;

        public VisitTypesController(IConfiguration configuration)
        {
            _configuration = configuration;
            _repository = new GenericRepository(configuration);
        }

        public async Task<IActionResult> Index()
        {
            var data = await _repository.GetAllAsync<VisitType>("VisitTypes");
            return View(data);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VisitType visitType)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await AddVisitTypeAsync(visitType);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Ошибка при добавлении типа визита: " + ex.Message);
                    TempData["ErrorMessage"] = "Ошибка при добавлении типа визита: " + ex.Message;
                }
            }
            return View(visitType);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var visitType = await GetVisitTypeByIdAsync(id.Value);
            if (visitType == null)
            {
                return NotFound();
            }

            return View(visitType);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int? id, VisitType visitType)
        {
            if (id != visitType.VisitTypeID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string query = $"UPDATE VisitTypes SET VisitTypeName = @VisitTypeName WHERE VisitTypeID = @VisitTypeID";

                    using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                    {
                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@VisitTypeName", visitType.VisitTypeName);
                        command.Parameters.AddWithValue("@VisitTypeID", visitType.VisitTypeID);
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Ошибка: " + ex.Message;
                    return View(visitType);
                }
            }
            return View(visitType);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var visitType = await GetVisitTypeByIdAsync(id.Value);
            if (visitType == null)
            {
                return NotFound();
            }

            return View(visitType);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await DeleteVisitTypeAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ошибка при удалении типа визита: " + ex.Message;
                return RedirectToAction(nameof(Delete), new { id = id });
            }
        }

        private async Task DeleteVisitTypeAsync(int id)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("DELETE FROM VisitTypes WHERE VisitTypeID = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        private async Task<VisitType> GetVisitTypeByIdAsync(int id)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT * FROM VisitTypes WHERE VisitTypeID = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new VisitType
                            {
                                VisitTypeID = reader.GetInt32(reader.GetOrdinal("VisitTypeID")),
                                VisitTypeName = reader.GetString(reader.GetOrdinal("VisitTypeName"))
                            };
                        }
                    }
                }
            }

            return null;
        }

        private async Task AddVisitTypeAsync(VisitType visitType)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("INSERT INTO VisitTypes (VisitTypeID, VisitTypeName) VALUES (@VisitTypeID, @VisitTypeName)", connection))
                {
                    command.Parameters.AddWithValue("@VisitTypeID", visitType.VisitTypeID);
                    command.Parameters.AddWithValue("@VisitTypeName", visitType.VisitTypeName);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

    }
}
