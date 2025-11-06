using Microsoft.Maui.Devices.Sensors;
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
        string? email = EmailEntry.Text?.Trim();
        string password = PasswordEntry.Text;
        string? userType = UserTypePicker.SelectedItem as string;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(userType))
        {
            await DisplayAlert("Erro", "Preencha todos os campos.", "OK");
            return;
        }

        try
        {
            if (userType == "Passageiro")
            {
                Passenger passenger = await _databaseService.GetPassengerByEmail(email);

                if (passenger != null && passenger.Password == password)
                {
                    await SecureStorage.SetAsync("user_type", "passenger");
                    await SecureStorage.SetAsync("user_id", passenger.Id.ToString());

                    var passengerId = await SecureStorage.GetAsync("user_id");

                    await DisplayAlert("Sucesso", "Login realizado com sucesso!", "OK");

                    await Navigation.PushAsync(new ChoosePagePassenger(int.Parse(passengerId)));
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

                    var driverId = await SecureStorage.GetAsync("user_id"); 

                    await DisplayAlert("Sucesso", "Login realizado com sucesso!", "OK");

                    await Navigation.PushAsync(new ChoosePageDriver(int.Parse(driverId)));
                    return;
                }
            }

            await DisplayAlert("Erro", "Usuário ou senha inválidos.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Falha no login: {ex.Message}", "OK");
        }
    }

    #region Pointer hover events

    private void OnPointerEntered(object sender, PointerEventArgs e)
    {
        ((Button)sender).BackgroundColor = Colors.DarkRed;
    }

    private void OnPointerExited(object sender, PointerEventArgs e)
    {
        ((Button)sender).BackgroundColor = Colors.Red;
    }

    private void OnPointer2Entered(object sender, PointerEventArgs e)
    {
        ((Button)sender).BackgroundColor = Colors.DarkRed;
    }

    private void OnPointer2Exited(object sender, PointerEventArgs e)
    {
        ((Button)sender).BackgroundColor = Colors.Red;
    }

    #endregion
}
