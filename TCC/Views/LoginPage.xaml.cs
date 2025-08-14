using Microsoft.Maui.Controls;
using TCC.Models;
using TCC.Services;

namespace TCC.Views;

public partial class LoginPage : ContentPage
{
    private readonly DatabaseService _databaseService;

    public LoginPage()
    {
        InitializeComponent();
        _databaseService = new DatabaseService();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        try
        {
            string email = EmailEntry.Text?.Trim();
            string password = PasswordEntry.Text?.Trim();
            string userType = UserTypePicker.SelectedItem?.ToString();

            // Validações básicas
            if (string.IsNullOrEmpty(email))
            {
                await DisplayAlert("Erro", "Por favor, digite seu email.", "OK");
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                await DisplayAlert("Erro", "Por favor, digite sua senha.", "OK");
                return;
            }

            if (string.IsNullOrEmpty(userType))
            {
                await DisplayAlert("Erro", "Por favor, selecione o tipo de usuário.", "OK");
                return;
            }

            // Verificar credenciais baseado no tipo de usuário
            if (userType == "Motorista")
            {
                var driver = await _databaseService.GetDriverByEmail(email);
                if (driver != null && driver.Password == password)
                {
                    // Login bem-sucedido para motorista
                    await Navigation.PushAsync(new Index(driver));
                    await DisplayAlert("Sucesso", $"Bem-vindo, {driver.Name}!", "OK");
                }
                else
                {
                    await DisplayAlert("Erro", "Email ou senha incorretos para motorista.", "OK");
                }
            }
            else if (userType == "Passageiro")
            {
                var passenger = await _databaseService.GetPassengerByEmail(email);
                if (passenger != null && passenger.Password == password)
                {
                    // Login bem-sucedido para passageiro
                    await Navigation.PushAsync(new Index(passenger));
                    await DisplayAlert("Sucesso", $"Bem-vindo, {passenger.Name}!", "OK");
                }
                else
                {
                    await DisplayAlert("Erro", "Email ou senha incorretos para passageiro.", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro durante o login: {ex.Message}", "OK");
        }
    }

    // Método para efeito visual do botão
    private void OnPointerEntered(object sender, PointerEventArgs e)
    {
        if (sender is Button button)
        {
            button.BackgroundColor = Color.FromArgb("#CC0000");
        }
    }

    private void OnPointerExited(object sender, PointerEventArgs e)
    {
        if (sender is Button button)
        {
            button.BackgroundColor = Color.FromArgb("#FF0000");
        }
    }
}