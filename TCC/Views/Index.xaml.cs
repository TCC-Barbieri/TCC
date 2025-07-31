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
            await DisplayAlert("Erro", "Usuário inválido.", "OK");
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
            await DisplayAlert("Erro", $"Erro ao carregar dados do usuário: {ex.Message}", "OK");
        }
    }

    private async Task LoadPassengerData()
    {
        var passengers = await _databaseService.GetPassengers();
        var passenger = passengers.FirstOrDefault(p => p.Id == _currentUserId);

        if (passenger != null)
        {
            // Informações básicas
            WelcomeLabel.Text = passenger.Name.ToUpper();
            RGLabel.Text = passenger.RG ?? "Não informado";
            CPFLabel.Text = passenger.CPF ?? "Não informado";
            EmailLabel.Text = passenger.Email ?? "Não informado";
            AddressLabel.Text = passenger.Address ?? "Não informado";
            PhoneLabel.Text = passenger.PhoneNumber ?? "Não informado";
            EmergencyContactLabel.Text = passenger.EmergencyPhoneNumber ?? "Não informado";
            GenderLabel.Text = passenger.Genre ?? "Não informado";

            // Campos específicos do passageiro
            SpecificField1Label.Text = "Escola";
            SpecificField1Value.Text = passenger.School ?? "Não informado";

            SpecificField2Label.Text = "Responsável";
            SpecificField2Value.Text = passenger.ResponsableName ?? "Não informado";

            // Mostrar campo de atendimento especial
            SpecialTreatmentLayout.IsVisible = true;
            SpecialTreatmentLabel.Text = passenger.SpecialTreatment ? "Sim" : "Não";
        }
    }

    private async Task LoadDriverData()
    {
        var drivers = await _databaseService.GetDrivers();
        var driver = drivers.FirstOrDefault(d => d.Id == _currentUserId);

        if (driver != null)
        {
            // Informações básicas
            WelcomeLabel.Text = driver.Name.ToUpper();
            RGLabel.Text = driver.RG ?? "Não informado";
            CPFLabel.Text = driver.CPF ?? "Não informado";
            EmailLabel.Text = driver.Email ?? "Não informado";
            AddressLabel.Text = driver.Address ?? "Não informado";
            PhoneLabel.Text = driver.PhoneNumber ?? "Não informado";
            EmergencyContactLabel.Text = driver.EmergencyPhoneNumber ?? "Não informado";
            GenderLabel.Text = driver.Genre ?? "Não informado";

            // Campos específicos do motorista
            SpecificField1Label.Text = "CNH";
            SpecificField1Value.Text = driver.CNH ?? "Não informado";

            SpecificField2Label.Text = "Categoria";
            SpecificField2Value.Text = "Profissional"; // Valor padrão ou pode ser adicionado ao modelo

            // Ocultar campo de atendimento especial para motoristas
            SpecialTreatmentLayout.IsVisible = false;
        }
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Sair", "Deseja realmente sair da sua conta?", "Sim", "Não");

        if (confirm)
        {
            // Remove dados da sessão
            SecureStorage.Remove("user_id");
            SecureStorage.Remove("user_type");

            // Redireciona para tela inicial (limpando a pilha de navegação)
            Application.Current.MainPage = new NavigationPage(new Home());
        }
    }

    private async void OnGroupViewClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Sair", "Deseja realmente sair da sua conta?", "Sim", "Não");

        if (confirm)
        {
            // Remove dados da sessão
            SecureStorage.Remove("user_id");
            SecureStorage.Remove("user_type");

            // Redireciona para tela inicial (limpando a pilha de navegação)
            Application.Current.MainPage = new NavigationPage(new Home());
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        if (_currentUserType == "passenger")
        {
            await Navigation.PushAsync(new PassengerEditPage(_currentUserId));
        }
        else if (_currentUserType == "driver")
        {
            await Navigation.PushAsync(new DriverEditPage(_currentUserId));
        }
    }
}