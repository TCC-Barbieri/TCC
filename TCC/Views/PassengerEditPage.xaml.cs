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

        // Define a data máxima do DatePicker (14 anos atrás) — evita datas futuras e usuários menores que 14 anos.
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
            _currentPassenger = passengers.FirstOrDefault(p => p.Id == _passengerId);

            if (_currentPassenger != null)
            {
                // Preenche os campos básicos
                NameEntry.Text = _currentPassenger.Name ?? string.Empty;
                RGEntry.Text = _currentPassenger.RG ?? string.Empty; // mantemos a string como veio do banco
                CPFEntry.Text = _currentPassenger.CPF ?? string.Empty;
                EmailEntry.Text = _currentPassenger.Email ?? string.Empty;
                BirthDatePicker.Date = _currentPassenger.BirthDate;

                // Formata os telefones para exibição (se existir)
                PhoneEntry.Text = !string.IsNullOrWhiteSpace(_currentPassenger.PhoneNumber)
                    ? PhoneValidationHelper.FormatPhone(_currentPassenger.PhoneNumber)
                    : string.Empty;

                EmergencyPhoneEntry.Text = !string.IsNullOrWhiteSpace(_currentPassenger.EmergencyPhoneNumber)
                    ? PhoneValidationHelper.FormatPhone(_currentPassenger.EmergencyPhoneNumber)
                    : string.Empty;

                AddressEntry.Text = _currentPassenger.Address ?? string.Empty;
                ResponsibleEntry.Text = _currentPassenger.ResponsableName ?? string.Empty;
                BackupAddressEntry.Text = _currentPassenger.ReservableAddress ?? string.Empty;

                // Seleciona pickers
                SetPickerSelection(GenderPicker, _currentPassenger.Genre);
                SetPickerSelection(SchoolPicker, _currentPassenger.School);

                // Atendimento especial
                if (_currentPassenger.SpecialTreatment)
                {
                    AtendimentoSimRadio.IsChecked = true;
                    SpecialTreatmentDetailsLayout.IsVisible = true;
                    SpecialTreatmentEditor.Text = _currentPassenger.SpecialTreatmentObservations ?? string.Empty;
                }
                else
                {
                    AtendimentoNaoRadio.IsChecked = true;
                    SpecialTreatmentDetailsLayout.IsVisible = false;
                    SpecialTreatmentEditor.Text = string.Empty;
                }
            }
            else
            {
                await DisplayAlert("Erro", "Passageiro não encontrado.", "OK");
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
        if (!string.IsNullOrEmpty(value))
        {
            var index = picker.Items.IndexOf(value);
            if (index >= 0)
                picker.SelectedIndex = index;
        }
    }

    private void OnAtendimentoEspecialChanged(object sender, CheckedChangedEventArgs e)
    {
        SpecialTreatmentDetailsLayout.IsVisible = AtendimentoSimRadio.IsChecked;

        // Limpa o campo de observações se "Não" for selecionado
        if (!AtendimentoSimRadio.IsChecked)
        {
            SpecialTreatmentEditor.Text = string.Empty;
        }
    }

    private async void OnSave_Clicked(object sender, EventArgs e)
    {
        try
        {
            // Validação dos campos obrigatórios
            var fieldsValidation = ValidateFields();

            if (!fieldsValidation.IsValid)
            {
                await DisplayAlert("Campos Obrigatórios", fieldsValidation.Message, "OK");
                return;
            }

            // Validar RG
            if (!RGValidatorHelper.IsValid(RGEntry.Text?.Trim()))
            {
                await DisplayAlert("Atenção", "RG inválido. Verifique os números digitados.", "OK");
                RGEntry.Focus();
                return;
            }

            // Validar CPF
            if (!CPFValidator.IsValid(CPFEntry.Text?.Trim()))
            {
                await DisplayAlert("Atenção", "CPF inválido. Verifique os números digitados.", "OK");
                CPFEntry.Focus();
                return;
            }

            // Validar idade mínima (14 anos)
            var age = DateTime.Today.Year - BirthDatePicker.Date.Year;
            if (BirthDatePicker.Date > DateTime.Today.AddYears(-age)) age--;

            if (age < 14)
            {
                await DisplayAlert("Atenção", "Passageiro deve ter no mínimo 14 anos.", "OK");
                return;
            }

            // Validar Telefone
            string phoneText = PhoneEntry.Text?.Trim();
            if (!PhoneValidationHelper.IsValidPhone(phoneText))
            {
                await DisplayAlert("Atenção", PhoneValidationHelper.GetValidationErrorMessage(), "OK");
                PhoneEntry.Focus();
                return;
            }

            // Validar Telefone de Emergência
            string emergencyPhoneText = EmergencyPhoneEntry.Text?.Trim();
            if (!PhoneValidationHelper.IsValidPhone(emergencyPhoneText))
            {
                await DisplayAlert("Atenção", "Contato de emergência inválido. " + PhoneValidationHelper.GetValidationErrorMessage(), "OK");
                EmergencyPhoneEntry.Focus();
                return;
            }

            // Validação de senha (se alterada)
            if (!ValidatePasswordChange())
            {
                await DisplayAlert("Erro", "As novas senhas não coincidem ou são muito curtas (mínimo 6 caracteres).", "OK");
                return;
            }

            // Verifica se algum dado foi alterado e se já está em uso por outro usuário
            var validationResult = await _databaseService.ValidateUniqueUserData(
                rg: RGValidatorHelper.RemoveFormat(RGEntry.Text.Trim()),
                cpf: CPFValidator.RemoveFormat(CPFEntry.Text.Trim()),
                email: EmailEntry.Text.Trim(),
                phone: PhoneValidationHelper.GetOnlyNumbers(PhoneEntry.Text.Trim()),
                excludeUserId: _passengerId,
                userType: "passenger"
            );

            if (!validationResult.IsValid)
            {
                await DisplayAlert("Dados já cadastrados", validationResult.Message, "OK");
                return;
            }

            // Atualiza os dados do passageiro
            UpdatePassengerData();

            // Salva no banco de dados
            await _databaseService.UpdatePassenger(_currentPassenger);

            await DisplayAlert("Sucesso", "Dados atualizados com sucesso!", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao salvar alterações: {ex.Message}", "OK");
        }
    }

    private (bool IsValid, string Message) ValidateFields()
    {
        var requiredFields = new[]
        {
            (NameEntry.Text, "Nome"),
            (RGEntry.Text, "RG"),
            (CPFEntry.Text, "CPF"),
            (EmailEntry.Text, "Email"),
            (PhoneEntry.Text, "Telefone"),
            (EmergencyPhoneEntry.Text, "Contato de Emergência"),
            (AddressEntry.Text, "Endereço"),
            (ResponsibleEntry.Text, "Nome do Responsável"),
            (BackupAddressEntry.Text, "Endereço Alternativo")
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

    private bool ValidatePasswordChange()
    {
        var newPassword = NewPasswordEntry.Text?.Trim();
        var confirmPassword = ConfirmNewPasswordEntry.Text?.Trim();

        // Se não está tentando alterar a senha, validação OK
        if (string.IsNullOrEmpty(newPassword) && string.IsNullOrEmpty(confirmPassword))
        {
            return true;
        }

        // Se está alterando, deve preencher ambos campos e eles devem coincidir
        if (newPassword != confirmPassword)
        {
            return false;
        }

        // Senha deve ter pelo menos 6 caracteres
        if (!string.IsNullOrEmpty(newPassword) && newPassword.Length < 6)
        {
            return false;
        }

        return true;
    }

    private void UpdatePassengerData()
    {
        _currentPassenger.Name = NameEntry.Text.Trim();
        _currentPassenger.RG = RGValidatorHelper.RemoveFormat(RGEntry.Text.Trim());
        _currentPassenger.CPF = CPFValidator.RemoveFormat(CPFEntry.Text.Trim());
        _currentPassenger.Email = EmailEntry.Text.Trim();
        _currentPassenger.PhoneNumber = PhoneValidationHelper.GetOnlyNumbers(PhoneEntry.Text.Trim());
        _currentPassenger.EmergencyPhoneNumber = PhoneValidationHelper.GetOnlyNumbers(EmergencyPhoneEntry.Text.Trim());
        _currentPassenger.Address = AddressEntry.Text.Trim();
        _currentPassenger.ReservableAddress = BackupAddressEntry.Text.Trim();
        _currentPassenger.Genre = GenderPicker.SelectedItem?.ToString() ?? "Não especificado";
        _currentPassenger.School = SchoolPicker.SelectedItem?.ToString() ?? "Não especificado";
        _currentPassenger.ResponsableName = ResponsibleEntry.Text.Trim();
        _currentPassenger.SpecialTreatment = AtendimentoSimRadio.IsChecked;
        _currentPassenger.SpecialTreatmentObservations = AtendimentoSimRadio.IsChecked
            ? SpecialTreatmentEditor.Text?.Trim() ?? string.Empty
            : string.Empty;
        _currentPassenger.BirthDate = BirthDatePicker.Date;

        // Atualiza a senha se foi alterada
        var newPassword = NewPasswordEntry.Text?.Trim();
        if (!string.IsNullOrEmpty(newPassword))
        {
            _currentPassenger.Password = newPassword;
        }
    }

    private async void OnCancel_Clicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Cancelar", "Deseja descartar as alterações?", "Sim", "Não");

        if (confirm)
        {
            await Navigation.PopAsync();
        }
    }
}
