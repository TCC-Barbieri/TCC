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

        BirthDatePicker.MaximumDate = MaxDriverBirthDate;
        BirthDatePicker.Date = MaxDriverBirthDate;
    }

    private async void OnRegister_Clicked(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("Register button clicked");

        try
        {
            // Limpar todas as mensagens de erro primeiro
            ClearAllErrors();

            // Validar antes de obter localização
            if (!await ValidateAllFieldsAsync())
            {
                System.Diagnostics.Debug.WriteLine("Validation failed - aborting registration");
                return;
            }

            var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(5));
            var location = await Geolocation.Default.GetLocationAsync(request);

            System.Diagnostics.Debug.WriteLine("All validations passed - proceeding with registration");

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

    private void ClearAllErrors()
    {
        NameErrorLabel.IsVisible = false;
        RGErrorLabel.IsVisible = false;
        CPFErrorLabel.IsVisible = false;
        EmailErrorLabel.IsVisible = false;
        BirthDateErrorLabel.IsVisible = false;
        PhoneErrorLabel.IsVisible = false;
        EmergencyPhoneErrorLabel.IsVisible = false;
        CNHErrorLabel.IsVisible = false;
        GenderErrorLabel.IsVisible = false;
        AddressErrorLabel.IsVisible = false;
        PasswordErrorLabel.IsVisible = false;
        ConfirmPasswordErrorLabel.IsVisible = false;
    }

    private async Task<bool> ValidateAllFieldsAsync()
    {
        System.Diagnostics.Debug.WriteLine("Starting field validation");
        bool isValid = true;

        // 1. Nome
        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            NameErrorLabel.Text = "Nome é obrigatório";
            NameErrorLabel.IsVisible = true;
            isValid = false;
        }
        else if (NameEntry.Text.Trim().Length < 3)
        {
            NameErrorLabel.Text = "Nome deve ter pelo menos 3 caracteres";
            NameErrorLabel.IsVisible = true;
            isValid = false;
        }

        // 2. RG
        if (string.IsNullOrWhiteSpace(RGEntry.Text))
        {
            RGErrorLabel.Text = "RG é obrigatório";
            RGErrorLabel.IsVisible = true;
            isValid = false;
        }
        else
        {
            string rgText = RGEntry.Text?.Trim();
            if (!RGValidatorHelper.IsValid(rgText))
            {
                RGErrorLabel.Text = "RG inválido. Verifique os números digitados";
                RGErrorLabel.IsVisible = true;
                isValid = false;
            }
        }

        // 3. CPF
        if (string.IsNullOrWhiteSpace(CPFEntry.Text))
        {
            CPFErrorLabel.Text = "CPF é obrigatório";
            CPFErrorLabel.IsVisible = true;
            isValid = false;
        }
        else
        {
            string cpfText = CPFEntry.Text?.Trim();
            if (!CPFValidator.IsValid(cpfText))
            {
                CPFErrorLabel.Text = "CPF inválido. Verifique os números digitados";
                CPFErrorLabel.IsVisible = true;
                isValid = false;
            }
        }

        // 4. Email
        if (string.IsNullOrWhiteSpace(EmailEntry.Text))
        {
            EmailErrorLabel.Text = "Email é obrigatório";
            EmailErrorLabel.IsVisible = true;
            isValid = false;
        }
        else if (!IsValidEmail(EmailEntry.Text.Trim()))
        {
            EmailErrorLabel.Text = "Email inválido. Use o formato: exemplo@email.com";
            EmailErrorLabel.IsVisible = true;
            isValid = false;
        }

        // 5. Data de Nascimento
        var age = DateTime.Today.Year - BirthDatePicker.Date.Year;
        if (BirthDatePicker.Date > DateTime.Today.AddYears(-age)) age--;

        if (age < 18)
        {
            BirthDateErrorLabel.Text = "Motorista deve ter no mínimo 18 anos";
            BirthDateErrorLabel.IsVisible = true;
            isValid = false;
        }

        // 6. Telefone
        if (string.IsNullOrWhiteSpace(PhoneEntry.Text))
        {
            PhoneErrorLabel.Text = "Telefone é obrigatório";
            PhoneErrorLabel.IsVisible = true;
            isValid = false;
        }
        else
        {
            var phoneNumbers = PhoneValidationHelper.GetOnlyNumbers(PhoneEntry.Text?.Trim());
            if (phoneNumbers.Length != 11)
            {
                PhoneErrorLabel.Text = "Telefone deve ter 11 dígitos (DDD + número)";
                PhoneErrorLabel.IsVisible = true;
                isValid = false;
            }
            else if (!PhoneValidationHelper.IsValidPhone(PhoneEntry.Text?.Trim()))
            {
                PhoneErrorLabel.Text = PhoneValidationHelper.GetValidationErrorMessage();
                PhoneErrorLabel.IsVisible = true;
                isValid = false;
            }
        }

        // 7. Telefone de Emergência
        if (string.IsNullOrWhiteSpace(ContatoEmergenciaEntry.Text))
        {
            EmergencyPhoneErrorLabel.Text = "Contato de emergência é obrigatório";
            EmergencyPhoneErrorLabel.IsVisible = true;
            isValid = false;
        }
        else
        {
            var emergencyNumbers = PhoneValidationHelper.GetOnlyNumbers(ContatoEmergenciaEntry.Text?.Trim());
            if (emergencyNumbers.Length != 11)
            {
                EmergencyPhoneErrorLabel.Text = "Telefone de emergência deve ter 11 dígitos (DDD + número)";
                EmergencyPhoneErrorLabel.IsVisible = true;
                isValid = false;
            }
            else if (!PhoneValidationHelper.IsValidPhone(ContatoEmergenciaEntry.Text?.Trim()))
            {
                EmergencyPhoneErrorLabel.Text = PhoneValidationHelper.GetValidationErrorMessage();
                EmergencyPhoneErrorLabel.IsVisible = true;
                isValid = false;
            }
        }

        // 8. CNH
        if (string.IsNullOrWhiteSpace(CNHEntry.Text))
        {
            CNHErrorLabel.Text = "CNH é obrigatória";
            CNHErrorLabel.IsVisible = true;
            isValid = false;
        }
        else if (CNHEntry.Text.Trim().Length != 11)
        {
            CNHErrorLabel.Text = "CNH deve ter 11 dígitos";
            CNHErrorLabel.IsVisible = true;
            isValid = false;
        }

        // 9. Gênero
        if (GenderPicker.SelectedIndex == -1)
        {
            GenderErrorLabel.Text = "Selecione seu gênero";
            GenderErrorLabel.IsVisible = true;
            isValid = false;
        }

        // 10. Endereço
        if (string.IsNullOrWhiteSpace(AddressEntry.Text))
        {
            AddressErrorLabel.Text = "Endereço é obrigatório";
            AddressErrorLabel.IsVisible = true;
            isValid = false;
        }
        else if (AddressEntry.Text.Trim().Length < 10)
        {
            AddressErrorLabel.Text = "Endereço deve conter pelo menos 10 caracteres";
            AddressErrorLabel.IsVisible = true;
            isValid = false;
        }

        // 11. Senha
        if (string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            PasswordErrorLabel.Text = "Senha é obrigatória";
            PasswordErrorLabel.IsVisible = true;
            isValid = false;
        }
        else if (PasswordEntry.Text.Length < 6)
        {
            PasswordErrorLabel.Text = "Senha deve ter no mínimo 6 caracteres";
            PasswordErrorLabel.IsVisible = true;
            isValid = false;
        }

        // 12. Confirmar Senha
        if (string.IsNullOrWhiteSpace(ConfirmPasswordEntry.Text))
        {
            ConfirmPasswordErrorLabel.Text = "Confirmação de senha é obrigatória";
            ConfirmPasswordErrorLabel.IsVisible = true;
            isValid = false;
        }
        else if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
        {
            ConfirmPasswordErrorLabel.Text = "As senhas não coincidem";
            ConfirmPasswordErrorLabel.IsVisible = true;
            isValid = false;
        }

        if (!isValid)
        {
            await DisplayAlert("Atenção", "Por favor, corrija os campos destacados em vermelho", "OK");
        }

        System.Diagnostics.Debug.WriteLine($"Validation result: {isValid}");
        return isValid;
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email && email.Contains("@") && email.Contains(".");
        }
        catch
        {
            return false;
        }
    }

    private async void OnAlreadyHaveAccount_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Views.LoginPage());
    }
}