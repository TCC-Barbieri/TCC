using TCC.Models;
using TCC.Services;
using TCC.Helpers;

namespace TCC.Views;

public partial class DriverRegisterPage : ContentPage
{
    private readonly DatabaseService _databaseService = new();

    public DriverRegisterPage()
    {
        InitializeComponent();

        // Motorista deve ter no mínimo 18 anos
        BirthDatePicker.MaximumDate = DateTime.Today.AddYears(-18);
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        try
        {
            if (!ValidateRequiredFields().IsValid)
            {
                var res = ValidateRequiredFields();
                await DisplayAlert("Atenção", res.Message, "OK");
                return;
            }

            // Validação RG
            if (!RGValidatorHelper.IsValid(RGEntry.Text.Trim()))
            {
                await DisplayAlert("Erro", "RG inválido", "OK");
                return;
            }

            // Validação CPF
            if (!CPFValidator.IsValid(CPFEntry.Text.Trim()))
            {
                await DisplayAlert("Erro", "CPF inválido", "OK");
                return;
            }

            // Idade mínima: 18 anos
            int age = DateTime.Today.Year - BirthDatePicker.Date.Year;
            if (BirthDatePicker.Date > DateTime.Today.AddYears(-age)) age--;

            if (age < 18)
            {
                await DisplayAlert("Erro", "O motorista deve ter pelo menos 18 anos.", "OK");
                return;
            }

            // Telefone
            if (!PhoneValidationHelper.IsValidPhone(PhoneEntry.Text))
            {
                await DisplayAlert("Erro", PhoneValidationHelper.GetValidationErrorMessage(), "OK");
                return;
            }

            // Contato emergência
            if (!PhoneValidationHelper.IsValidPhone(ContatoEmergenciaEntry.Text))
            {
                await DisplayAlert("Erro", PhoneValidationHelper.GetValidationErrorMessage(), "OK");
                return;
            }

            // Senha
            if (PasswordEntry.Text.Length < 6)
            {
                await DisplayAlert("Erro", "Senha deve ter pelo menos 6 caracteres.", "OK");
                return;
            }

            if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
            {
                await DisplayAlert("Erro", "As senhas não coincidem.", "OK");
                return;
            }

            // 🔵 VALIDA APENAS MOTORISTAS
            var uniqueCheck = await _databaseService.ValidateUniqueUserData(
                rg: RGValidatorHelper.RemoveFormat(RGEntry.Text.Trim()),
                cpf: CPFValidator.RemoveFormat(CPFEntry.Text.Trim()),
                email: EmailEntry.Text.Trim(),
                phone: PhoneValidationHelper.GetOnlyNumbers(PhoneEntry.Text.Trim()),
                cnh: CNHEntry.Text.Trim(),
                userType: "driver"
            );

            if (!uniqueCheck.IsValid)
            {
                await DisplayAlert("Atenção", uniqueCheck.Message, "OK");
                return;
            }

            Driver driver = new Driver
            {
                Name = NameEntry.Text.Trim(),
                RG = RGValidatorHelper.RemoveFormat(RGEntry.Text.Trim()),
                CPF = CPFValidator.RemoveFormat(CPFEntry.Text.Trim()),
                Email = EmailEntry.Text.Trim(),
                PhoneNumber = PhoneValidationHelper.GetOnlyNumbers(PhoneEntry.Text.Trim()),
                EmergencyPhoneNumber = PhoneValidationHelper.GetOnlyNumbers(ContatoEmergenciaEntry.Text.Trim()),
                BirthDate = BirthDatePicker.Date,
                Address = AddressEntry.Text.Trim(),
                CNH = CNHEntry.Text.Trim(),
                Genre = GenderPicker.SelectedItem?.ToString() ?? "Não especificado",
                Password = PasswordEntry.Text.Trim(),
                Latitude = 0,
                Longitude = 0
            };

            await _databaseService.CreateDriver(driver);

            await DisplayAlert("Sucesso", "Motorista registrado com sucesso!", "OK");
            await Navigation.PushAsync(new LoginPage());
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
            return (false, "Informe o email.");

        if (string.IsNullOrWhiteSpace(PhoneEntry.Text))
            return (false, "Informe o telefone.");

        if (string.IsNullOrWhiteSpace(ContatoEmergenciaEntry.Text))
            return (false, "Informe o contato de emergência.");

        if (string.IsNullOrWhiteSpace(AddressEntry.Text))
            return (false, "Informe o endereço.");

        if (string.IsNullOrWhiteSpace(CNHEntry.Text))
            return (false, "Informe a CNH.");

        if (GenderPicker.SelectedIndex == -1)
            return (false, "Selecione o gênero.");

        if (string.IsNullOrWhiteSpace(PasswordEntry.Text))
            return (false, "Informe a senha.");

        if (string.IsNullOrWhiteSpace(ConfirmPasswordEntry.Text))
            return (false, "Confirme a senha.");

        return (true, "");
    }

    private async void OnAlreadyHaveAccount_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LoginPage());
    }
}
