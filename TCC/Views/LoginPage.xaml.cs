
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

                await Navigation.PushAsync(new ChoosePagePassenger()); // Redirecting to the main page after successful login

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

                await Navigation.PushAsync(new ChoosePageDriver()); // Redirecting to the main page after successful login

                await DisplayAlert("Sucesso", "Login realizado com sucesso!", "OK");

                return;
            }
        }

        await DisplayAlert("Erro", "Usuário ou senha inválidos.", "OK");
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

    private void OnPointer2Entered(object sender, PointerEventArgs e)
    {
        // Ação quando o mouse entra no botão
        ((Button)sender).BackgroundColor = Colors.DarkRed; // Muda a cor do botão
    }

    private void OnPointer2Exited(object sender, PointerEventArgs e)
    {
        // Ação quando o mouse sai do botão
        ((Button)sender).BackgroundColor = Colors.Red; // Volta à cor original
    }
}