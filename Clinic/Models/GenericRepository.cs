using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Clinic.Models
{
    public class GenericRepository
    {
        private readonly string _connectionString;

        public GenericRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<T>> GetAllAsync<T>(string tableName) where T : new()
        {
            var models = new List<T>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand($"SELECT * FROM {tableName}", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var model = new T();
                            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                            foreach (var property in properties)
                            {
                                if (reader.HasColumn(property.Name))
                                {
                                    var value = reader[property.Name];
                                    if (value != DBNull.Value)
                                    {
                                        property.SetValue(model, Convert.ChangeType(value, property.PropertyType));
                                    }
                                }
                            }

                            models.Add(model);
                        }
                    }
                }
            }

            return models;
        }
    }

    public static class DataReaderExtensions
    {
        public static bool HasColumn(this IDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
