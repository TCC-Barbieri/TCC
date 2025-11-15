using TCC.Models;
using TCC.Services;
using TCC.Helpers;

namespace TCC.Views;

public partial class DriverEditPage : ContentPage
{
    private readonly DatabaseService _databaseService = new();
    private readonly int _driverId;
    private Driver _currentDriver;

    public DriverEditPage(int driverId)
    {
        InitializeComponent();
        _driverId = driverId;

        // Motorista deve ter no mínimo 18 anos
        BirthDatePicker.MaximumDate = DateTime.Today.AddYears(-18);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDriverData();
    }

    private async Task LoadDriverData()
    {
        try
        {
            var drivers = await _databaseService.GetDrivers();
            _currentDriver = drivers.FirstOrDefault(d => d.Id == _driverId);

            if (_currentDriver != null)
            {
                NameEntry.Text = _currentDriver.Name ?? string.Empty;
                RGEntry.Text = _currentDriver.RG ?? string.Empty;
                CPFEntry.Text = _currentDriver.CPF ?? string.Empty;

                EmailEntry.Text = _currentDriver.Email ?? string.Empty;
                AddressEntry.Text = _currentDriver.Address ?? string.Empty;
                CNHEntry.Text = _currentDriver.CNH ?? string.Empty;

                BirthDatePicker.Date = _currentDriver.BirthDate;

                PhoneEntry.Text = !string.IsNullOrWhiteSpace(_currentDriver.PhoneNumber)
                    ? PhoneValidationHelper.FormatPhone(_currentDriver.PhoneNumber)
                    : string.Empty;

                ContatoEmergenciaEntry.Text = !string.IsNullOrWhiteSpace(_currentDriver.EmergencyPhoneNumber)
                    ? PhoneValidationHelper.FormatPhone(_currentDriver.EmergencyPhoneNumber)
                    : string.Empty;

                SetPickerSelection(GenderPicker, _currentDriver.Genre);
            }
            else
            {
                await DisplayAlert("Erro", "Motorista não encontrado.", "OK");
                await Navigation.PopAsync();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao carregar dados: {ex.Message}", "OK");
        }
    }

    private void SetPickerSelection(Picker picker, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            var index = picker.Items.IndexOf(value);
            if (index >= 0)
                picker.SelectedIndex = index;
        }
    }

    private async void OnSave_Clicked(object sender, EventArgs e)
    {
        try
        {
            var validation = ValidateFields();
            if (!validation.IsValid)
            {
                await DisplayAlert("Campos obrigatórios", validation.Message, "OK");
                return;
            }

            // Validar RG
            if (!RGValidatorHelper.IsValid(RGEntry.Text?.Trim()))
            {
                await DisplayAlert("Atenção", "RG inválido. Verifique os números.", "OK");
                return;
            }

            // Validar CPF
            if (!CPFValidator.IsValid(CPFEntry.Text?.Trim()))
            {
                await DisplayAlert("Atenção", "CPF inválido.", "OK");
                return;
            }

            // Idade mínima 18 anos
            var age = DateTime.Today.Year - BirthDatePicker.Date.Year;
            if (BirthDatePicker.Date > DateTime.Today.AddYears(-age)) age--;

            if (age < 18)
            {
                await DisplayAlert("Atenção", "O motorista deve ter no mínimo 18 anos.", "OK");
                return;
            }

            // Validar telefone
            if (!PhoneValidationHelper.IsValidPhone(PhoneEntry.Text?.Trim()))
            {
                await DisplayAlert("Atenção", PhoneValidationHelper.GetValidationErrorMessage(), "OK");
                return;
            }

            // Validar contato emergência
            if (!PhoneValidationHelper.IsValidPhone(ContatoEmergenciaEntry.Text?.Trim()))
            {
                await DisplayAlert("Atenção",
                    "Contato de emergência inválido. " + PhoneValidationHelper.GetValidationErrorMessage(),
                    "OK");
                return;
            }

            // Validar senha
            if (!ValidatePasswordChange())
            {
                await DisplayAlert("Erro", "As senhas não coincidem ou são muito curtas.", "OK");
                return;
            }

            // Validação de unicidade
            var validationResult = await _databaseService.ValidateUniqueUserData(
                rg: RGValidatorHelper.RemoveFormat(RGEntry.Text.Trim()),
                cpf: CPFValidator.RemoveFormat(CPFEntry.Text.Trim()),
                email: EmailEntry.Text.Trim(),
                phone: PhoneValidationHelper.GetOnlyNumbers(PhoneEntry.Text.Trim()),
                cnh: CNHEntry.Text.Trim(),
                excludeUserId: _driverId,
                userType: "driver"
            );

            if (!validationResult.IsValid)
            {
                await DisplayAlert("Atenção", validationResult.Message, "OK");
                return;
            }

            UpdateDriverData();
            await _databaseService.UpdateDriver(_currentDriver);

            await DisplayAlert("Sucesso", "Dados atualizados com sucesso!", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", ex.Message, "OK");
        }
    }

    private (bool IsValid, string Message) ValidateFields()
    {
        var required = new[]
        {
            (NameEntry.Text, "Nome"),
            (RGEntry.Text, "RG"),
            (CPFEntry.Text, "CPF"),
            (EmailEntry.Text, "Email"),
            (PhoneEntry.Text, "Telefone"),
            (ContatoEmergenciaEntry.Text, "Contato de Emergência"),
            (CNHEntry.Text, "CNH"),
            (AddressEntry.Text, "Endereço")
        };

        foreach (var (value, name) in required)
        {
            if (string.IsNullOrWhiteSpace(value))
                return (false, $"Preencha o campo {name}.");
        }

        if (GenderPicker.SelectedItem == null)
            return (false, "Selecione o gênero.");

        return (true, "");
    }

    private bool ValidatePasswordChange()
    {
        var newPass = NewPasswordEntry.Text?.Trim();
        var confirmPass = ConfirmNewPasswordEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(newPass) && string.IsNullOrWhiteSpace(confirmPass))
            return true;

        if (newPass != confirmPass)
            return false;

        return newPass.Length >= 6;
    }

    private void UpdateDriverData()
    {
        _currentDriver.Name = NameEntry.Text.Trim();
        _currentDriver.RG = RGValidatorHelper.RemoveFormat(RGEntry.Text.Trim());
        _currentDriver.CPF = CPFValidator.RemoveFormat(CPFEntry.Text.Trim());
        _currentDriver.Email = EmailEntry.Text.Trim();

        _currentDriver.PhoneNumber = PhoneValidationHelper.GetOnlyNumbers(PhoneEntry.Text.Trim());
        _currentDriver.EmergencyPhoneNumber = PhoneValidationHelper.GetOnlyNumbers(ContatoEmergenciaEntry.Text.Trim());

        _currentDriver.CNH = CNHEntry.Text.Trim();
        _currentDriver.Address = AddressEntry.Text.Trim();

        _currentDriver.Genre = GenderPicker.SelectedItem?.ToString() ?? "Não especificado";
        _currentDriver.BirthDate = BirthDatePicker.Date;

        var newPassword = NewPasswordEntry.Text?.Trim();
        if (!string.IsNullOrEmpty(newPassword))
            _currentDriver.Password = newPassword;
    }

    private async void OnCancel_Clicked(object sender, EventArgs e)
    {
        if (await DisplayAlert("Cancelar", "Deseja descartar as alterações?", "Sim", "Não"))
            await Navigation.PopAsync();
    }
}
