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

    // Ocultar detalhes se "Não" for selecionado
    private void OnNaoTapped(object sender, EventArgs e)
    {
        AtendimentoNaoRadio.IsChecked = true;
    }

    // Controla a visibilidade dos detalhes do atendimento especial
    private void OnAtendimentoEspecialChanged(object sender, CheckedChangedEventArgs e)
    {
        SpecialTreatmentDetailsLayout.IsVisible = AtendimentoSimRadio.IsChecked;
    }

    // Evento do botão registrar
    private async void OnRegistrarClicked(object sender, EventArgs e)
    {
        try
        {
            // Verificação dos campos obrigatórios
            if (string.IsNullOrWhiteSpace(NameEntry.Text) ||
                string.IsNullOrWhiteSpace(PasswordEntry.Text) ||
                string.IsNullOrWhiteSpace(EmailEntry.Text) ||
                string.IsNullOrWhiteSpace(PhoneEntry.Text) ||
                string.IsNullOrWhiteSpace(EmergencyPhoneEntry.Text) ||
                string.IsNullOrWhiteSpace(AddressEntry.Text) ||
                string.IsNullOrWhiteSpace(BackupAddressEntry.Text) ||
                string.IsNullOrWhiteSpace(RGEntry.Text) ||
                string.IsNullOrWhiteSpace(CPFEntry.Text) ||
                GenderPicker.SelectedItem == null ||
                SchoolPicker.SelectedItem == null ||
                string.IsNullOrWhiteSpace(ResponsibleEntry.Text) ||
                (AtendimentoSimRadio.IsChecked && string.IsNullOrWhiteSpace(SpecialTreatmentEditor.Text)))
            {
                await DisplayAlert("Campos obrigatórios", "Por favor, preencha todos os campos antes de registrar.", "OK");
                return;
            }

            Passenger passenger = new Passenger
            {
                Name = NameEntry.Text,
                Password = PasswordEntry.Text,
                Email = EmailEntry.Text,
                PhoneNumber = PhoneEntry.Text,
                EmergencyPhoneNumber = EmergencyPhoneEntry.Text,
                Address = AddressEntry.Text,
                ReservableAddress = BackupAddressEntry.Text,
                RG = RGEntry.Text,
                CPF = CPFEntry.Text,
                Genre = GenderPicker.SelectedItem.ToString(),
                School = SchoolPicker.SelectedItem.ToString(),
                ResponsableName = ResponsibleEntry.Text,
                SpecialTreatment = AtendimentoSimRadio.IsChecked,
                SpecialTreatmentObservations = SpecialTreatmentEditor.Text,
                BirthDate = BirthDatePicker.Date
            };

            if (await _databaseService.IsEmailTaken(passenger.Email))
            {
                await DisplayAlert("Erro", "Este e-mail já está em uso.", "OK");
                return;
            }

            await _databaseService.CreatePassenger(passenger);
            await DisplayAlert("Sucesso", "Passageiro registrado com sucesso!", "OK");
            // Aqui você pode limpar os campos ou navegar para outra página
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao registrar passageiro: {ex.Message}", "OK");
        }
    }


    // Evento do botão "Já tenho uma conta"
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        string dbPath = FileSystem.AppDataDirectory;
        await DisplayAlert("Database Path", dbPath, "OK");
    }
}