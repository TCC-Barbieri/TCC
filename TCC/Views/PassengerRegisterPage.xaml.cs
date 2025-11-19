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
            ClearAllErrors();

            if (!await ValidateAllFieldsAsync())
                return;

            var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(5));
            var location = await Geolocation.Default.GetLocationAsync(request);

            // 🔵 AGORA A VALIDAÇÃO É APENAS PARA PASSAGEIRO
            var uniqueValidation = await _databaseService.ValidateUniqueUserData(
                rg: RGValidatorHelper.RemoveFormat(RGEntry.Text.Trim()),
                cpf: CPFValidator.RemoveFormat(CPFEntry.Text.Trim()),
                email: EmailEntry.Text.Trim(),
                phone: PhoneValidationHelper.GetOnlyNumbers(PhoneEntry.Text.Trim()),
                userType: "passenger" // 🔥 IMPORTANTE
            );

            if (!uniqueValidation.IsValid)
            {
                await DisplayAlert("Dados já cadastrados", uniqueValidation.Message, "OK");
                return;
            }

            Passenger passenger = new Passenger
            {
                Name = NameEntry.Text.Trim(),
                Password = PasswordEntry.Text.Trim(),
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
                SpecialTreatmentObservations = AtendimentoSimRadio.IsChecked ? SpecialTreatmentEditor.Text?.Trim() ?? "" : "",
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
            await DisplayAlert("Erro", $"Erro ao registrar: {ex.Message}", "OK");
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
        bool valid = true;

        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            NameErrorLabel.Text = "Nome é obrigatório";
            NameErrorLabel.IsVisible = true;
            valid = false;
        }

        if (string.IsNullOrWhiteSpace(RGEntry.Text) ||
            !RGValidatorHelper.IsValid(RGEntry.Text.Trim()))
        {
            RGErrorLabel.Text = "RG inválido";
            RGErrorLabel.IsVisible = true;
            valid = false;
        }

        if (string.IsNullOrWhiteSpace(CPFEntry.Text) ||
            !CPFValidator.IsValid(CPFEntry.Text.Trim()))
        {
            CPFErrorLabel.Text = "CPF inválido";
            CPFErrorLabel.IsVisible = true;
            valid = false;
        }

        if (string.IsNullOrWhiteSpace(EmailEntry.Text) ||
            !IsValidEmail(EmailEntry.Text.Trim()))
        {
            EmailErrorLabel.Text = "Email inválido";
            EmailErrorLabel.IsVisible = true;
            valid = false;
        }

        int age = DateTime.Today.Year - BirthDatePicker.Date.Year;
        if (BirthDatePicker.Date > DateTime.Today.AddYears(-age)) age--;

        if (age < 14)
        {
            BirthDateErrorLabel.Text = "Idade mínima: 14 anos";
            BirthDateErrorLabel.IsVisible = true;
            valid = false;
        }

        if (!PhoneValidationHelper.IsValidPhone(PhoneEntry.Text))
        {
            PhoneErrorLabel.Text = PhoneValidationHelper.GetValidationErrorMessage();
            PhoneErrorLabel.IsVisible = true;
            valid = false;
        }

        if (!PhoneValidationHelper.IsValidPhone(EmergencyPhoneEntry.Text))
        {
            EmergencyPhoneErrorLabel.Text = PhoneValidationHelper.GetValidationErrorMessage();
            EmergencyPhoneErrorLabel.IsVisible = true;
            valid = false;
        }

        if (GenderPicker.SelectedIndex == -1)
        {
            GenderErrorLabel.Text = "Selecione o gênero";
            GenderErrorLabel.IsVisible = true;
            valid = false;
        }

        if (SchoolPicker.SelectedIndex == -1)
        {
            SchoolErrorLabel.Text = "Selecione a escola";
            SchoolErrorLabel.IsVisible = true;
            valid = false;
        }

        if (string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            PasswordErrorLabel.Text = "Senha obrigatória";
            PasswordErrorLabel.IsVisible = true;
            valid = false;
        }

        if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
        {
            ConfirmPasswordErrorLabel.Text = "As senhas não coincidem";
            ConfirmPasswordErrorLabel.IsVisible = true;
            valid = false;
        }

        if (!valid)
            await DisplayAlert("Atenção", "Corrija os campos destacados", "OK");

        return valid;
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private void ClearFields()
    {
        NameEntry.Text = "";
        PasswordEntry.Text = "";
        ConfirmPasswordEntry.Text = "";
        EmailEntry.Text = "";
        PhoneEntry.Text = "";
        EmergencyPhoneEntry.Text = "";
        AddressEntry.Text = "";
        BackupAddressEntry.Text = "";
        RGEntry.Text = "";
        CPFEntry.Text = "";
        ResponsibleEntry.Text = "";
        SpecialTreatmentEditor.Text = "";
        GenderPicker.SelectedIndex = -1;
        SchoolPicker.SelectedIndex = -1;
        AtendimentoNaoRadio.IsChecked = true;
    }

    private async void OnAlreadyHaveAccount_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LoginPage());
    }
}
