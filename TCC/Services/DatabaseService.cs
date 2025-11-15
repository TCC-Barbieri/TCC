using SQLite;
using TCC.Models;

namespace TCC.Services
{
    public class DatabaseService
    {
        private const string DB_NAME = "database.db3";
        private readonly SQLiteAsyncConnection _connection;
        private bool _isInitialized = false;

        public DatabaseService()
        {
            _connection = new SQLiteAsyncConnection(Path.Combine(FileSystem.AppDataDirectory, DB_NAME));
        }

        /// <summary>
        /// Inicializa as tabelas do banco de dados. Deve ser chamado antes de usar o serviço.
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_isInitialized)
                return;

            try
            {
                await _connection.CreateTableAsync<Passenger>();
                await _connection.CreateTableAsync<Driver>();
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao inicializar banco de dados: {ex.Message}", ex);
            }
        }

        #region Functions that get's all drivers and passengers

        public async Task<List<Driver>> GetDrivers()
        {
            await InitializeAsync();
            return await _connection.Table<Driver>().ToListAsync();
        }

        public async Task<List<Passenger>> GetPassengers()
        {
            await InitializeAsync();
            return await _connection.Table<Passenger>().ToListAsync();
        }

        #endregion

        #region Functions responsible for registering/editing/deleting users

        public async Task<int> CreatePassenger(Passenger passenger)
        {
            await InitializeAsync();
            return await _connection.InsertAsync(passenger);
        }

        public async Task<int> CreateDriver(Driver driver)
        {
            await InitializeAsync();
            return await _connection.InsertAsync(driver);
        }

        public async Task<int> UpdateDriver(Driver driver)
        {
            await InitializeAsync();
            return await _connection.UpdateAsync(driver);
        }

        public async Task<int> UpdatePassenger(Passenger passenger)
        {
            await InitializeAsync();
            return await _connection.UpdateAsync(passenger);
        }

        public async Task<int> DeleteDriver(Driver driver)
        {
            await InitializeAsync();
            return await _connection.DeleteAsync(driver);
        }

        public async Task<int> DeletePassenger(Passenger passenger)
        {
            await InitializeAsync();
            return await _connection.DeleteAsync(passenger);
        }

        #endregion

        #region Login and validation methods

        public async Task<Driver> GetDriverByEmail(string email)
        {
            await InitializeAsync();
            return await _connection.Table<Driver>()
                .Where(d => d.Email == email)
                .FirstOrDefaultAsync();
        }

        public async Task<Passenger> GetPassengerByEmail(string email)
        {
            await InitializeAsync();
            return await _connection.Table<Passenger>()
                .Where(p => p.Email == email)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> IsEmailTaken(string email)
        {
            await InitializeAsync();
            var driver = await GetDriverByEmail(email);
            var passenger = await GetPassengerByEmail(email);
            return driver != null || passenger != null;
        }

        public async Task<bool> IsDriverFieldUnique(string fieldName, string value)
        {
            await InitializeAsync();

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
            await InitializeAsync();

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

        /// <summary>
        /// Valida se os dados do usuário são únicos no sistema
        /// </summary>
        public async Task<(bool IsValid, string Message)> ValidateUniqueUserData(
            string rg = null,
            string cpf = null,
            string email = null,
            string phone = null,
            string cnh = null,
            int? excludeUserId = null,
            string userType = null)
        {
            try
            {
                await InitializeAsync();

                var drivers = await GetDrivers();
                var passengers = await GetPassengers();

                // Remove o usuário atual se estiver editando
                if (excludeUserId.HasValue && !string.IsNullOrEmpty(userType))
                {
                    if (userType == "driver")
                        drivers = drivers.Where(d => d.Id != excludeUserId.Value).ToList();
                    else if (userType == "passenger")
                        passengers = passengers.Where(p => p.Id != excludeUserId.Value).ToList();
                }

                // Valida RG
                if (!string.IsNullOrEmpty(rg) && (drivers.Any(d => d.RG == rg) || passengers.Any(p => p.RG == rg)))
                    return (false, "Este RG já está cadastrado no sistema.");

                // Valida CPF
                if (!string.IsNullOrEmpty(cpf) && (drivers.Any(d => d.CPF == cpf) || passengers.Any(p => p.CPF == cpf)))
                    return (false, "Este CPF já está cadastrado no sistema.");

                // Valida Email
                if (!string.IsNullOrEmpty(email) && (drivers.Any(d => d.Email == email) || passengers.Any(p => p.Email == email)))
                    return (false, "Este e-mail já está cadastrado no sistema.");

                // Valida Telefone
                if (!string.IsNullOrEmpty(phone) && (drivers.Any(d => d.PhoneNumber == phone) || passengers.Any(p => p.PhoneNumber == phone)))
                    return (false, "Este número de telefone já está cadastrado no sistema.");

                // Valida CNH (apenas para motoristas)
                if (!string.IsNullOrEmpty(cnh) && drivers.Any(d => d.CNH == cnh))
                    return (false, "Esta CNH já está cadastrada no sistema.");

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao validar dados: {ex.Message}");
            }
        }

        /// <summary>
        /// Verifica se um email já está em uso
        /// </summary>
        public async Task<bool> IsEmailTaken(string email, int? excludeUserId = null, string userType = null)
        {
            var result = await ValidateUniqueUserData(
                email: email,
                excludeUserId: excludeUserId,
                userType: userType);
            return !result.IsValid;
        }

        #region Methods to update location in real time

        public async Task UpdatePassengerLocationAsync(int passengerId, double latitude, double longitude)
        {
            await InitializeAsync();

            var passenger = await _connection.Table<Passenger>()
                .Where(p => p.Id == passengerId)
                .FirstOrDefaultAsync();

            if (passenger != null)
            {
                passenger.Latitude = latitude;
                passenger.Longitude = longitude;
                await _connection.UpdateAsync(passenger);
            }
        }

        public async Task UpdateDriverLocationAsync(int driverId, double latitude, double longitude)
        {
            await InitializeAsync();

            var driver = await _connection.Table<Driver>()
                .Where(d => d.Id == driverId)
                .FirstOrDefaultAsync();

            if (driver != null)
            {
                driver.Latitude = latitude;
                driver.Longitude = longitude;
                await _connection.UpdateAsync(driver);
            }
        }

        #endregion

        /// <summary>
        /// Fecha a conexão com o banco de dados
        /// </summary>
        public async Task CloseConnection()
        {
            if (_connection != null)
            {
                await _connection.CloseAsync();
                _isInitialized = false;
            }
        }
    }
}