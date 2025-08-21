using TCC.Services;
using TCC.Models;
using Microsoft.Maui.Storage;

namespace TCC.Views;

public partial class Index : ContentPage
{
    private readonly DatabaseService _databaseService;
    private string _currentUserType;
    private int _currentUserId;

    public Index()
    {
        InitializeComponent();
        _databaseService = new DatabaseService();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        string? userType = await SecureStorage.GetAsync("user_type");
        string? userIdString = await SecureStorage.GetAsync("user_id");

        if (!int.TryParse(userIdString, out int userId))
        {
            await DisplayAlert("Erro", "Usu?rio inv?lido.", "OK");
            return;
        }

        _currentUserType = userType;
        _currentUserId = userId;

        await LoadUserData();
    }

    private async Task LoadUserData()
    {
        try
        {
            if (_currentUserType == "passenger")
            {
                await LoadPassengerData();
            }
            else if (_currentUserType == "driver")
            {
                await LoadDriverData();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao carregar dados do usu?rio: {ex.Message}", "OK");
        }
    }

    private async Task LoadPassengerData()
    {
        var passengers = await _databaseService.GetPassengers();
        var passenger = passengers.FirstOrDefault(p => p.Id == _currentUserId);

        if (passenger != null)
        {
            // Informa??es b?sicas
            WelcomeLabel.Text = passenger.Name.ToUpper();
            RGLabel.Text = passenger.RG ?? "N?o informado";
            CPFLabel.Text = passenger.CPF ?? "N?o informado";
            EmailLabel.Text = passenger.Email ?? "N?o informado";
            AddressLabel.Text = passenger.Address ?? "N?o informado";
            PhoneLabel.Text = passenger.PhoneNumber ?? "N?o informado";
            EmergencyContactLabel.Text = passenger.EmergencyPhoneNumber ?? "N?o informado";
            GenderLabel.Text = passenger.Genre ?? "N?o informado";

            // Campos espec?ficos do passageiro
            SpecificField1Label.Text = "Escola";
            SpecificField1Value.Text = passenger.School ?? "N?o informado";

            SpecificField2Label.Text = "Respons?vel";
            SpecificField2Value.Text = passenger.ResponsableName ?? "N?o informado";

            // Mostrar campo de atendimento especial
            SpecialTreatmentLayout.IsVisible = true;
            SpecialTreatmentLabel.Text = passenger.SpecialTreatment ? "Sim" : "N?o";
        }
    }

    private async Task LoadDriverData()
    {
        var drivers = await _databaseService.GetDrivers();
        var driver = drivers.FirstOrDefault(d => d.Id == _currentUserId);

        if (driver != null)
        {
            // Informa??es b?sicas
            WelcomeLabel.Text = driver.Name.ToUpper();
            RGLabel.Text = driver.RG ?? "N?o informado";
            CPFLabel.Text = driver.CPF ?? "N?o informado";
            EmailLabel.Text = driver.Email ?? "N?o informado";
            AddressLabel.Text = driver.Address ?? "N?o informado";
            PhoneLabel.Text = driver.PhoneNumber ?? "N?o informado";
            EmergencyContactLabel.Text = driver.EmergencyPhoneNumber ?? "N?o informado";
            GenderLabel.Text = driver.Genre ?? "N?o informado";

            // Campos espec?ficos do motorista
            SpecificField1Label.Text = "CNH";
            SpecificField1Value.Text = driver.CNH ?? "N?o informado";

            SpecificField2Label.Text = "Categoria";
            SpecificField2Value.Text = "Profissional"; // Valor padr?o ou pode ser adicionado ao modelo

            // Ocultar campo de atendimento especial para motoristas
            SpecialTreatmentLayout.IsVisible = false;
        }
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Sair", "Deseja realmente sair da sua conta?", "Sim", "N?o");

        if (confirm)
        {
            // Remove dados da sess?o
            SecureStorage.Remove("user_id");
            SecureStorage.Remove("user_type");

            // Redireciona para tela inicial (limpando a pilha de navega??o)
            Application.Current.MainPage = new NavigationPage(new Home());
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Editar", "Funcionalidade de edi??o em desenvolvimento", "OK");

        // Aqui voc? pode implementar a navega??o para uma p?gina de edi??o
        // if (_currentUserType == "passenger")
        // {
        //     await Navigation.PushAsync(new PassengerEditPage(_currentUserId));
        // }
        // else if (_currentUserType == "driver")
        // {
        //     await Navigation.PushAsync(new DriverEditPage(_currentUserId));
        // }
    }
}