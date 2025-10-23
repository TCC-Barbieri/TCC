using System.Collections.ObjectModel;
using TCC.Models;
using TCC.Services;

namespace TCC.Views;

public partial class GroupViewPage : ContentPage
{
    private readonly DatabaseService _databaseService;

    public ObservableCollection<UserDisplayModel> EtecUsers { get; set; } = new();
    public ObservableCollection<UserDisplayModel> FatecUsers { get; set; } = new();
    public ObservableCollection<UserDisplayModel> UnespUsers { get; set; } = new();
    public ObservableCollection<UserDisplayModel> Drivers { get; set; } = new();

    public GroupViewPage()
    {
        InitializeComponent();
        _databaseService = new DatabaseService();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadUserGroups();
    }

    private async Task LoadUserGroups()
    {
        try
        {
            // Limpar as listas
            EtecUsers.Clear();
            FatecUsers.Clear();
            UnespUsers.Clear();
            Drivers.Clear();

            // Carregar passageiros
            var passengers = await _databaseService.GetPassengers();
            foreach (var passenger in passengers)
            {
                var userModel = new UserDisplayModel
                {
                    Id = passenger.Id,
                    Name = passenger.Name,
                    Email = passenger.Email,
                    School = passenger.School,
                    UserType = "Passageiro",
                    TelephoneNumber = string.Empty
                };

                // Classificar por escola
                var school = passenger.School?.ToUpper();
                if (school != null)
                {
                    if (school.Contains("ETEC"))
                        EtecUsers.Add(userModel);
                    else if (school.Contains("FATEC"))
                        FatecUsers.Add(userModel);
                    else 
                        UnespUsers.Add(userModel);
                }
            }

            // Carregar motoristas
            var drivers = await _databaseService.GetDrivers();
            foreach (var driver in drivers)
            {
                var driverModel = new UserDisplayModel
                {
                    Id = driver.Id,
                    Name = driver.Name,
                    Email = driver.Email,
                    School = "N/A",
                    UserType = "Motorista",
                    TelephoneNumber = $"Telephone Number: {driver.PhoneNumber}"
                };

                Drivers.Add(driverModel);
            }

            // Atualizar contadores
            UpdateCountLabels();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao carregar grupos: {ex.Message}", "OK");
        }
    }

    private void UpdateCountLabels()
    {
        EtecCountLabel.Text = $"{EtecUsers.Count} usuário{(EtecUsers.Count != 1 ? "s" : "")}";
        FatecCountLabel.Text = $"{FatecUsers.Count} usuário{(FatecUsers.Count != 1 ? "s" : "")}";
        UnespCountLabel.Text = $"{UnespUsers.Count} usuário{(UnespUsers.Count != 1 ? "s" : "")}";
        DriversCountLabel.Text = $"{Drivers.Count} motorista{(Drivers.Count != 1 ? "s" : "")}";
    }

    private void BackButton_Clicked(object sender, EventArgs e)
    {
        Navigation.PopAsync();
    }
}

public class UserDisplayModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string School { get; set; }
    public string UserType { get; set; }
    public string TelephoneNumber { get; set; }
}

