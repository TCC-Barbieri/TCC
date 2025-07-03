using Microsoft.Maui.Storage;
using TCC.Models;
using TCC.Services;

namespace TCC.Views;

public partial class LoginPage : ContentPage
{
    private readonly DatabaseService _databaseService;

    public LoginPage()
    {
        InitializeComponent();
        _databaseService = new DatabaseService(); // Instancia o serviço de banco
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string? email = EmailEntry.Text.Trim();
        string password = PasswordEntry.Text;
        string? userType = UserTypePicker.SelectedItem as string;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(userType))
        {
            await DisplayAlert("Erro", "Preencha todos os campos.", "OK");
        }

        if (userType == "Passageiro")
        {
            var passenger = await _databaseService.GetPassengerByEmail(email);

            if (passenger != null && passenger.Password == password)
            {
                await SecureStorage.SetAsync("user_type", "passenger");
                await SecureStorage.SetAsync("user_id", passenger.Id.ToString());

                await DisplayAlert("Sucesso", "Login realizado com sucesso!", "OK");
            }
        }
        else if (userType == "Motorista")
        {
            var driver = await _databaseService.GetDriverByEmail(email);

            if (driver != null && driver.Password == password)
            {
                await SecureStorage.SetAsync("user_type", "driver");
                await SecureStorage.SetAsync("user_id", driver.Id.ToString());

                await DisplayAlert("Sucesso", "Login realizado com sucesso!", "OK");
            }
        }

        await DisplayAlert("Erro", "Usuário ou senha inválidos.", "OK");
    }
}
