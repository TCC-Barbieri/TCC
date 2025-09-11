using TCC.Models;
using TCC.Services;
using System;
using TCC.Helpers;

namespace TCC.Views;

public partial class DriverRegisterPage : ContentPage
{
    private readonly DatabaseService _databaseService = new();
    public DriverRegisterPage()
    {
        InitializeComponent();
    }

    private async void OnRegister_Clicked(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("Register button clicked");

        try
        {
            // IMPORTANTE: Validar ANTES de qualquer coisa
            if (!await ValidateAllFieldsAsync())
            {
                System.Diagnostics.Debug.WriteLine("Validation failed - aborting registration");
                return;
            }

            System.Diagnostics.Debug.WriteLine("All validations passed - proceeding with registration");

            // Criar o objeto Driver apenas ap�s valida��o
            var driver = new Driver
            {
                Name = NameEntry.Text?.Trim(),
                CPF = CPFValidator.RemoveFormat(CPFEntry.Text?.Trim()),
                RG = RGEntry.Text?.Trim(),
                Email = EmailEntry.Text?.Trim(),
                PhoneNumber = PhoneEntry.Text?.Trim(),
                EmergencyPhoneNumber = ContatoEmergenciaEntry.Text?.Trim(),
                CNH = CNHEntry.Text?.Trim(),
                Genre = GenderPicker.SelectedItem?.ToString(),
                Address = AddressEntry.Text?.Trim(),
                BirthDate = BirthDatePicker.Date,
                Password = PasswordEntry.Text
            };

            // Salvar no banco de dados aqui
            await _databaseService.CreateDriver(driver);

            await DisplayAlert("Sucesso", "Conta criada com sucesso!", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in registration: {ex.Message}");
            await DisplayAlert("Erro", $"Erro ao criar conta: {ex.Message}", "OK");
        }
    }

    private async Task<bool> ValidateAllFieldsAsync()
    {
        System.Diagnostics.Debug.WriteLine("Starting field validation");

        // 1. Validar campos obrigat�rios primeiro
        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            await DisplayAlert("Aten��o", "Nome � obrigat�rio", "OK");
            NameEntry.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(CPFEntry.Text))
        {
            await DisplayAlert("Aten��o", "CPF � obrigat�rio", "OK");
            CPFEntry.Focus();
            return false;
        }

        // 2. Validar CPF usando DIRETAMENTE o CPFValidator
        string cpfText = CPFEntry.Text?.Trim();
        System.Diagnostics.Debug.WriteLine($"Validating CPF directly: '{cpfText}'");

        if (!CPFValidator.IsValid(cpfText))
        {
            System.Diagnostics.Debug.WriteLine("CPF validation FAILED");
            await DisplayAlert("Aten��o", "CPF inv�lido. Verifique os n�meros digitados.", "OK");
            CPFEntry.Focus();

            // Mostrar erro visual tamb�m
            if (CPFValidation != null)
            {
                CPFValidation.ValidateCPF();
            }

            return false;
        }

        System.Diagnostics.Debug.WriteLine("CPF validation PASSED");

        // 3. Validar outros campos
        if (string.IsNullOrWhiteSpace(EmailEntry.Text))
        {
            await DisplayAlert("Aten��o", "Email � obrigat�rio", "OK");
            EmailEntry.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            await DisplayAlert("Aten��o", "Senha � obrigat�ria", "OK");
            PasswordEntry.Focus();
            return false;
        }

        if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
        {
            await DisplayAlert("Aten��o", "Senhas n�o coincidem", "OK");
            ConfirmPasswordEntry.Focus();
            return false;
        }

        if (PasswordEntry.Text.Length < 6)
        {
            await DisplayAlert("Aten��o", "Senha deve ter no m�nimo 6 caracteres", "OK");
            PasswordEntry.Focus();
            return false;
        }

        System.Diagnostics.Debug.WriteLine("All validations PASSED");
        return true;
    }

    // M�todo para testar CPF manualmente
    private async void TestCPF_Clicked(object sender, EventArgs e)
    {
        string cpf = CPFEntry.Text;
        bool isValid = CPFValidator.IsValid(cpf);
        await DisplayAlert("Teste CPF", $"CPF: {cpf}\nV�lido: {isValid}", "OK");
    }


    private void OnAlreadyHaveAccount_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new Views.LoginPage());
    }
    private void OnPointerEntered(object sender, PointerEventArgs e)
    {
        // A��o quando o mouse entra no bot�o
        ((Button)sender).BackgroundColor = Colors.DarkRed; // Muda a cor do bot�o
    }

    private void OnPointerExited(object sender, PointerEventArgs e)
    {
        // A��o quando o mouse sai do bot�o
        ((Button)sender).BackgroundColor = Colors.Red; // Volta � cor original
    }

    private void OnPointer2Entered(object sender, PointerEventArgs e)
    {
        // A��o quando o mouse entra no bot�o
        ((Button)sender).TextColor = Colors.DarkRed; // Muda a cor do bot�o
    }

    private void OnPointer2Exited(object sender, PointerEventArgs e)
    {
        // A��o quando o mouse sai do bot�o
        ((Button)sender).TextColor = Colors.Red; // Volta � cor original
    }
}