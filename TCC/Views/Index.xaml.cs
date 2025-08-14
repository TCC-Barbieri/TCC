using Microsoft.Maui.Controls;
using TCC.Models;
using TCC.Services;

namespace TCC.Views;

public partial class Index : ContentPage
{
    private readonly DatabaseService _databaseService;
    private Driver _currentDriver;
    private Passenger _currentPassenger;
    private string _userType;

    public Index()
    {
        InitializeComponent();
        _databaseService = new DatabaseService();
    }

    public Index(Driver driver) : this()
    {
        _currentDriver = driver;
        _userType = "driver";
        LoadUserData();
    }

    public Index(Passenger passenger) : this()
    {
        _currentPassenger = passenger;
        _userType = "passenger";
        LoadUserData();
    }

    private void LoadUserData()
    {
        if (_userType == "driver" && _currentDriver != null)
        {
            LoadDriverData();
            // Mostrar bot�o iniciar viagem para motoristas
            IniciarViagemButton.IsVisible = true;
        }
        else if (_userType == "passenger" && _currentPassenger != null)
        {
            LoadPassengerData();
            // Ocultar bot�o iniciar viagem para passageiros
            IniciarViagemButton.IsVisible = false;
        }
    }

    private void LoadDriverData()
    {
        WelcomeLabel.Text = _currentDriver.Name;
        RGLabel.Text = _currentDriver.RG;
        CPFLabel.Text = _currentDriver.CPF;
        EmailLabel.Text = _currentDriver.Email;
        AddressLabel.Text = _currentDriver.Address;
        PhoneLabel.Text = _currentDriver.PhoneNumber;
        EmergencyContactLabel.Text = _currentDriver.EmergencyPhoneNumber;
        GenderLabel.Text = _currentDriver.Genre;

        // Campos espec�ficos para motorista
        SpecificField1Label.Text = "CNH";
        SpecificField1Value.Text = _currentDriver.CNH;
        SpecificField2Label.Text = "Data de Nascimento";
        SpecificField2Value.Text = _currentDriver.BirthDate.ToString("dd/MM/yyyy");

        // Ocultar campo de atendimento especial (espec�fico de passageiro)
        SpecialTreatmentLayout.IsVisible = false;
    }

    private void LoadPassengerData()
    {
        WelcomeLabel.Text = _currentPassenger.Name;
        RGLabel.Text = _currentPassenger.RG;
        CPFLabel.Text = _currentPassenger.CPF;
        EmailLabel.Text = _currentPassenger.Email;
        AddressLabel.Text = _currentPassenger.Address;
        PhoneLabel.Text = _currentPassenger.PhoneNumber;
        EmergencyContactLabel.Text = _currentPassenger.EmergencyPhoneNumber;
        GenderLabel.Text = _currentPassenger.Genre;

        // Campos espec�ficos para passageiro
        SpecificField1Label.Text = "Escola";
        SpecificField1Value.Text = _currentPassenger.School;
        SpecificField2Label.Text = "Respons�vel";
        SpecificField2Value.Text = _currentPassenger.ResponsableName;

        // Mostrar campo de atendimento especial
        SpecialTreatmentLayout.IsVisible = true;
        SpecialTreatmentLabel.Text = _currentPassenger.SpecialTreatment ? "Sim" : "N�o";
    }

    private async void OnIniciarViagemClicked(object sender, EventArgs e)
    {
        // Verificar se � motorista antes de permitir iniciar viagem
        if (_userType != "driver")
        {
            await DisplayAlert("Acesso Negado", "Apenas motoristas podem iniciar viagens.", "OK");
            return;
        }

        try
        {
            await Navigation.PushAsync(new ViagemPage());
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao iniciar viagem: {ex.Message}", "OK");
        }
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        try
        {
            // Por enquanto, vamos mostrar uma mensagem informativa
            // Voc� pode implementar as p�ginas de edi��o posteriormente
            if (_userType == "driver")
            {
                await DisplayAlert("Informa��o", "Funcionalidade de edi��o de motorista ser� implementada em breve.", "OK");
                // Quando implementar a p�gina de edi��o, descomente a linha abaixo:
                // await Navigation.PushAsync(new DriverEditPage(_currentDriver));
            }
            else
            {
                await DisplayAlert("Informa��o", "Funcionalidade de edi��o de passageiro ser� implementada em breve.", "OK");
                // Quando implementar a p�gina de edi��o, descomente a linha abaixo:
                // await Navigation.PushAsync(new PassengerEditPage(_currentPassenger));
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao abrir edi��o: {ex.Message}", "OK");
        }
    }

    private async void OnGroupViewClicked(object sender, EventArgs e)
    {
        try
        {
            // Por enquanto, vamos mostrar uma mensagem informativa
            // Voc� pode implementar as p�ginas de listagem posteriormente
            if (_userType == "driver")
            {
                await DisplayAlert("Informa��o", "Lista de motoristas ser� implementada em breve.", "OK");
                // Quando implementar a p�gina de lista, descomente a linha abaixo:
                // await Navigation.PushAsync(new DriverListPage());
            }
            else
            {
                await DisplayAlert("Informa��o", "Lista de passageiros ser� implementada em breve.", "OK");
                // Quando implementar a p�gina de lista, descomente a linha abaixo:
                // await Navigation.PushAsync(new PassengerListPage());
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao abrir lista: {ex.Message}", "OK");
        }
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Confirmar", "Deseja realmente sair?", "Sim", "N�o");
        if (confirm)
        {
            // Limpar dados do usu�rio
            _currentDriver = null;
            _currentPassenger = null;
            _userType = null;

            // Voltar para a p�gina inicial
            await Navigation.PopToRootAsync();
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    // M�todos para efeitos visuais dos bot�es
    private void OnPointerEntered(object sender, PointerEventArgs e)
    {
        if (sender is Button button)
        {
            button.BackgroundColor = Color.FromArgb("#CC0000");
        }
    }

    private void OnPointerExited(object sender, PointerEventArgs e)
    {
        if (sender is Button button)
        {
            button.BackgroundColor = Color.FromArgb("#FF0000");
        }
    }

    private void OnPointer2Entered(object sender, PointerEventArgs e)
    {
        if (sender is Frame frame)
        {
            frame.BackgroundColor = Color.FromArgb("#CC0000");
        }
    }

    private void OnPointer2Exited(object sender, PointerEventArgs e)
    {
        if (sender is Frame frame)
        {
            frame.BackgroundColor = Color.FromArgb("#FF0000");
        }
    }
}