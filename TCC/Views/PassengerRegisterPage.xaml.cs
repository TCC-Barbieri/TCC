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

        // Define a data máxima do DatePicker (14 anos atrás)
        BirthDatePicker.MaximumDate = MaxPassengerBirthDate;
        BirthDatePicker.Date = MaxPassengerBirthDate;
    }

    // Mostrar detalhes se "Sim" for selecionado
    private void OnSimTapped(object sender, EventArgs e)
    {
        AtendimentoSimRadio.IsChecked = true;
    }

    // Ocultar detalhes se "Não" for selecionado
    private void OnNaoTapped(object sender, EventArgs e)
    {
        AtendimentoNaoRadio.IsChecked = true;
    }

    // Controla a visibilidade dos detalhes do atendimento especial
    private void OnAtendimentoEspecialChanged(object sender, CheckedChangedEventArgs e)
    {
        SpecialTreatmentDetailsLayout.IsVisible = AtendimentoSimRadio.IsChecked;

        // Limpa o campo se "Não" for selecionado
        if (!AtendimentoSimRadio.IsChecked)
        {
            SpecialTreatmentEditor.Text = string.Empty;
        }
    }

    // Evento do botão registrar
    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        try
        {
            var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(5));
            var location = await Geolocation.Default.GetLocationAsync(request);

            // Verificação dos campos obrigatórios
            var validationResult = ValidateRequiredFields();
            if (!validationResult.IsValid)
            {
                await DisplayAlert("Campos obrigatórios", validationResult.Message, "OK");
                return;
            }

            // Validar RG
            if (!RGValidatorHelper.IsValid(RGEntry.Text?.Trim()))
            {
                await DisplayAlert("Atenção", "RG inválido. Verifique os números digitados.", "OK");
                RGEntry.Focus();

                if (RGValidation != null)
                {
                    RGValidation.ValidateRG();
                }

                return;
            }

            // Validar CPF
            if (!CPFValidator.IsValid(CPFEntry.Text?.Trim()))
            {
                await DisplayAlert("Atenção", "CPF inválido. Verifique os números digitados.", "OK");
                CPFEntry.Focus();

                if (CPFValidation != null)
                {
                    CPFValidation.ValidateCPF();
                }

                return;
            }

            // Validar idade (mínimo 14 anos)
            var age = DateTime.Today.Year - BirthDatePicker.Date.Year;
            if (BirthDatePicker.Date > DateTime.Today.AddYears(-age)) age--;

            if (age < 14)
            {
                BirthDateErrorLabel.Text = "Passageiro deve ter no mínimo 14 anos";
                BirthDateErrorLabel.IsVisible = true;
                await DisplayAlert("Atenção", "Passageiro deve ter no mínimo 14 anos para se cadastrar", "OK");
                return;
            }
            else
            {
                BirthDateErrorLabel.IsVisible = false;
            }

            // Validar Telefone
            string phoneText = PhoneEntry.Text?.Trim();
            if (!PhoneValidationHelper.IsValidPhone(phoneText))
            {
                PhoneErrorLabel.Text = PhoneValidationHelper.GetValidationErrorMessage();
                PhoneErrorLabel.IsVisible = true;
                await DisplayAlert("Atenção", PhoneValidationHelper.GetValidationErrorMessage(), "OK");
                PhoneEntry.Focus();
                return;
            }
            else
            {
                PhoneErrorLabel.IsVisible = false;
            }

            if (PhoneValidationHelper.GetOnlyNumbers(PhoneEntry.Text?.Trim()).Length != 11)
            {
                PhoneErrorLabel.Text = "Telefone de emergência precisa ter 11 dígitos";
                PhoneErrorLabel.IsVisible = true;
                await DisplayAlert("Atenção", "Telefone de emergência precisa ter 11 dígitos", "OK");
                PhoneEntry.Focus();
            }

            // Validar Telefone de Emergência
            string emergencyPhoneText = EmergencyPhoneEntry.Text?.Trim();
            if (!PhoneValidationHelper.IsValidPhone(emergencyPhoneText))
            {
                EmergencyPhoneErrorLabel.Text = PhoneValidationHelper.GetValidationErrorMessage();
                EmergencyPhoneErrorLabel.IsVisible = true;
                await DisplayAlert("Atenção", "Contato de emergência inválido. " + PhoneValidationHelper.GetValidationErrorMessage(), "OK");
                EmergencyPhoneEntry.Focus();
                return;
            }
            else
            {
                EmergencyPhoneErrorLabel.IsVisible = false;
            }

            if (PhoneValidationHelper.GetOnlyNumbers(EmergencyPhoneEntry.Text?.Trim()).Length != 11)
            {
                PhoneErrorLabel.Text = "Telefone de emergência precisa ter 11 dígitos";
                PhoneErrorLabel.IsVisible = true;
                await DisplayAlert("Atenção", "Telefone de emergência precisa ter 11 dígitos", "OK");
                PhoneEntry.Focus();
            }

            // Verificação de senhas
            if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
            {
                await DisplayAlert("Erro", "As senhas não coincidem.", "OK");
                return;
            }

            if (PasswordEntry.Text.Length < 6)
            {
                await DisplayAlert("Erro", "A senha deve ter pelo menos 6 caracteres.", "OK");
                return;
            }

            if (GenderPicker.SelectedIndex == -1)
            {
                await DisplayAlert("Campos Obrigatórios", "Por favor informe seu gênero", "OK");
                return;
            }

            // Validação de dados únicos (RG, CPF, Email, Telefone)
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

            // Cria o objeto passageiro
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

            // Registra o passageiro
            await _databaseService.CreatePassenger(passenger);

            await DisplayAlert("Sucesso", "Passageiro registrado com sucesso!", "OK");

            // Limpa os campos após registro bem-sucedido
            ClearFields();

            await Navigation.PushAsync(new LoginPage());
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao registrar passageiro: {ex.Message}", "OK");
        }
    }

    private (bool IsValid, string Message) ValidateRequiredFields()
    {
        var requiredFields = new[]
        {
            (NameEntry.Text, "Nome"),
            (PasswordEntry.Text, "Senha"),
            (ConfirmPasswordEntry.Text, "Confirmação de Senha"),
            (EmailEntry.Text, "Email"),
            (PhoneEntry.Text, "Telefone"),
            (EmergencyPhoneEntry.Text, "Contato de Emergência"),
            (AddressEntry.Text, "Endereço"),
            (BackupAddressEntry.Text, "Endereço Alternativo"),
            (RGEntry.Text, "RG"),
            (CPFEntry.Text, "CPF"),
            (ResponsibleEntry.Text, "Nome do Responsável")
        };

        foreach (var (value, fieldName) in requiredFields)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return (false, $"Por favor, preencha o campo {fieldName}.");
            }
        }

        if (GenderPicker.SelectedItem == null)
        {
            return (false, "Por favor, selecione o gênero.");
        }

        if (SchoolPicker.SelectedItem == null)
        {
            return (false, "Por favor, selecione a escola.");
        }

        if (AtendimentoSimRadio.IsChecked && string.IsNullOrWhiteSpace(SpecialTreatmentEditor.Text))
        {
            return (false, "Por favor, descreva os detalhes do atendimento especial.");
        }

        return (true, string.Empty);
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
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        string dbPath = FileSystem.AppDataDirectory;
        await DisplayAlert("Database Path", dbPath, "OK");
    }

    private void OnAlreadyHaveAccount_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new Views.LoginPage());
    }
}