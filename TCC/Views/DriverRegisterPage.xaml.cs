using TCC.Models;
using TCC.Services;
using TCC.Helpers;

namespace TCC.Views;

public partial class DriverRegisterPage : ContentPage
{
    private readonly DatabaseService _databaseService = new();

    public DateTime MaxDriverBirthDate => DateTime.Today.AddYears(-18);

    public DriverRegisterPage()
    {
        InitializeComponent();

        // Define a data máxima do DatePicker (18 anos atrás)
        BirthDatePicker.MaximumDate = MaxDriverBirthDate;
        BirthDatePicker.Date = MaxDriverBirthDate;
    }

    private async void OnRegister_Clicked(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("Register button clicked");

        try
        {
            var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(5));
            var location = await Geolocation.Default.GetLocationAsync(request);

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
                RG = RGValidatorHelper.RemoveFormat(RGEntry.Text?.Trim()),
                Email = EmailEntry.Text?.Trim(),
                PhoneNumber = PhoneValidationHelper.GetOnlyNumbers(PhoneEntry.Text?.Trim()),
                EmergencyPhoneNumber = PhoneValidationHelper.GetOnlyNumbers(ContatoEmergenciaEntry.Text?.Trim()),
                CNH = CNHEntry.Text?.Trim(),
                Genre = GenderPicker.SelectedItem?.ToString(),
                Address = AddressEntry.Text?.Trim(),
                BirthDate = BirthDatePicker.Date,
                Password = PasswordEntry.Text,
                Longitude = location.Longitude,
                Latitude = location.Latitude
            };

            // Salvar no banco de dados aqui
            await _databaseService.CreateDriver(driver);

            await DisplayAlert("Sucesso", "Conta criada com sucesso!", "OK");
            await Navigation.PushAsync(new LoginPage());
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

        // 2. Validar RG
        if (string.IsNullOrWhiteSpace(RGEntry.Text))
        {
            await DisplayAlert("Atenção", "RG é obrigatório", "OK");
            RGEntry.Focus();
            return false;
        }

        string rgText = RGEntry.Text?.Trim();
        if (!RGValidatorHelper.IsValid(rgText))
        {
            await DisplayAlert("Atenção", "RG inválido. Verifique os números digitados.", "OK");
            RGEntry.Focus();

            if (RGValidation != null)
            {
                RGValidation.ValidateRG();
            }

            return false;
        }

        // 3. Validar CPF
        if (string.IsNullOrWhiteSpace(CPFEntry.Text))
        {
            await DisplayAlert("Atenção", "CPF é obrigatório", "OK");
            CPFEntry.Focus();
            return false;
        }

        string cpfText = CPFEntry.Text?.Trim();
        System.Diagnostics.Debug.WriteLine($"Validating CPF directly: '{cpfText}'");

        if (!CPFValidator.IsValid(cpfText))
        {
            System.Diagnostics.Debug.WriteLine("CPF validation FAILED");
            await DisplayAlert("Atenção", "CPF inválido. Verifique os números digitados.", "OK");
            CPFEntry.Focus();

            if (CPFValidation != null)
            {
                CPFValidation.ValidateCPF();
            }

            return false;
        }

        System.Diagnostics.Debug.WriteLine("CPF validation PASSED");

        // 4. Validar Email
        if (string.IsNullOrWhiteSpace(EmailEntry.Text))
        {
            await DisplayAlert("Atenção", "Email é obrigatório", "OK");
            EmailEntry.Focus();
            return false;
        }

        // 5. Validar idade (mínimo 18 anos)
        var age = DateTime.Today.Year - BirthDatePicker.Date.Year;
        if (BirthDatePicker.Date > DateTime.Today.AddYears(-age)) age--;

        if (age < 18)
        {
            BirthDateErrorLabel.Text = "Motorista deve ter no mínimo 18 anos";
            BirthDateErrorLabel.IsVisible = true;
            await DisplayAlert("Atenção", "Motorista deve ter no mínimo 18 anos para se cadastrar", "OK");
            return false;
        }
        else
        {
            BirthDateErrorLabel.IsVisible = false;
        }

        // 6. Validar Telefone
        if (string.IsNullOrWhiteSpace(PhoneEntry.Text))
        {
            PhoneErrorLabel.Text = "Telefone é obrigatório";
            PhoneErrorLabel.IsVisible = true;
            await DisplayAlert("Atenção", "Telefone é obrigatório", "OK");
            PhoneEntry.Focus();
            return false;
        }

        if(PhoneValidationHelper.GetOnlyNumbers(PhoneEntry.Text?.Trim()).Length != 11)
        {
            PhoneErrorLabel.Text = "Telefone precisa ter 11 dígitos";
            PhoneErrorLabel.IsVisible = true;
            await DisplayAlert("Atenção", "Telefone precisa ter 11 dígitos", "OK");
            PhoneEntry.Focus();
            return false;
        }

        string phoneText = PhoneEntry.Text?.Trim();
        if (!PhoneValidationHelper.IsValidPhone(phoneText))
        {
            PhoneErrorLabel.Text = PhoneValidationHelper.GetValidationErrorMessage();
            PhoneErrorLabel.IsVisible = true;
            await DisplayAlert("Atenção", PhoneValidationHelper.GetValidationErrorMessage(), "OK");
            PhoneEntry.Focus();
            return false;
        }
        else
        {
            PhoneErrorLabel.IsVisible = false;
        }

        // 7. Validar Telefone de Emergência
        if (string.IsNullOrWhiteSpace(ContatoEmergenciaEntry.Text))
        {
            EmergencyPhoneErrorLabel.Text = "Contato de emergência é obrigatório";
            EmergencyPhoneErrorLabel.IsVisible = true;
            await DisplayAlert("Atenção", "Contato de emergência é obrigatório", "OK");
            ContatoEmergenciaEntry.Focus();
            return false;
        }

        string emergencyPhoneText = ContatoEmergenciaEntry.Text?.Trim();
        if (!PhoneValidationHelper.IsValidPhone(emergencyPhoneText))
        {
            EmergencyPhoneErrorLabel.Text = PhoneValidationHelper.GetValidationErrorMessage();
            EmergencyPhoneErrorLabel.IsVisible = true;
            await DisplayAlert("Atenção", "Contato de emergência inválido. " + PhoneValidationHelper.GetValidationErrorMessage(), "OK");
            ContatoEmergenciaEntry.Focus();
            return false;
        }
        else
        {
            EmergencyPhoneErrorLabel.IsVisible = false;
        }

        if (PhoneValidationHelper.GetOnlyNumbers(ContatoEmergenciaEntry.Text?.Trim()).Length != 11)
        {
            PhoneErrorLabel.Text = "Telefone de emergência precisa ter 11 dígitos";
            PhoneErrorLabel.IsVisible = true;
            await DisplayAlert("Atenção", "Telefone de emergência precisa ter 11 dígitos", "OK");
            PhoneEntry.Focus();
            return false;
        }


        // 8. Validar CNH
        if (string.IsNullOrWhiteSpace(CNHEntry.Text))
        {
            await DisplayAlert("Atenção", "CNH é obrigatória", "OK");
            CNHEntry.Focus();
            return false;
        }

        // 9. Validar Senha
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

    private void OnAlreadyHaveAccount_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new Views.LoginPage());
    }
}