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

            // Criar o objeto Driver apenas após validação
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

        // 1. Validar campos obrigatórios primeiro
        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            await DisplayAlert("Atenção", "Nome é obrigatório", "OK");
            NameEntry.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(CPFEntry.Text))
        {
            await DisplayAlert("Atenção", "CPF é obrigatório", "OK");
            CPFEntry.Focus();
            return false;
        }

        // 2. Validar CPF usando DIRETAMENTE o CPFValidator
        string cpfText = CPFEntry.Text?.Trim();
        System.Diagnostics.Debug.WriteLine($"Validating CPF directly: '{cpfText}'");

        if (!CPFValidator.IsValid(cpfText))
        {
            System.Diagnostics.Debug.WriteLine("CPF validation FAILED");
            await DisplayAlert("Atenção", "CPF inválido. Verifique os números digitados.", "OK");
            CPFEntry.Focus();

            // Mostrar erro visual também
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
            await DisplayAlert("Atenção", "Email é obrigatório", "OK");
            EmailEntry.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            await DisplayAlert("Atenção", "Senha é obrigatória", "OK");
            PasswordEntry.Focus();
            return false;
        }

        if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
        {
            await DisplayAlert("Atenção", "Senhas não coincidem", "OK");
            ConfirmPasswordEntry.Focus();
            return false;
        }

        if (PasswordEntry.Text.Length < 6)
        {
            await DisplayAlert("Atenção", "Senha deve ter no mínimo 6 caracteres", "OK");
            PasswordEntry.Focus();
            return false;
        }

        System.Diagnostics.Debug.WriteLine("All validations PASSED");
        return true;
    }

    // Método para testar CPF manualmente
    private async void TestCPF_Clicked(object sender, EventArgs e)
    {
        string cpf = CPFEntry.Text;
        bool isValid = CPFValidator.IsValid(cpf);
        await DisplayAlert("Teste CPF", $"CPF: {cpf}\nVálido: {isValid}", "OK");
    }


    private void OnAlreadyHaveAccount_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new Views.LoginPage());
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
        ((Button)sender).TextColor = Colors.DarkRed; // Muda a cor do botão
    }

    private void OnPointer2Exited(object sender, PointerEventArgs e)
    {
        // Ação quando o mouse sai do botão
        ((Button)sender).TextColor = Colors.Red; // Volta à cor original
    }
}