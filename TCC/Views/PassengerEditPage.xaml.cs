using TCC.Models;
using TCC.Services;

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
                // Preenche os campos b�sicos
                NameEntry.Text = _currentPassenger.Name ?? string.Empty;
                RGEntry.Text = _currentPassenger.RG ?? string.Empty;
                CPFEntry.Text = _currentPassenger.CPF ?? string.Empty;
                EmailEntry.Text = _currentPassenger.Email ?? string.Empty;
                BirthDatePicker.Date = _currentPassenger.BirthDate;
                PhoneEntry.Text = _currentPassenger.PhoneNumber ?? string.Empty;
                EmergencyPhoneEntry.Text = _currentPassenger.EmergencyPhoneNumber ?? string.Empty;
                AddressEntry.Text = _currentPassenger.Address ?? string.Empty;
                ResponsibleEntry.Text = _currentPassenger.ResponsableName ?? string.Empty;
                BackupAddressEntry.Text = _currentPassenger.ReservableAddress ?? string.Empty;

                // Seleciona o g�nero no Picker
                SetPickerSelection(GenderPicker, _currentPassenger.Genre);

                // Seleciona a escola no Picker
                SetPickerSelection(SchoolPicker, _currentPassenger.School);

                // Configura o atendimento especial
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
                }
            }
            else
            {
                await DisplayAlert("Erro", "Passageiro n�o encontrado.", "OK");
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

        // Limpa o campo de observa��es se "N�o" for selecionado
        if (!AtendimentoSimRadio.IsChecked)
        {
            SpecialTreatmentEditor.Text = string.Empty;
        }
    }

    private async void OnSave_Clicked(object sender, EventArgs e)
    {
        try
        {
            // Valida��o de campos obrigat�rios
            var fieldsValidation = ValidateFields();

            if (!fieldsValidation.IsValid)
            {
                await DisplayAlert("Campos Obrigat�rios", fieldsValidation.Message, "OK");
                return;
            }

            // Valida��o de senha (se alterada)
            if (!ValidatePasswordChange())
            {
                await DisplayAlert("Erro", "As novas senhas n�o coincidem ou s�o muito curtas (m�nimo 6 caracteres).", "OK");
                return;
            }

            // Verifica se algum dado foi alterado e se j� est� em uso por outro usu�rio
            var validationResult = await _databaseService.ValidateUniqueUserData(
                rg: RGEntry.Text.Trim(),
                cpf: CPFEntry.Text.Trim(),
                email: EmailEntry.Text.Trim(),
                phone: PhoneEntry.Text.Trim(),
                excludeUserId: _passengerId,
                userType: "passenger"
            );

            if (!validationResult.IsValid)
            {
                await DisplayAlert("Dados j� cadastrados", validationResult.Message, "OK");
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
            await DisplayAlert("Erro", $"Erro ao salvar altera��es: {ex.Message}", "OK");
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
            (EmergencyPhoneEntry.Text, "Contato de Emerg�ncia"),
            (AddressEntry.Text, "Endere�o"),
            (ResponsibleEntry.Text, "Nome do Respons�vel"),
            (BackupAddressEntry.Text, "Endere�o Alternativo")
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
            return (false, "Por favor, selecione o g�nero.");
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

        // Se n�o est� tentando alterar a senha, valida��o OK
        if (string.IsNullOrEmpty(newPassword) && string.IsNullOrEmpty(confirmPassword))
        {
            return true;
        }

        // Se est� alterando, deve preencher ambos campos e eles devem coincidir
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
        _currentPassenger.RG = RGEntry.Text.Trim();
        _currentPassenger.CPF = CPFEntry.Text.Trim();
        _currentPassenger.Email = EmailEntry.Text.Trim();
        _currentPassenger.PhoneNumber = PhoneEntry.Text.Trim();
        _currentPassenger.EmergencyPhoneNumber = EmergencyPhoneEntry.Text.Trim();
        _currentPassenger.Address = AddressEntry.Text.Trim();
        _currentPassenger.ReservableAddress = BackupAddressEntry.Text.Trim();
        _currentPassenger.Genre = GenderPicker.SelectedItem?.ToString() ?? "N�o especificado";
        _currentPassenger.School = SchoolPicker.SelectedItem?.ToString() ?? "N�o especificado";
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
        bool confirm = await DisplayAlert("Cancelar", "Deseja descartar as altera��es?", "Sim", "N�o");

        if (confirm)
        {
            await Navigation.PopAsync();
        }
    }

    private void OnPointerEntered(object sender, PointerEventArgs e)
    {
        // A��o quando o mouse entra no bot�o
        ((Button)sender).BackgroundColor = Colors.DarkRed; // Muda a cor do bot�o
    }

    private void OnPointerExited(object sender, PointerEventArgs e)
    {
        // A��o quando o mouse sai do bot�o
        ((Button)sender).BackgroundColor = Colors.Red; // Volta � cor original
    }

    private void OnPointer2Entered(object sender, PointerEventArgs e)
    {
        // A��o quando o mouse entra no bot�o
        ((Button)sender).TextColor = Colors.DarkRed; // Muda a cor do bot�o
    }

    private void OnPointer2Exited(object sender, PointerEventArgs e)
    {
        // A��o quando o mouse sai do bot�o
        ((Button)sender).TextColor = Colors.Red; // Volta � cor original
    }
}