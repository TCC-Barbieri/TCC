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
            Passenger passenger = await _databaseService.GetPassengerByEmail(email);

            if (passenger != null && passenger.Password == password)
            {
                await SecureStorage.SetAsync("user_type", "passenger");
                await SecureStorage.SetAsync("user_id", passenger.Id.ToString());

                await Navigation.PushAsync(new Index()); // Redirecting to the main page after successful login

                await DisplayAlert("Sucesso", "Login realizado com sucesso!", "OK");

                return;
            }
        }
        else if (userType == "Motorista")
        {
            Driver driver = await _databaseService.GetDriverByEmail(email);

            if (driver != null && driver.Password == password)
            {
                await SecureStorage.SetAsync("user_type", "driver");
                await SecureStorage.SetAsync("user_id", driver.Id.ToString());

                await Navigation.PushAsync(new Index()); // Redirecting to the main page after successful login

                await DisplayAlert("Sucesso", "Login realizado com sucesso!", "OK");

                return;
            }
        }

        await DisplayAlert("Erro", "Usuário ou senha inválidos.", "OK");
    }

    protected override async void OnAppearing() // Verifies if we already have a user logged in (so we can skip the login page)
    {
        base.OnAppearing();

        var userType = await SecureStorage.GetAsync("user_type");
        var userId = await SecureStorage.GetAsync("user_id");

        if (!string.IsNullOrEmpty(userType) && !string.IsNullOrEmpty(userId))
        {
            await Navigation.PushAsync(new Index());
        }
    }

    private void OnPointerEntered(object sender, PointerEventArgs e)
    {
        // Ação quando o mouse entra no botão
        ((Button)sender).BackgroundColor = Colors.DarkRed; // Muda a cor do botão
    }

    private void OnPointerExited(object sender, PointerEventArgs e)
    {
        // Ação quando o mouse sai do botão
        ((Button)sender).BackgroundColor = Colors.Red; // Volta à cor original
    }

}
