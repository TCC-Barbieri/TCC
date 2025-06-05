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
            _connection.CreateTableAsync<Passenger>();
            _connection.CreateTableAsync<Driver>();
        }

        #region Functions that get's all drivers and passengers
        public async Task<List<Driver>> GetDrivers()
        {
            return await _connection.Table<Driver>().ToListAsync(); // Retrieves all contacts
        }

        public async Task<List<Passenger>> GetPassengers()
        {
            return await _connection.Table<Passenger>().ToListAsync(); // Retrieves all contacts
        }

        #endregion

        #region Functions responsible for registering/editing/deleting users
        public async Task CreatePassenger(Driver driver)
        {
            await _connection.InsertAsync(driver); // Inserts new driver
        }

        public async Task CreateDriver(Passenger passenger)
        {
            await _connection.InsertAsync(passenger); // Inserts new passenger
        }

        public async Task UpdateDriver(Driver driver)
        {
            await _connection.UpdateAsync(driver); // Updates existing driver
        }

        public async Task UpdatePassenger(Passenger passenger)
        {
            await _connection.UpdateAsync(passenger); // Updates existing passenger
        }

        public async Task DeleteDriver(Driver driver)
        {
            await _connection.DeleteAsync(driver); // Deletes driver
        }
        
        public async Task DeletePassenger(Passenger passenger)
        {
            await _connection.DeleteAsync(passenger); // Deletes passenger
        }

        #endregion
    }
}
