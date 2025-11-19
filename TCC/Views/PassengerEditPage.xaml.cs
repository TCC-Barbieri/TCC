using TCC.Models;
using TCC.Services;
using TCC.Helpers;

namespace TCC.Views;

public partial class PassengerEditPage : ContentPage
{
    private readonly DatabaseService _databaseService = new();
    private readonly int _passengerId;
    private Passenger _currentPassenger;

    public PassengerEditPage(int passengerId)
    {
        InitializeComponent();
        _passengerId = passengerId;

        BirthDatePicker.MaximumDate = DateTime.Today.AddYears(-14);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadPassengerData();
    }

    private async Task LoadPassengerData()
    {
        try
        {
            var passengers = await _databaseService.GetPassengers();
            _currentPassenger = passengers.FirstOrDefault(x => x.Id == _passengerId);

            if (_currentPassenger == null)
            {
                await DisplayAlert("Erro", "Passageiro não encontrado.", "OK");
                await Navigation.PopAsync();
                return;
            }

            NameEntry.Text = _currentPassenger.Name;
            RGEntry.Text = _currentPassenger.RG;
            CPFEntry.Text = _currentPassenger.CPF;
            EmailEntry.Text = _currentPassenger.Email;

            PhoneEntry.Text = PhoneValidationHelper.FormatPhone(_currentPassenger.PhoneNumber);
            EmergencyPhoneEntry.Text = PhoneValidationHelper.FormatPhone(_currentPassenger.EmergencyPhoneNumber);

            AddressEntry.Text = _currentPassenger.Address;
            BackupAddressEntry.Text = _currentPassenger.ReservableAddress;
            ResponsibleEntry.Text = _currentPassenger.ResponsableName;
            BirthDatePicker.Date = _currentPassenger.BirthDate;

            GenderPicker.SelectedItem = _currentPassenger.Genre;
            SchoolPicker.SelectedItem = _currentPassenger.School;

            AtendimentoSimRadio.IsChecked = _currentPassenger.SpecialTreatment;
            AtendimentoNaoRadio.IsChecked = !_currentPassenger.SpecialTreatment;
            SpecialTreatmentDetailsLayout.IsVisible = _currentPassenger.SpecialTreatment;
            SpecialTreatmentEditor.Text = _currentPassenger.SpecialTreatmentObservations;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao carregar dados: {ex.Message}", "OK");
        }
    }

    private void OnAtendimentoEspecialChanged(object sender, CheckedChangedEventArgs e)
    {
        SpecialTreatmentDetailsLayout.IsVisible = AtendimentoSimRadio.IsChecked;

        if (!AtendimentoSimRadio.IsChecked)
            SpecialTreatmentEditor.Text = "";
    }

    private async void OnSave_Clicked(object sender, EventArgs e)
    {
        try
        {
            var requiredFields = ValidateRequiredFields();
            if (!requiredFields.IsValid)
            {
                await DisplayAlert("Atenção", requiredFields.Message, "OK");
                return;
            }

            if (!RGValidatorHelper.IsValid(RGEntry.Text.Trim()))
            {
                await DisplayAlert("Erro", "RG inválido", "OK");
                return;
            }

            if (!CPFValidator.IsValid(CPFEntry.Text.Trim()))
            {
                await DisplayAlert("Erro", "CPF inválido", "OK");
                return;
            }

            if (!PhoneValidationHelper.IsValidPhone(PhoneEntry.Text))
            {
                await DisplayAlert("Erro", PhoneValidationHelper.GetValidationErrorMessage(), "OK");
                return;
            }

            if (!PhoneValidationHelper.IsValidPhone(EmergencyPhoneEntry.Text))
            {
                await DisplayAlert("Erro", "Contato de emergência inválido", "OK");
                return;
            }

            if (!ValidatePasswordChange())
            {
                await DisplayAlert("Erro", "Senha nova inválida ou não coincide", "OK");
                return;
            }

            // 🔵 VALIDA APENAS PASSAGEIROS
            var uniqueCheck = await _databaseService.ValidateUniqueUserData(
                rg: RGValidatorHelper.RemoveFormat(RGEntry.Text.Trim()),
                cpf: CPFValidator.RemoveFormat(CPFEntry.Text.Trim()),
                email: EmailEntry.Text.Trim(),
                phone: PhoneValidationHelper.GetOnlyNumbers(PhoneEntry.Text.Trim()),
                excludeUserId: _passengerId,
                userType: "passenger"
            );

            if (!uniqueCheck.IsValid)
            {
                await DisplayAlert("Dados já cadastrados", uniqueCheck.Message, "OK");
                return;
            }

            UpdatePassengerModel();

            await _databaseService.UpdatePassenger(_currentPassenger);

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

        if (string.IsNullOrWhiteSpace(EmergencyPhoneEntry.Text))
            return (false, "Informe o Contato de Emergência.");

        if (string.IsNullOrWhiteSpace(AddressEntry.Text))
            return (false, "Informe o Endereço.");

        if (string.IsNullOrWhiteSpace(BackupAddressEntry.Text))
            return (false, "Informe o Endereço Alternativo.");

        if (string.IsNullOrWhiteSpace(ResponsibleEntry.Text))
            return (false, "Informe o nome do responsável.");

        if (GenderPicker.SelectedIndex == -1)
            return (false, "Selecione o gênero.");

        if (SchoolPicker.SelectedIndex == -1)
            return (false, "Selecione a escola.");

        if (AtendimentoSimRadio.IsChecked && string.IsNullOrWhiteSpace(SpecialTreatmentEditor.Text))
            return (false, "Descreva o atendimento especial.");

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

    private void UpdatePassengerModel()
    {
        _currentPassenger.Name = NameEntry.Text.Trim();
        _currentPassenger.RG = RGValidatorHelper.RemoveFormat(RGEntry.Text.Trim());
        _currentPassenger.CPF = CPFValidator.RemoveFormat(CPFEntry.Text.Trim());
        _currentPassenger.Email = EmailEntry.Text.Trim();
        _currentPassenger.PhoneNumber = PhoneValidationHelper.GetOnlyNumbers(PhoneEntry.Text.Trim());
        _currentPassenger.EmergencyPhoneNumber = PhoneValidationHelper.GetOnlyNumbers(EmergencyPhoneEntry.Text.Trim());
        _currentPassenger.Address = AddressEntry.Text.Trim();
        _currentPassenger.ReservableAddress = BackupAddressEntry.Text.Trim();
        _currentPassenger.Genre = GenderPicker.SelectedItem.ToString();
        _currentPassenger.School = SchoolPicker.SelectedItem.ToString();
        _currentPassenger.ResponsableName = ResponsibleEntry.Text.Trim();

        _currentPassenger.SpecialTreatment = AtendimentoSimRadio.IsChecked;
        _currentPassenger.SpecialTreatmentObservations =
            AtendimentoSimRadio.IsChecked ? SpecialTreatmentEditor.Text.Trim() : "";

        if (!string.IsNullOrWhiteSpace(NewPasswordEntry.Text))
            _currentPassenger.Password = NewPasswordEntry.Text.Trim();
    }

    private async void OnCancel_Clicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Cancelar", "Descartar alterações?", "Sim", "Não");
        if (confirm)
            await Navigation.PopAsync();
    }
}
