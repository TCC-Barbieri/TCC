using TCC.Models;
using TCC.Services;
using TCC.Helpers;

namespace TCC.Views;

public partial class PassengerRegisterPage : ContentPage
{
    private readonly DatabaseService _databaseService = new();

    public DateTime MaxPassengerBirthDate => DateTime.Today.AddYears(-14);

    public PassengerRegisterPage()
    {
        InitializeComponent();

        BirthDatePicker.MaximumDate = MaxPassengerBirthDate;
        BirthDatePicker.Date = MaxPassengerBirthDate;
    }

    private void OnSimTapped(object sender, EventArgs e)
    {
        AtendimentoSimRadio.IsChecked = true;
    }

    private void OnNaoTapped(object sender, EventArgs e)
    {
        AtendimentoNaoRadio.IsChecked = true;
    }

    private void OnAtendimentoEspecialChanged(object sender, CheckedChangedEventArgs e)
    {
        SpecialTreatmentDetailsLayout.IsVisible = AtendimentoSimRadio.IsChecked;

        if (!AtendimentoSimRadio.IsChecked)
        {
            SpecialTreatmentEditor.Text = string.Empty;
            SpecialTreatmentErrorLabel.IsVisible = false;
        }
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        try
        {
            // Limpar todas as mensagens de erro
            ClearAllErrors();

            // Validar campos antes de prosseguir
            if (!await ValidateAllFieldsAsync())
            {
                return;
            }

            var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(5));
            var location = await Geolocation.Default.GetLocationAsync(request);

            // Validação de dados únicos
            var uniqueDataValidation = await _databaseService.ValidateUniqueUserData(
                rg: RGValidatorHelper.RemoveFormat(RGEntry.Text.Trim()),
                cpf: CPFValidator.RemoveFormat(CPFEntry.Text.Trim()),
                email: EmailEntry.Text.Trim(),
                phone: PhoneValidationHelper.GetOnlyNumbers(PhoneEntry.Text.Trim())
            );

            if (!uniqueDataValidation.IsValid)
            {
                await DisplayAlert("Dados já cadastrados", uniqueDataValidation.Message, "OK");
                return;
            }

            Passenger passenger = new Passenger
            {
                Name = NameEntry.Text.Trim(),
                Password = PasswordEntry.Text,
                Email = EmailEntry.Text.Trim(),
                PhoneNumber = PhoneValidationHelper.GetOnlyNumbers(PhoneEntry.Text.Trim()),
                EmergencyPhoneNumber = PhoneValidationHelper.GetOnlyNumbers(EmergencyPhoneEntry.Text.Trim()),
                Address = AddressEntry.Text.Trim(),
                ReservableAddress = BackupAddressEntry.Text.Trim(),
                RG = RGValidatorHelper.RemoveFormat(RGEntry.Text.Trim()),
                CPF = CPFValidator.RemoveFormat(CPFEntry.Text.Trim()),
                Genre = GenderPicker.SelectedItem?.ToString() ?? "Não especificado",
                School = SchoolPicker.SelectedItem?.ToString() ?? "Não especificado",
                ResponsableName = ResponsibleEntry.Text.Trim(),
                SpecialTreatment = AtendimentoSimRadio.IsChecked,
                SpecialTreatmentObservations = AtendimentoSimRadio.IsChecked
                    ? SpecialTreatmentEditor.Text?.Trim() ?? string.Empty
                    : string.Empty,
                BirthDate = BirthDatePicker.Date,
                Longitude = location.Longitude,
                Latitude = location.Latitude
            };

            await _databaseService.CreatePassenger(passenger);

            await DisplayAlert("Sucesso", "Passageiro registrado com sucesso!", "OK");

            ClearFields();

            await Navigation.PushAsync(new LoginPage());
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao registrar passageiro: {ex.Message}", "OK");
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
        GenderErrorLabel.IsVisible = false;
        SchoolErrorLabel.IsVisible = false;
        AddressErrorLabel.IsVisible = false;
        BackupAddressErrorLabel.IsVisible = false;
        ResponsibleErrorLabel.IsVisible = false;
        SpecialTreatmentErrorLabel.IsVisible = false;
        PasswordErrorLabel.IsVisible = false;
        ConfirmPasswordErrorLabel.IsVisible = false;
    }

    private async Task<bool> ValidateAllFieldsAsync()
    {
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
        else if (!RGValidatorHelper.IsValid(RGEntry.Text?.Trim()))
        {
            RGErrorLabel.Text = "RG inválido. Verifique os números digitados";
            RGErrorLabel.IsVisible = true;
            isValid = false;
        }

        // 3. CPF
        if (string.IsNullOrWhiteSpace(CPFEntry.Text))
        {
            CPFErrorLabel.Text = "CPF é obrigatório";
            CPFErrorLabel.IsVisible = true;
            isValid = false;
        }
        else if (!CPFValidator.IsValid(CPFEntry.Text?.Trim()))
        {
            CPFErrorLabel.Text = "CPF inválido. Verifique os números digitados";
            CPFErrorLabel.IsVisible = true;
            isValid = false;
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

        if (age < 14)
        {
            BirthDateErrorLabel.Text = "Passageiro deve ter no mínimo 14 anos";
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
        if (string.IsNullOrWhiteSpace(EmergencyPhoneEntry.Text))
        {
            EmergencyPhoneErrorLabel.Text = "Contato de emergência é obrigatório";
            EmergencyPhoneErrorLabel.IsVisible = true;
            isValid = false;
        }
        else
        {
            var emergencyNumbers = PhoneValidationHelper.GetOnlyNumbers(EmergencyPhoneEntry.Text?.Trim());
            if (emergencyNumbers.Length != 11)
            {
                EmergencyPhoneErrorLabel.Text = "Telefone de emergência deve ter 11 dígitos (DDD + número)";
                EmergencyPhoneErrorLabel.IsVisible = true;
                isValid = false;
            }
            else if (!PhoneValidationHelper.IsValidPhone(EmergencyPhoneEntry.Text?.Trim()))
            {
                EmergencyPhoneErrorLabel.Text = PhoneValidationHelper.GetValidationErrorMessage();
                EmergencyPhoneErrorLabel.IsVisible = true;
                isValid = false;
            }
        }

        // 8. Gênero
        if (GenderPicker.SelectedIndex == -1)
        {
            GenderErrorLabel.Text = "Selecione seu gênero";
            GenderErrorLabel.IsVisible = true;
            isValid = false;
        }

        // 9. Escola
        if (SchoolPicker.SelectedIndex == -1)
        {
            SchoolErrorLabel.Text = "Selecione sua escola";
            SchoolErrorLabel.IsVisible = true;
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

        // 11. Endereço Alternativo
        if (string.IsNullOrWhiteSpace(BackupAddressEntry.Text))
        {
            BackupAddressErrorLabel.Text = "Endereço alternativo é obrigatório";
            BackupAddressErrorLabel.IsVisible = true;
            isValid = false;
        }
        else if (BackupAddressEntry.Text.Trim().Length < 10)
        {
            BackupAddressErrorLabel.Text = "Endereço deve conter pelo menos 10 caracteres";
            BackupAddressErrorLabel.IsVisible = true;
            isValid = false;
        }

        // 12. Nome do Responsável
        if (string.IsNullOrWhiteSpace(ResponsibleEntry.Text))
        {
            ResponsibleErrorLabel.Text = "Nome do responsável é obrigatório";
            ResponsibleErrorLabel.IsVisible = true;
            isValid = false;
        }
        else if (ResponsibleEntry.Text.Trim().Length < 3)
        {
            ResponsibleErrorLabel.Text = "Nome deve ter pelo menos 3 caracteres";
            ResponsibleErrorLabel.IsVisible = true;
            isValid = false;
        }

        // 13. Atendimento Especial
        if (AtendimentoSimRadio.IsChecked && string.IsNullOrWhiteSpace(SpecialTreatmentEditor.Text))
        {
            SpecialTreatmentErrorLabel.Text = "Descreva os detalhes do atendimento especial";
            SpecialTreatmentErrorLabel.IsVisible = true;
            isValid = false;
        }

        // 14. Senha
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

        // 15. Confirmar Senha
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

    private void ClearFields()
    {
        NameEntry.Text = string.Empty;
        PasswordEntry.Text = string.Empty;
        ConfirmPasswordEntry.Text = string.Empty;
        EmailEntry.Text = string.Empty;
        PhoneEntry.Text = string.Empty;
        EmergencyPhoneEntry.Text = string.Empty;
        AddressEntry.Text = string.Empty;
        BackupAddressEntry.Text = string.Empty;
        RGEntry.Text = string.Empty;
        CPFEntry.Text = string.Empty;
        ResponsibleEntry.Text = string.Empty;
        SpecialTreatmentEditor.Text = string.Empty;

        GenderPicker.SelectedIndex = -1;
        SchoolPicker.SelectedIndex = -1;
        BirthDatePicker.Date = MaxPassengerBirthDate;

        AtendimentoNaoRadio.IsChecked = true;
        SpecialTreatmentDetailsLayout.IsVisible = false;

        ClearAllErrors();
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        string dbPath = FileSystem.AppDataDirectory;
        await DisplayAlert("Database Path", dbPath, "OK");
    }

    private async void OnAlreadyHaveAccount_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Views.LoginPage());
    }
}