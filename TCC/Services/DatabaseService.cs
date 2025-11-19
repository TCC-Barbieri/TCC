using SQLite;
using TCC.Models;
using Microsoft.Maui.Storage; // <- Necessário para Preferences

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

        #region Unique data validation

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

                if (string.IsNullOrEmpty(userType))
                    return (false, "Tipo de usuário não informado na validação.");

                // Carrega SOMENTE a tabela correta
                if (userType == "driver")
                {
                    var drivers = await GetDrivers();

                    // Exclui o próprio usuário em caso de edição
                    if (excludeUserId.HasValue)
                        drivers = drivers.Where(d => d.Id != excludeUserId.Value).ToList();

                    if (!string.IsNullOrEmpty(rg) && drivers.Any(d => d.RG == rg))
                        return (false, "Este RG já está cadastrado para outro motorista.");

                    if (!string.IsNullOrEmpty(cpf) && drivers.Any(d => d.CPF == cpf))
                        return (false, "Este CPF já está cadastrado para outro motorista.");

                    if (!string.IsNullOrEmpty(email) && drivers.Any(d => d.Email == email))
                        return (false, "Este e-mail já está cadastrado para outro motorista.");

                    if (!string.IsNullOrEmpty(phone) && drivers.Any(d => d.PhoneNumber == phone))
                        return (false, "Este telefone já está cadastrado para outro motorista.");

                    if (!string.IsNullOrEmpty(cnh) && drivers.Any(d => d.CNH == cnh))
                        return (false, "Esta CNH já está cadastrada para outro motorista.");

                    return (true, string.Empty);
                }
                else if (userType == "passenger")
                {
                    var passengers = await GetPassengers();

                    // Exclui o próprio usuário em caso de edição
                    if (excludeUserId.HasValue)
                        passengers = passengers.Where(p => p.Id != excludeUserId.Value).ToList();

                    if (!string.IsNullOrEmpty(rg) && passengers.Any(p => p.RG == rg))
                        return (false, "Este RG já está cadastrado para outro passageiro.");

                    if (!string.IsNullOrEmpty(cpf) && passengers.Any(p => p.CPF == cpf))
                        return (false, "Este CPF já está cadastrado para outro passageiro.");

                    if (!string.IsNullOrEmpty(email) && passengers.Any(p => p.Email == email))
                        return (false, "Este e-mail já está cadastrado para outro passageiro.");

                    if (!string.IsNullOrEmpty(phone) && passengers.Any(p => p.PhoneNumber == phone))
                        return (false, "Este telefone já está cadastrado para outro passageiro.");

                    return (true, string.Empty);
                }

                return (false, "Tipo de usuário inválido.");
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao validar dados: {ex.Message}");
            }
        }


        #endregion

        #region Real time location update

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


        // =====================================================================
        // 🔵 ADIÇÃO DOS MÉTODOS NOVOS
        // =====================================================================

        #region Logged user methods

        public async Task<object?> GetLoggedUser()
        {
            await InitializeAsync();

            var email = Preferences.Get("LoggedUserEmail", null as string);
            var userType = Preferences.Get("LoggedUserType", null as string);

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(userType))
                return null;

            if (userType.Equals("Driver", StringComparison.OrdinalIgnoreCase))
                return await GetDriverByEmail(email);

            if (userType.Equals("Passenger", StringComparison.OrdinalIgnoreCase))
                return await GetPassengerByEmail(email);

            return null;
        }

        #endregion

        #region Group list methods

        public class GroupDto
        {
            public string Name { get; set; } = string.Empty;
            public string School { get; set; } = string.Empty;
            public int Count { get; set; }
        }

        public async Task<List<GroupDto>> GetGroupsAsync()
        {
            await InitializeAsync();

            var passengers = await GetPassengers();

            var groups = passengers
                .Where(p => !string.IsNullOrWhiteSpace(p.School))
                .GroupBy(p => p.School.Trim())
                .Select(g => new GroupDto
                {
                    Name = g.Key,
                    School = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(g => g.Count)
                .ToList();

            var withoutSchoolCount = passengers.Count(p => string.IsNullOrWhiteSpace(p.School));
            if (withoutSchoolCount > 0)
            {
                groups.Add(new GroupDto
                {
                    Name = "Sem Escola",
                    School = "Sem Escola",
                    Count = withoutSchoolCount
                });
            }

            return groups;
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
