using Clinic.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Clinic.Controllers
{
    [Authorize]
    public class DoctorsController : Controller
    {
        private readonly GenericRepository _repository;
        IConfiguration _configuration;

        public DoctorsController(IConfiguration configuration)
        {
            _configuration = configuration;
            _repository = new GenericRepository(configuration);
        }

        public async Task<IActionResult> Index()
        {
            var data = await _repository.GetAllAsync<Doctor>("Doctors");
            return View(data);
        }

        public IActionResult Create()
        {
            return View();
        }

        // POST: Doctors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Doctor doctor)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await AddDoctorAsync(doctor);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Обработка исключений
                    ModelState.AddModelError(string.Empty, "Ошибка при добавлении доктора: " + ex.Message);
                    TempData["ErrorMessage"] = "Ошибка при добавлении доктора: " + ex.Message;
                }
            }
            return View(doctor);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await GetDoctorByIdAsync(id.Value);
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int? id, Doctor doctor)
        {
            if (id != doctor.DoctorID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string query = $"UPDATE Doctors SET LastName = @LastName, FirstName = @FirstName, MiddleName = @MiddleName, Login = @Login, Password = @Password, SpecialtyID = @SpecialtyID, CategoryID = @CategoryID, RoleID = @RoleID WHERE DoctorID = @DoctorID";

                    using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                    {
                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@LastName", doctor.LastName);
                        command.Parameters.AddWithValue("@FirstName", doctor.FirstName);
                        command.Parameters.AddWithValue("@MiddleName", doctor.MiddleName);
                        command.Parameters.AddWithValue("@Login", doctor.Login);
                        command.Parameters.AddWithValue("@Password", doctor.Password);
                        command.Parameters.AddWithValue("@SpecialtyID", doctor.SpecialtyID);
                        command.Parameters.AddWithValue("@CategoryID", doctor.CategoryID);
                        command.Parameters.AddWithValue("@RoleID", doctor.RoleID);
                        command.Parameters.AddWithValue("@DoctorID", doctor.DoctorID);
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Обработка исключений
                    TempData["ErrorMessage"] = "Ошибка: " + ex.Message;
                    return View(doctor);
                }
            }
            return View(doctor);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await GetDoctorByIdAsync(id.Value);
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await DeleteDoctorAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Обработка исключений
                TempData["ErrorMessage"] = "Ошибка при удалении доктора: " + ex.Message;
                return RedirectToAction(nameof(Delete), new { id = id });
            }
        }

        private async Task DeleteDoctorAsync(int id)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("DELETE FROM Doctors WHERE DoctorID = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await GetDoctorDetailsAsync(id.Value);
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        private async Task<DoctorDetails> GetDoctorDetailsAsync(int id)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                string QUERY = @"SELECT
                                    d.DoctorID,
                                    d.LastName,
                                    d.FirstName,
                                    d.MiddleName,
                                    d.Login,
                                    d.Password,
                                    s.SpecialtyName,
                                    c.CategoryName,
                                    r.RoleName
                                FROM
                                    Doctors d
                                JOIN
                                    Specialties s ON d.SpecialtyID = s.SpecialtyID
                                JOIN
                                    Categories c ON d.CategoryID = c.CategoryID
                                JOIN
                                    Roles r ON d.RoleID = r.RoleID
                                WHERE d.DoctorID = @id";
                using (var command = new SqlCommand(QUERY, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new DoctorDetails
                            {
                                DoctorID = reader.GetInt32(reader.GetOrdinal("DoctorID")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                MiddleName = reader.GetString(reader.GetOrdinal("MiddleName")),
                                Login = reader.GetString(reader.GetOrdinal("Login")),
                                Password = reader.GetString(reader.GetOrdinal("Password")),
                                SpecialtyID = reader.GetString(reader.GetOrdinal("SpecialtyName")),
                                CategoryID = reader.GetString(reader.GetOrdinal("CategoryName")),
                                RoleID = reader.GetString(reader.GetOrdinal("RoleName"))
                            };
                        }
                    }
                }
            }

            return null;
        }

        private async Task AddDoctorAsync(Doctor doctor)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("INSERT INTO Doctors (DoctorID, LastName, FirstName, MiddleName, Login, Password, SpecialtyID, CategoryID, RoleID) VALUES (@DoctorID, @LastName, @FirstName, @MiddleName, @Login, @Password, @SpecialtyID, @CategoryID, @RoleID)", connection))
                {
                    command.Parameters.AddWithValue("@DoctorID", doctor.DoctorID);
                    command.Parameters.AddWithValue("@LastName", doctor.LastName);
                    command.Parameters.AddWithValue("@FirstName", doctor.FirstName);
                    command.Parameters.AddWithValue("@MiddleName", doctor.MiddleName);
                    command.Parameters.AddWithValue("@Login", doctor.Login);
                    command.Parameters.AddWithValue("@Password", doctor.Password);
                    command.Parameters.AddWithValue("@SpecialtyID", doctor.SpecialtyID);
                    command.Parameters.AddWithValue("@CategoryID", doctor.CategoryID);
                    command.Parameters.AddWithValue("@RoleID", doctor.RoleID);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        private async Task<Doctor> GetDoctorByIdAsync(int id)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT * FROM Doctors WHERE DoctorID = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Doctor
                            {
                                DoctorID = reader.GetInt32(reader.GetOrdinal("DoctorID")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                MiddleName = reader.GetString(reader.GetOrdinal("MiddleName")),
                                Login = reader.GetString(reader.GetOrdinal("Login")),
                                Password = reader.GetString(reader.GetOrdinal("Password")),
                                SpecialtyID = reader.GetInt32(reader.GetOrdinal("SpecialtyID")),
                                CategoryID = reader.GetInt32(reader.GetOrdinal("CategoryID")),
                                RoleID = reader.GetInt32(reader.GetOrdinal("RoleID"))
                            };
                        }
                    }
                }
            }

            return null;
        }

    }
}
