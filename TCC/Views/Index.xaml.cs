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
            // Mostrar botão iniciar viagem para motoristas
            IniciarViagemButton.IsVisible = true;
        }
        else if (_userType == "passenger" && _currentPassenger != null)
        {
            LoadPassengerData();
            // Ocultar botão iniciar viagem para passageiros
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

        // Campos específicos para motorista
        SpecificField1Label.Text = "CNH";
        SpecificField1Value.Text = _currentDriver.CNH;
        SpecificField2Label.Text = "Data de Nascimento";
        SpecificField2Value.Text = _currentDriver.BirthDate.ToString("dd/MM/yyyy");

        // Ocultar campo de atendimento especial (específico de passageiro)
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

        // Campos específicos para passageiro
        SpecificField1Label.Text = "Escola";
        SpecificField1Value.Text = _currentPassenger.School;
        SpecificField2Label.Text = "Responsável";
        SpecificField2Value.Text = _currentPassenger.ResponsableName;

        // Mostrar campo de atendimento especial
        SpecialTreatmentLayout.IsVisible = true;
        SpecialTreatmentLabel.Text = _currentPassenger.SpecialTreatment ? "Sim" : "Não";
    }

    private async void OnIniciarViagemClicked(object sender, EventArgs e)
    {
        // Verificar se é motorista antes de permitir iniciar viagem
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
            // Você pode implementar as páginas de edição posteriormente
            if (_userType == "driver")
            {
                await DisplayAlert("Informação", "Funcionalidade de edição de motorista será implementada em breve.", "OK");
                // Quando implementar a página de edição, descomente a linha abaixo:
                // await Navigation.PushAsync(new DriverEditPage(_currentDriver));
            }
            else
            {
                await DisplayAlert("Informação", "Funcionalidade de edição de passageiro será implementada em breve.", "OK");
                // Quando implementar a página de edição, descomente a linha abaixo:
                // await Navigation.PushAsync(new PassengerEditPage(_currentPassenger));
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao abrir edição: {ex.Message}", "OK");
        }
    }

    private async void OnGroupViewClicked(object sender, EventArgs e)
    {
        try
        {
            // Por enquanto, vamos mostrar uma mensagem informativa
            // Você pode implementar as páginas de listagem posteriormente
            if (_userType == "driver")
            {
                await DisplayAlert("Informação", "Lista de motoristas será implementada em breve.", "OK");
                // Quando implementar a página de lista, descomente a linha abaixo:
                // await Navigation.PushAsync(new DriverListPage());
            }
            else
            {
                await DisplayAlert("Informação", "Lista de passageiros será implementada em breve.", "OK");
                // Quando implementar a página de lista, descomente a linha abaixo:
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
        bool confirm = await DisplayAlert("Confirmar", "Deseja realmente sair?", "Sim", "Não");
        if (confirm)
        {
            // Limpar dados do usuário
            _currentDriver = null;
            _currentPassenger = null;
            _userType = null;

            // Voltar para a página inicial
            await Navigation.PopToRootAsync();
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    // Métodos para efeitos visuais dos botões
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