using TCC.Services;

namespace TCC.Views;

public partial class Index : ContentPage
{
    private readonly DatabaseService _databaseService;
    private string _currentUserType;
    private int _currentUserId;

    // Construtor com Injeção de Dependência
    public Index(DatabaseService databaseService)
    {
        InitializeComponent();
        _databaseService = databaseService;
    }

    // Construtor alternativo (para compatibilidade)
    public Index() : this(
        Application.Current?.Handler?.MauiContext?.Services.GetService<DatabaseService>()
        ?? new DatabaseService())
    {
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            // Inicializa o banco de dados
            await _databaseService.InitializeAsync();

            string? userType = await SecureStorage.GetAsync("user_type");
            string? userIdString = await SecureStorage.GetAsync("user_id");

            if (!int.TryParse(userIdString, out int userId))
            {
                await DisplayAlert("Erro", "Usuário inválido.", "OK");
                return;
            }

            _currentUserType = userType;
            _currentUserId = userId;

            await LoadUserData();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao inicializar: {ex.Message}", "OK");
        }
    }

    private async Task LoadUserData()
    {
        try
        {
            if (_currentUserType == "passenger")
            {
                await LoadPassengerData();
                SpecificSectionTitle.Text = "📚 Informações Acadêmicas";
            }
            else if (_currentUserType == "driver")
            {
                await LoadDriverData();
                SpecificSectionTitle.Text = "🚗 Informações Profissionais";
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao carregar dados do usuário: {ex.Message}", "OK");
        }
    }

    private async Task LoadPassengerData()
    {
        var passengers = await _databaseService.GetPassengers();
        var passenger = passengers.FirstOrDefault(p => p.Id == _currentUserId);

        if (passenger != null)
        {
            // Informações básicas
            WelcomeLabel.Text = passenger.Name?.ToUpper() ?? "USUÁRIO";
            RGLabel.Text = FormatRG(passenger.RG) ?? "Não informado";
            CPFLabel.Text = FormatCPF(passenger.CPF) ?? "Não informado";
            EmailLabel.Text = passenger.Email ?? "Não informado";
            AddressLabel.Text = passenger.Address ?? "Não informado";
            PhoneLabel.Text = FormatPhone(passenger.PhoneNumber) ?? "Não informado";
            GenderLabel.Text = passenger.Genre ?? "Não informado";

            // Campos específicos do passageiro
            SpecificField1Label.Text = "🏫 Escola";
            SpecificField1Value.Text = passenger.School ?? "Não informado";

            SpecificField2Label.Text = "👨‍👩‍👧‍👦 Responsável";
            SpecificField2Value.Text = passenger.ResponsableName ?? "Não informado";

            // Contato de emergência - layout completo para passageiros
            EmergencyContactFullLayout.IsVisible = true;
            EmergencyContactLabel.Text = FormatPhone(passenger.EmergencyPhoneNumber) ?? "Não informado";
            EmergencyContactCompactLayout.IsVisible = false;

            // Mostrar campo de atendimento especial
            SpecialTreatmentLayout.IsVisible = true;
            SpecialTreatmentLabel.Text = passenger.SpecialTreatment ? "Sim" : "Não";
        }
    }

    private async Task LoadDriverData()
    {
        var drivers = await _databaseService.GetDrivers();
        var driver = drivers.FirstOrDefault(d => d.Id == _currentUserId);

        if (driver != null)
        {
            // Informações básicas
            WelcomeLabel.Text = driver.Name?.ToUpper() ?? "USUÁRIO";
            RGLabel.Text = FormatRG(driver.RG) ?? "Não informado";
            CPFLabel.Text = FormatCPF(driver.CPF) ?? "Não informado";
            EmailLabel.Text = driver.Email ?? "Não informado";
            AddressLabel.Text = driver.Address ?? "Não informado";
            PhoneLabel.Text = FormatPhone(driver.PhoneNumber) ?? "Não informado";
            GenderLabel.Text = driver.Genre ?? "Não informado";

            // Campos específicos do motorista
            SpecificField1Label.Text = "🚗 CNH";
            SpecificField1Value.Text = driver.CNH ?? "Não informado";

            // Contato de emergência - layout compacto à direita para motoristas
            EmergencyContactFullLayout.IsVisible = false;
            EmergencyContactCompactLayout.IsVisible = true;
            EmergencyContactCompactLabel.Text = FormatPhone(driver.EmergencyPhoneNumber) ?? "Não informado";

            // Ocultar campo 2 e PCD para motoristas
            SpecificField2Layout.IsVisible = false;
            SpecialTreatmentLayout.IsVisible = false;
        }
    }

    // Métodos auxiliares para formatação
    private string FormatCPF(string? cpf)
    {
        if (string.IsNullOrEmpty(cpf) || cpf.Length != 11)
            return cpf;

        return $"{cpf.Substring(0, 3)}.{cpf.Substring(3, 3)}.{cpf.Substring(6, 3)}-{cpf.Substring(9, 2)}";
    }

    private string FormatRG(string? rg)
    {
        if (string.IsNullOrEmpty(rg))
            return rg;

        // Remove caracteres não numéricos
        string numbers = new string(rg.Where(char.IsDigit).ToArray());

        if (numbers.Length >= 9)
        {
            return $"{numbers.Substring(0, 2)}.{numbers.Substring(2, 3)}.{numbers.Substring(5, 3)}-{numbers.Substring(8, 1)}";
        }

        return rg;
    }

    private string FormatPhone(string? phone)
    {
        if (string.IsNullOrEmpty(phone))
            return phone;

        // Remove caracteres não numéricos
        string numbers = new string(phone.Where(char.IsDigit).ToArray());

        if (numbers.Length == 11)
        {
            return $"({numbers.Substring(0, 2)}) {numbers.Substring(2, 5)}-{numbers.Substring(7, 4)}";
        }
        else if (numbers.Length == 10)
        {
            return $"({numbers.Substring(0, 2)}) {numbers.Substring(2, 4)}-{numbers.Substring(6, 4)}";
        }

        return phone;
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Sair", "Deseja realmente sair da sua conta?", "Sim", "Não");

        if (confirm)
        {
            try
            {
                // Remove dados da sessão
                SecureStorage.Remove("user_id");
                SecureStorage.Remove("user_type");

                // Animação de saída suave
                await this.FadeTo(0, 300);

                // Redireciona para tela inicial (limpando a pilha de navegação)
                Application.Current.MainPage = new NavigationPage(new Home());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao sair: {ex.Message}", "OK");
            }
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        try
        {
            if (Navigation.NavigationStack.Count > 1)
                await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao voltar: {ex.Message}", "OK");
        }
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        try
        {
            if (_currentUserType == "passenger")
            {
                await Navigation.PushAsync(new PassengerEditPage(_currentUserId));
            }
            else if (_currentUserType == "driver")
            {
                await Navigation.PushAsync(new DriverEditPage(_currentUserId));
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao abrir página de edição: {ex.Message}", "OK");
        }
    }
}