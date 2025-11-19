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

            if (_currentDriver == null)
            {
                await DisplayAlert("Erro", "Motorista não encontrado.", "OK");
                await Navigation.PopAsync();
                return;
            }

            NameEntry.Text = _currentDriver.Name;
            RGEntry.Text = _currentDriver.RG;
            CPFEntry.Text = _currentDriver.CPF;
            EmailEntry.Text = _currentDriver.Email;

            PhoneEntry.Text = PhoneValidationHelper.FormatPhone(_currentDriver.PhoneNumber);
            ContatoEmergenciaEntry.Text = PhoneValidationHelper.FormatPhone(_currentDriver.EmergencyPhoneNumber);

            AddressEntry.Text = _currentDriver.Address;
            CNHEntry.Text = _currentDriver.CNH;
            BirthDatePicker.Date = _currentDriver.BirthDate;

            GenderPicker.SelectedItem = _currentDriver.Genre;

        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", ex.Message, "OK");
        }
    }

    private async void OnSave_Clicked(object sender, EventArgs e)
    {
        try
        {
            var fieldsCheck = ValidateRequiredFields();
            if (!fieldsCheck.IsValid)
            {
                await DisplayAlert("Atenção", fieldsCheck.Message, "OK");
                return;
            }

            if (!RGValidatorHelper.IsValid(RGEntry.Text.Trim()))
            {
                await DisplayAlert("Erro", "RG inválido.", "OK");
                return;
            }

            if (!CPFValidator.IsValid(CPFEntry.Text.Trim()))
            {
                await DisplayAlert("Erro", "CPF inválido.", "OK");
                return;
            }

            int age = DateTime.Today.Year - BirthDatePicker.Date.Year;
            if (BirthDatePicker.Date > DateTime.Today.AddYears(-age)) age--;

            if (age < 18)
            {
                await DisplayAlert("Erro", "Motorista deve ter no mínimo 18 anos.", "OK");
                return;
            }

            if (!PhoneValidationHelper.IsValidPhone(PhoneEntry.Text))
            {
                await DisplayAlert("Erro", PhoneValidationHelper.GetValidationErrorMessage(), "OK");
                return;
            }

            if (!PhoneValidationHelper.IsValidPhone(ContatoEmergenciaEntry.Text))
            {
                await DisplayAlert("Erro", PhoneValidationHelper.GetValidationErrorMessage(), "OK");
                return;
            }

            if (!ValidatePasswordChange())
            {
                await DisplayAlert("Erro", "As senhas não coincidem ou são muito curtas.", "OK");
                return;
            }

            // 🔵 VALIDA APENAS MOTORISTA
            var uniqueCheck = await _databaseService.ValidateUniqueUserData(
                rg: RGValidatorHelper.RemoveFormat(RGEntry.Text.Trim()),
                cpf: CPFValidator.RemoveFormat(CPFEntry.Text.Trim()),
                email: EmailEntry.Text.Trim(),
                phone: PhoneValidationHelper.GetOnlyNumbers(PhoneEntry.Text.Trim()),
                cnh: CNHEntry.Text.Trim(),
                excludeUserId: _driverId,
                userType: "driver"
            );

            if (!uniqueCheck.IsValid)
            {
                await DisplayAlert("Erro", uniqueCheck.Message, "OK");
                return;
            }

            UpdateDriverModel();

            await _databaseService.UpdateDriver(_currentDriver);

            await DisplayAlert("Sucesso", "Dados atualizados com sucesso!", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", ex.Message, "OK");
        }
    }

    private (bool IsValid, string Message) ValidateRequiredFields()
    {
        if (string.IsNullOrWhiteSpace(NameEntry.Text))
            return (false, "Informe o nome.");

        if (string.IsNullOrWhiteSpace(RGEntry.Text))
            return (false, "Informe o RG.");

        if (string.IsNullOrWhiteSpace(CPFEntry.Text))
            return (false, "Informe o CPF.");

        if (string.IsNullOrWhiteSpace(EmailEntry.Text))
            return (false, "Informe o Email.");

        if (string.IsNullOrWhiteSpace(PhoneEntry.Text))
            return (false, "Informe o Telefone.");

        if (string.IsNullOrWhiteSpace(ContatoEmergenciaEntry.Text))
            return (false, "Informe o Contato de Emergência.");

        if (string.IsNullOrWhiteSpace(AddressEntry.Text))
            return (false, "Informe o Endereço.");

        if (string.IsNullOrWhiteSpace(CNHEntry.Text))
            return (false, "Informe a CNH.");

        if (GenderPicker.SelectedIndex == -1)
            return (false, "Selecione o gênero.");

        return (true, "");
    }

    private bool ValidatePasswordChange()
    {
        string newPass = NewPasswordEntry.Text?.Trim();
        string confirm = ConfirmNewPasswordEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(newPass) && string.IsNullOrWhiteSpace(confirm))
            return true;

        if (newPass != confirm)
            return false;

        if (newPass.Length < 6)
            return false;

        return true;
    }

    private void UpdateDriverModel()
    {
        _currentDriver.Name = NameEntry.Text.Trim();
        _currentDriver.RG = RGValidatorHelper.RemoveFormat(RGEntry.Text.Trim());
        _currentDriver.CPF = CPFValidator.RemoveFormat(CPFEntry.Text.Trim());
        _currentDriver.Email = EmailEntry.Text.Trim();

        _currentDriver.PhoneNumber = PhoneValidationHelper.GetOnlyNumbers(PhoneEntry.Text.Trim());
        _currentDriver.EmergencyPhoneNumber = PhoneValidationHelper.GetOnlyNumbers(ContatoEmergenciaEntry.Text.Trim());

        _currentDriver.Genre = GenderPicker.SelectedItem.ToString();
        _currentDriver.Address = AddressEntry.Text.Trim();
        _currentDriver.CNH = CNHEntry.Text.Trim();
        _currentDriver.BirthDate = BirthDatePicker.Date;

        if (!string.IsNullOrWhiteSpace(NewPasswordEntry.Text))
            _currentDriver.Password = NewPasswordEntry.Text.Trim();
    }

    private async void OnCancel_Clicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Cancelar", "Deseja descartar alterações?", "Sim", "Não");

        if (confirm)
            await Navigation.PopAsync();
    }
}
