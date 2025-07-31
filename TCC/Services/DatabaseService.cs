using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using TCC.Models;

namespace TCC.Services
{
    public class DatabaseService
    {
        private const string DB_NAME = "database.db3";
        private readonly SQLiteAsyncConnection _connection;

        public DatabaseService()
        {
            _connection = new SQLiteAsyncConnection(Path.Combine(FileSystem.AppDataDirectory, DB_NAME));
            // Inicializar as tabelas
            InitializeTables();
        }

        private async void InitializeTables()
        {
            await _connection.CreateTableAsync<Passenger>();
            await _connection.CreateTableAsync<Driver>();
        }

        #region Functions that get's all drivers and passengers
        public async Task<List<Driver>> GetDrivers()
        {
            return await _connection.Table<Driver>().ToListAsync();
        }

        public async Task<List<Passenger>> GetPassengers()
        {
            return await _connection.Table<Passenger>().ToListAsync();
        }

        #endregion

        #region Functions responsible for registering/editing/deleting users

        // CORRIGIDO: Métodos estavam trocados
        public async Task<int> CreatePassenger(Passenger passenger)
        {
            return await _connection.InsertAsync(passenger);
        }

        public async Task<int> CreateDriver(Driver driver)
        {
            return await _connection.InsertAsync(driver);
        }

        public async Task<int> UpdateDriver(Driver driver)
        {
            return await _connection.UpdateAsync(driver);
        }

        public async Task<int> UpdatePassenger(Passenger passenger)
        {
            return await _connection.UpdateAsync(passenger);
        }

        public async Task<int> DeleteDriver(Driver driver)
        {
            return await _connection.DeleteAsync(driver);
        }

        public async Task<int> DeletePassenger(Passenger passenger)
        {
            return await _connection.DeleteAsync(passenger);
        }

        #endregion

        #region Login and validation methods

        public async Task<Driver> GetDriverByEmail(string email)
        {
            return await _connection.Table<Driver>()
                .Where(d => d.Email == email)
                .FirstOrDefaultAsync();
        }

        public async Task<Passenger> GetPassengerByEmail(string email)
        {
            return await _connection.Table<Passenger>()
                .Where(p => p.Email == email)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> IsEmailTaken(string email)
        {
            var driver = await GetDriverByEmail(email);
            var passenger = await GetPassengerByEmail(email);
            return driver != null || passenger != null;
        }

        public async Task<bool> IsDriverFieldUnique(string fieldName, string value)
        {
            switch (fieldName.ToLower())
            {
                case "email":
                    return await _connection.Table<Driver>().Where(d => d.Email == value).CountAsync() == 0;
                case "phonenumber":
                    return await _connection.Table<Driver>().Where(d => d.PhoneNumber == value).CountAsync() == 0;
                case "emergencyphonenumber":
                    return await _connection.Table<Driver>().Where(d => d.EmergencyPhoneNumber == value).CountAsync() == 0;
                case "rg":
                    return await _connection.Table<Driver>().Where(d => d.RG == value).CountAsync() == 0;
                case "cpf":
                    return await _connection.Table<Driver>().Where(d => d.CPF == value).CountAsync() == 0;
                case "cnh":
                    return await _connection.Table<Driver>().Where(d => d.CNH == value).CountAsync() == 0;
                default:
                    return true;
            }
        }

        public async Task<bool> IsPassengerFieldUnique(string fieldName, string value)
        {
            switch (fieldName.ToLower())
            {
                case "email":
                    return await _connection.Table<Passenger>().Where(p => p.Email == value).CountAsync() == 0;
                case "phonenumber":
                    return await _connection.Table<Passenger>().Where(p => p.PhoneNumber == value).CountAsync() == 0;
                case "emergencyphonenumber":
                    return await _connection.Table<Passenger>().Where(p => p.EmergencyPhoneNumber == value).CountAsync() == 0;
                case "rg":
                    return await _connection.Table<Passenger>().Where(p => p.RG == value).CountAsync() == 0;
                case "cpf":
                    return await _connection.Table<Passenger>().Where(p => p.CPF == value).CountAsync() == 0;
                default:
                    return true;
            }
        }

        #endregion

        public async Task<(bool IsValid, string Message)> ValidateUniqueUserData(string rg, string cpf, string email, string phone, string cnh = null, int? excludeUserId = null, string userType = null)
        {
            try
            {
                var drivers = await GetDrivers();
                var passengers = await GetPassengers();

                // Removes actual user to edit 
                if (excludeUserId.HasValue && !string.IsNullOrEmpty(userType))
                {
                    if (userType == "driver")
                        drivers = drivers.Where(d => d.Id != excludeUserId.Value).ToList();
                    else if (userType == "passenger")
                        passengers = passengers.Where(p => p.Id != excludeUserId.Value).ToList();
                }

                // Verifiy RG
                if (!string.IsNullOrEmpty(rg))
                {
                    if (drivers.Any(d => d.RG == rg) || passengers.Any(p => p.RG == rg))
                    {
                        return (false, "Este RG já está cadastrado no sistema.");
                    }
                }

                // Verifiy CPF
                if (!string.IsNullOrEmpty(cpf))
                {
                    if (drivers.Any(d => d.CPF == cpf) || passengers.Any(p => p.CPF == cpf))
                    {
                        return (false, "Este CPF já está cadastrado no sistema.");
                    }
                }

                // Verifiy Email
                if (!string.IsNullOrEmpty(email))
                {
                    if (drivers.Any(d => d.Email == email) || passengers.Any(p => p.Email == email))
                    {
                        return (false, "Este e-mail já está cadastrado no sistema.");
                    }
                }

                // Verifiy phone number
                if (!string.IsNullOrEmpty(phone))
                {
                    if (drivers.Any(d => d.PhoneNumber == phone) || passengers.Any(p => p.PhoneNumber == phone))
                    {
                        return (false, "Este número de telefone já está cadastrado no sistema.");
                    }
                }

                // Verify CNH (Drivers only)
                if (!string.IsNullOrEmpty(cnh))
                {
                    if (drivers.Any(d => d.CNH == cnh))
                    {
                        return (false, "Esta CNH já está cadastrada no sistema.");
                    }
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao validar dados: {ex.Message}");
            }
        }

        // Simplified method to check email (only)
        public async Task<bool> IsEmailTaken(string email, int? excludeUserId = null, string userType = null)
        {
            var result = await ValidateUniqueUserData(null, null, email, null, null, excludeUserId, userType);
            return !result.IsValid;
        }

        // Método para fechar a conexão quando necessário
        public async Task CloseConnection()
        {
            await _connection.CloseAsync();
        }
    }
}