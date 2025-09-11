using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Storage;
using TCC.Models;
using TCC.Services;

namespace TCC.Views;

public partial class LoginPage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private CancellationTokenSource _cts;

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

                    // Inicia rastreamento de localização em tempo real
                    _ = StartTrackingLocationAsync(passenger.Id, "passenger");

                    await DisplayAlert("Sucesso", "Login realizado com sucesso!", "OK");

                    await Navigation.PushAsync(new ChoosePagePassenger());
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

                    // Inicia rastreamento de localização em tempo real
                    _ = StartTrackingLocationAsync(driver.Id, "driver");

                    await DisplayAlert("Sucesso", "Login realizado com sucesso!", "OK");

                    await Navigation.PushAsync(new ChoosePageDriver());
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

    #region Real-time location tracking

    private async Task StartTrackingLocationAsync(int userId, string userType)
    {
        try
        {
            _cts = new CancellationTokenSource();

            while (!_cts.Token.IsCancellationRequested)
            {
                var location = await Geolocation.Default.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Best,
                    Timeout = TimeSpan.FromSeconds(10)
                }, _cts.Token);

                if (location != null)
                {
                    if (userType == "passenger")
                        await _databaseService.UpdatePassengerLocationAsync(userId, location.Latitude, location.Longitude);
                    else if (userType == "driver")
                        await _databaseService.UpdateDriverLocationAsync(userId, location.Latitude, location.Longitude);
                }

                await Task.Delay(5000); // Atualiza a cada 5 segundos
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Falha ao obter localização: {ex.Message}", "OK");
        }
    }

    private void StopTrackingLocation()
    {
        if (_cts != null && !_cts.Token.IsCancellationRequested)
            _cts.Cancel();
    }

    #endregion

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
