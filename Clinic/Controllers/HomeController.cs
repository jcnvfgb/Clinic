using Clinic.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Diagnostics;

namespace Clinic.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private string _username;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            _username = HttpContext.Session.GetString("Username");

            var visits = await GetVisitsByDoctorLoginAsync(_username);

            return View(visits);
        }

        private async Task<List<Visit>> GetVisitsByDoctorLoginAsync(string login)
        {
            var visits = new List<Visit>();

            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                string query = @"SELECT V.*
                                 FROM Visits V
                                 JOIN Doctors D ON V.DoctorID = D.DoctorID
                                 WHERE D.Login = @Login";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Login", login);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            visits.Add(new Visit
                            {
                                VisitID = reader.GetInt32(reader.GetOrdinal("VisitID")),
                                DoctorID = reader.GetInt32(reader.GetOrdinal("DoctorID")),
                                PatientID = reader.GetInt32(reader.GetOrdinal("PatientID")),
                                VisitDate = reader.GetDateTime(reader.GetOrdinal("VisitDate")),
                                Diagnosis = reader.GetString(reader.GetOrdinal("Diagnosis")),
                                TreatmentCost = reader.GetDouble(reader.GetOrdinal("TreatmentCost")),
                                VisitTypeID = reader.GetInt32(reader.GetOrdinal("VisitTypeID"))
                            });
                        }
                    }
                }
            }

            return visits;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Visit visit)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await AddVisitAsync(visit);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Ошибка при добавлении визита: " + ex.Message);
                    TempData["ErrorMessage"] = "Ошибка при добавлении визита: " + ex.Message;
                }
            }
            return View(visit);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var visit = await GetVisitByIdAsync(id.Value);
            if (visit == null)
            {
                return NotFound();
            }

            return View(visit);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int? id, Visit visit)
        {
            if (id != visit.VisitID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string query = @"UPDATE Visits
                                     SET DoctorID = @DoctorID,
                                         PatientID = @PatientID,
                                         VisitDate = @VisitDate,
                                         Diagnosis = @Diagnosis,
                                         TreatmentCost = @TreatmentCost,
                                         VisitTypeID = @VisitTypeID
                                     WHERE VisitID = @VisitID";

                    using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                    {
                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@DoctorID", visit.DoctorID);
                        command.Parameters.AddWithValue("@PatientID", visit.PatientID);
                        command.Parameters.AddWithValue("@VisitDate", visit.VisitDate);
                        command.Parameters.AddWithValue("@Diagnosis", visit.Diagnosis);
                        command.Parameters.AddWithValue("@TreatmentCost", visit.TreatmentCost);
                        command.Parameters.AddWithValue("@VisitTypeID", visit.VisitTypeID);
                        command.Parameters.AddWithValue("@VisitID", visit.VisitID);
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Ошибка: " + ex.Message;
                    return View(visit);
                }
            }
            return View(visit);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var visit = await GetVisitByIdAsync(id.Value);
            if (visit == null)
            {
                return NotFound();
            }

            return View(visit);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await DeleteVisitAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ошибка при удалении визита: " + ex.Message;
                return RedirectToAction(nameof(Delete), new { id = id });
            }
        }
 
        private async Task DeleteVisitAsync(int id)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("DELETE FROM Visits WHERE VisitID = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        private async Task<Visit> GetVisitByIdAsync(int id)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT * FROM Visits WHERE VisitID = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Visit
                            {
                                VisitID = reader.GetInt32(reader.GetOrdinal("VisitID")),
                                DoctorID = reader.GetInt32(reader.GetOrdinal("DoctorID")),
                                PatientID = reader.GetInt32(reader.GetOrdinal("PatientID")),
                                VisitDate = reader.GetDateTime(reader.GetOrdinal("VisitDate")),
                                Diagnosis = reader.GetString(reader.GetOrdinal("Diagnosis")),
                                TreatmentCost = reader.GetDouble(reader.GetOrdinal("TreatmentCost")),
                                VisitTypeID = reader.GetInt32(reader.GetOrdinal("VisitTypeID"))
                            };
                        }
                    }
                }
            }

            return null;
        }

        private async Task AddVisitAsync(Visit visit)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(@"INSERT INTO Visits (VisitID, DoctorID, PatientID, VisitDate, Diagnosis, TreatmentCost, VisitTypeID)
                                                     VALUES (@VisitID, @DoctorID, @PatientID, @VisitDate, @Diagnosis, @TreatmentCost, @VisitTypeID)", connection))
                {
                    command.Parameters.AddWithValue("@VisitID", visit.VisitID);
                    command.Parameters.AddWithValue("@DoctorID", visit.DoctorID);
                    command.Parameters.AddWithValue("@PatientID", visit.PatientID);
                    command.Parameters.AddWithValue("@VisitDate", visit.VisitDate);
                    command.Parameters.AddWithValue("@Diagnosis", visit.Diagnosis);
                    command.Parameters.AddWithValue("@TreatmentCost", visit.TreatmentCost);
                    command.Parameters.AddWithValue("@VisitTypeID", visit.VisitTypeID);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
