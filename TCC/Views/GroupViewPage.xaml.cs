using TCC.Models;
using TCC.Services;

namespace TCC.Views
{
    public partial class GroupViewPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        private Driver _loggedDriver;

        public List<Passenger> EtecUsers { get; set; } = new();
        public List<Passenger> FatecUsers { get; set; } = new();
        public List<Passenger> UnespUsers { get; set; } = new();
        public List<Driver> Drivers { get; set; } = new();

        public GroupViewPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadLoggedDriverAsync();
            await LoadGroupsAsync();
        }

        private async Task LoadLoggedDriverAsync()
        {
            try
            {
                string userType = await SecureStorage.GetAsync("user_type");
                string userIdString = await SecureStorage.GetAsync("user_id");

                if (userType == "driver" && int.TryParse(userIdString, out int driverId))
                {
                    var drivers = await _databaseService.GetDrivers();
                    _loggedDriver = drivers.FirstOrDefault(d => d.Id == driverId);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao carregar motorista logado: {ex.Message}", "OK");
            }
        }

        private async Task LoadGroupsAsync()
        {
            try
            {
                var loggedUser = await _databaseService.GetLoggedUser();
                bool isPassenger = loggedUser is Passenger;
                string loggedSchool = isPassenger ? ((Passenger)loggedUser).School.Trim().ToUpper() : "";

                var allPassengers = await _databaseService.GetPassengers();
                var allDrivers = await _databaseService.GetDrivers();

                if (isPassenger)
                {
                    allPassengers = allPassengers
                        .Where(p => p.School.Trim().ToUpper() == loggedSchool)
                        .ToList();
                }

                EtecUsers = allPassengers.Where(p => p.School.ToUpper() == "ETEC").ToList();
                FatecUsers = allPassengers.Where(p => p.School.ToUpper() == "FATEC").ToList();
                UnespUsers = allPassengers.Where(p => p.School.ToUpper() == "UNESP").ToList();

                Drivers = isPassenger ? new List<Driver>() : allDrivers;

                EtecCountLabel.Text = $"{EtecUsers.Count} usuários";
                FatecCountLabel.Text = $"{FatecUsers.Count} usuários";
                UnespCountLabel.Text = $"{UnespUsers.Count} usuários";
                DriversCountLabel.Text = $"{Drivers.Count} motoristas";

                EtecCollectionView.ItemsSource = EtecUsers;
                FatecCollectionView.ItemsSource = FatecUsers;
                UnespCollectionView.ItemsSource = UnespUsers;
                DriversCollectionView.ItemsSource = Drivers;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.Message, "OK");
            }
        }

        private async void BackButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnUserSelected(object sender, SelectionChangedEventArgs e)
        {
            var selected = e.CurrentSelection.FirstOrDefault();
            if (selected == null)
                return;

            // Limpa a seleção imediatamente
            ((CollectionView)sender).SelectedItem = null;

            // Se não for motorista logado, não faz nada
            if (_loggedDriver == null)
                return;

            // Apenas passageiros podem ser visualizados
            if (selected is Passenger passenger)
            {
                await Navigation.PushAsync(
                    new UserProfilePage(_loggedDriver, passenger)
                );
            }
        }
    }
}