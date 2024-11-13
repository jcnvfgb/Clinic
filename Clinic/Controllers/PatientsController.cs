using Clinic.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Configuration;

namespace Clinic.Controllers
{
    public class PatientsController : Controller
    {
        private readonly GenericRepository _repository;
        private readonly IConfiguration _configuration;

        public PatientsController(IConfiguration configuration)
        {
            _configuration = configuration;
            _repository = new GenericRepository(configuration);
        }

        public async Task<IActionResult> Index()
        {
            var data = await _repository.GetAllAsync<Patient>("Patients");
            return View(data);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Patient patient)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await AddPatientAsync(patient);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Ошибка при добавлении пациента: " + ex.Message);
                    TempData["ErrorMessage"] = "Ошибка при добавлении пациента: " + ex.Message;
                }
            }
            return View(patient);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await GetPatientByIdAsync(id.Value);
            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int? id, Patient patient)
        {
            if (id != patient.PatientID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string query = $"UPDATE Patients SET LastName = @LastName, FirstName = @FirstName, MiddleName = @MiddleName, BirthYear = @BirthYear WHERE PatientID = @PatientID";

                    using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                    {
                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@LastName", patient.LastName);
                        command.Parameters.AddWithValue("@FirstName", patient.FirstName);
                        command.Parameters.AddWithValue("@MiddleName", patient.MiddleName);
                        command.Parameters.AddWithValue("@BirthYear", patient.BirthYear);
                        command.Parameters.AddWithValue("@PatientID", patient.PatientID);
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Ошибка: " + ex.Message;
                    return View(patient);
                }
            }
            return View(patient);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await GetPatientByIdAsync(id.Value);
            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await DeletePatientAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ошибка при удалении пациента: " + ex.Message;
                return RedirectToAction(nameof(Delete), new { id = id });
            }
        }

        private async Task DeletePatientAsync(int id)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("DELETE FROM Patients WHERE PatientID = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }


        private async Task<Patient> GetPatientByIdAsync(int id)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT * FROM Patients WHERE PatientID = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Patient
                            {
                                PatientID = reader.GetInt32(reader.GetOrdinal("PatientID")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                MiddleName = reader.GetString(reader.GetOrdinal("MiddleName")),
                                BirthYear = reader.GetInt32(reader.GetOrdinal("BirthYear"))
                            };
                        }
                    }
                }
            }

            return null;
        }


        private async Task AddPatientAsync(Patient patient)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("INSERT INTO Patients (PatientID, LastName, FirstName, MiddleName, BirthYear) VALUES (@PatientID, @LastName, @FirstName, @MiddleName, @BirthYear)", connection))
                {
                    command.Parameters.AddWithValue("@PatientID", patient.PatientID);
                    command.Parameters.AddWithValue("@LastName", patient.LastName);
                    command.Parameters.AddWithValue("@FirstName", patient.FirstName);
                    command.Parameters.AddWithValue("@MiddleName", patient.MiddleName);
                    command.Parameters.AddWithValue("@BirthYear", patient.BirthYear);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
