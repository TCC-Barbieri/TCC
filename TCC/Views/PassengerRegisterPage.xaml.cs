using TCC.Models;
using TCC.Services;

namespace TCC.Views;

public partial class PassengerRegisterPage : ContentPage
{
    private readonly DatabaseService _databaseService = new();

    public PassengerRegisterPage()
    {
        InitializeComponent();
    }

    // Mostrar detalhes se "Sim" for selecionado
    private void OnSimTapped(object sender, EventArgs e)
    {
        AtendimentoSimRadio.IsChecked = true;
    }

    // Ocultar detalhes se "N�o" for selecionado
    private void OnNaoTapped(object sender, EventArgs e)
    {
        AtendimentoNaoRadio.IsChecked = true;
    }

    // Controla a visibilidade dos detalhes do atendimento especial
    private void OnAtendimentoEspecialChanged(object sender, CheckedChangedEventArgs e)
    {
        SpecialTreatmentDetailsLayout.IsVisible = AtendimentoSimRadio.IsChecked;

        // Limpa o campo se "N�o" for selecionado
        if (!AtendimentoSimRadio.IsChecked)
        {
            SpecialTreatmentEditor.Text = string.Empty;
        }
    }

    // Evento do bot�o registrar
    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        try
        {
            // Verifica��o dos campos obrigat�rios
            var validationResult = ValidateRequiredFields();
            if (!validationResult.IsValid)
            {
                await DisplayAlert("Campos obrigat�rios", validationResult.Message, "OK");
                return;
            }

            // Verifica��o de senhas
            if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
            {
                await DisplayAlert("Erro", "As senhas n�o coincidem.", "OK");
                return;
            }

            if (PasswordEntry.Text.Length < 6)
            {
                await DisplayAlert("Erro", "A senha deve ter pelo menos 6 caracteres.", "OK");
                return;
            }

            // Valida��o de dados �nicos (RG, CPF, Email, Telefone)
            var uniqueDataValidation = await _databaseService.ValidateUniqueUserData(
                rg: RGEntry.Text.Trim(),
                cpf: CPFEntry.Text.Trim(),
                email: EmailEntry.Text.Trim(),
                phone: PhoneEntry.Text.Trim()
            );

            if (!uniqueDataValidation.IsValid)
            {
                await DisplayAlert("Dados j� cadastrados", uniqueDataValidation.Message, "OK");
                return;
            }

            // Cria o objeto passageiro
            Passenger passenger = new Passenger
            {
                Name = NameEntry.Text.Trim(),
                Password = PasswordEntry.Text,
                Email = EmailEntry.Text.Trim(),
                PhoneNumber = PhoneEntry.Text.Trim(),
                EmergencyPhoneNumber = EmergencyPhoneEntry.Text.Trim(),
                Address = AddressEntry.Text.Trim(),
                ReservableAddress = BackupAddressEntry.Text.Trim(),
                RG = RGEntry.Text.Trim(),
                CPF = CPFEntry.Text.Trim(),
                Genre = GenderPicker.SelectedItem?.ToString() ?? "N�o especificado",
                School = SchoolPicker.SelectedItem?.ToString() ?? "N�o especificado",
                ResponsableName = ResponsibleEntry.Text.Trim(),
                SpecialTreatment = AtendimentoSimRadio.IsChecked,
                SpecialTreatmentObservations = AtendimentoSimRadio.IsChecked
                    ? SpecialTreatmentEditor.Text?.Trim() ?? string.Empty
                    : string.Empty,
                BirthDate = BirthDatePicker.Date
            };

            // Registra o passageiro
            await _databaseService.CreatePassenger(passenger);

            await DisplayAlert("Sucesso", "Passageiro registrado com sucesso!", "OK");

            // Limpa os campos ap�s registro bem-sucedido
            ClearFields();

            // Opcional: Navegar para p�gina de login
            // await Navigation.PushAsync(new Views.LoginPage());
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
            (ConfirmPasswordEntry.Text, "Confirma��o de Senha"),
            (EmailEntry.Text, "Email"),
            (PhoneEntry.Text, "Telefone"),
            (EmergencyPhoneEntry.Text, "Contato de Emerg�ncia"),
            (AddressEntry.Text, "Endere�o"),
            (BackupAddressEntry.Text, "Endere�o Alternativo"),
            (RGEntry.Text, "RG"),
            (CPFEntry.Text, "CPF"),
            (ResponsibleEntry.Text, "Nome do Respons�vel")
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
        BirthDatePicker.Date = DateTime.Today;

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