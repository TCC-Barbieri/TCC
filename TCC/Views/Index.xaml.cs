using TCC.Services;
using TCC.Models;
using Microsoft.Maui.Storage;

namespace TCC.Views;

public partial class Index : ContentPage
{
    private readonly DatabaseService _databaseService;

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

        string userName = string.Empty;

        if (userType == "passenger")
        {
            Passenger? passenger = await _databaseService.GetPassengers()
                .ContinueWith(task => task.Result.FirstOrDefault(p => p.Id == userId));

            if (passenger != null)
                userName = passenger.Name;
        }
        else if (userType == "driver")
        {
            Driver? driver = await _databaseService.GetDrivers()
                .ContinueWith(task => task.Result.FirstOrDefault(d => d.Id == userId));

            if (driver != null)
                userName = driver.Name;
        }

        if (!string.IsNullOrEmpty(userName))
        {
            WelcomeLabel.Text = $"Bem-vindo, {userName.Trim()}!";
        }
    }


    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        // Remove dados da sessão
        SecureStorage.Remove("user_id");
        SecureStorage.Remove("user_type");

        // Redireciona para tela de login (limpando a pilha de navegação)
        Application.Current.MainPage = new NavigationPage(new Home());
    }

}
