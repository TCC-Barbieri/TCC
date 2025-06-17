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

        // Método para fechar a conexão quando necessário
        public async Task CloseConnection()
        {
            await _connection.CloseAsync();
        }
    }
}