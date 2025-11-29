using TCC.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;

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
        LoadSavedProfilePhoto();

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

    private async void OnSelectPhotoClicked(object sender, EventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🔵 Botão de foto clicado!");

            // Pergunta ao usuário se deseja tirar foto ou escolher da galeria
            var action = await DisplayActionSheet(
                "Foto de Perfil",
                "Cancelar",
                null,
                "📷 Tirar Foto",
                "🖼️ Escolher da Galeria"
            );

            System.Diagnostics.Debug.WriteLine($"🔵 Ação selecionada: {action}");

            if (action == "Cancelar" || string.IsNullOrEmpty(action))
            {
                return;
            }

            FileResult photo = null;

            if (action == "📷 Tirar Foto")
            {
                photo = await TakePhotoAsync();
            }
            else if (action == "🖼️ Escolher da Galeria")
            {
                photo = await PickPhotoAsync();
            }

            if (photo != null)
            {
                await LoadPhotoToProfile(photo);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"🔴 ERRO: {ex.Message}");
            await DisplayAlert("Erro", $"Erro ao selecionar foto: {ex.Message}", "OK");
        }
    }

    private async Task<FileResult> TakePhotoAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🔵 Verificando permissão da câmera...");

            // Verifica se o dispositivo suporta captura de foto
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                await DisplayAlert(
                    "Não Suportado",
                    "Este dispositivo não suporta captura de fotos.",
                    "OK"
                );
                return null;
            }

            // Verifica o status atual da permissão
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            System.Diagnostics.Debug.WriteLine($"🔵 Status inicial da câmera: {status}");

            // Se não tiver permissão, solicita
            if (status != PermissionStatus.Granted)
            {
                // Verifica se deve mostrar explicação
                if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    await DisplayAlert(
                        "Permissão Necessária",
                        "Para tirar fotos, você precisa permitir o acesso à câmera nas configurações do dispositivo.",
                        "OK"
                    );
                    return null;
                }

                // Mostra uma mensagem explicativa antes de pedir permissão
                bool shouldRequest = await DisplayAlert(
                    "Permissão de Câmera",
                    "Este app precisa acessar sua câmera para tirar fotos de perfil. Permitir?",
                    "Sim",
                    "Não"
                );

                if (!shouldRequest)
                {
                    return null;
                }

                // Solicita a permissão
                status = await Permissions.RequestAsync<Permissions.Camera>();
                System.Diagnostics.Debug.WriteLine($"🔵 Status após solicitar: {status}");
            }

            // Verifica se a permissão foi concedida
            if (status == PermissionStatus.Granted)
            {
                System.Diagnostics.Debug.WriteLine("🔵 Permissão concedida! Abrindo câmera...");

                var photo = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
                {
                    Title = "Tire uma foto para o perfil"
                });

                return photo;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"🔴 Permissão negada: {status}");

                await DisplayAlert(
                    "Permissão Negada",
                    "Não é possível tirar fotos sem permissão de acesso à câmera.",
                    "OK"
                );

                return null;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"🔴 Erro ao tirar foto: {ex.Message}");
            await DisplayAlert("Erro", $"Erro ao acessar câmera: {ex.Message}", "OK");
            return null;
        }
    }

    private async Task<FileResult> PickPhotoAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🔵 Verificando permissão de fotos...");

            // Verifica o status atual da permissão
            var status = await Permissions.CheckStatusAsync<Permissions.Photos>();
            System.Diagnostics.Debug.WriteLine($"🔵 Status inicial de fotos: {status}");

            // Se não tiver permissão, solicita
            if (status != PermissionStatus.Granted)
            {
                // Verifica se deve mostrar explicação (iOS)
                if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    await DisplayAlert(
                        "Permissão Necessária",
                        "Para escolher fotos, você precisa permitir o acesso à galeria nas configurações do dispositivo.",
                        "OK"
                    );
                    return null;
                }

                // Mostra uma mensagem explicativa antes de pedir permissão
                bool shouldRequest = await DisplayAlert(
                    "Permissão de Galeria",
                    "Este app precisa acessar suas fotos para selecionar uma foto de perfil. Permitir?",
                    "Sim",
                    "Não"
                );

                if (!shouldRequest)
                {
                    return null;
                }

                // Solicita a permissão
                status = await Permissions.RequestAsync<Permissions.Photos>();
                System.Diagnostics.Debug.WriteLine($"🔵 Status após solicitar: {status}");
            }

            // Verifica se a permissão foi concedida
            if (status == PermissionStatus.Granted)
            {
                System.Diagnostics.Debug.WriteLine("🔵 Permissão concedida! Abrindo galeria...");

                var photo = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Selecione uma foto para o perfil"
                });

                return photo;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"🔴 Permissão negada: {status}");

                await DisplayAlert(
                    "Permissão Negada",
                    "Não é possível escolher fotos sem permissão de acesso à galeria.",
                    "OK"
                );

                return null;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"🔴 Erro ao escolher foto: {ex.Message}");
            await DisplayAlert("Erro", $"Erro ao acessar galeria: {ex.Message}", "OK");
            return null;
        }
    }

    private async Task LoadPhotoToProfile(FileResult photo)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"🔵 Carregando foto: {photo.FileName}");

            // Carrega a imagem selecionada
            var stream = await photo.OpenReadAsync();

            // Cria uma cópia do stream para evitar problemas de acesso
            var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            ProfileImage.Source = ImageSource.FromStream(() =>
            {
                memoryStream.Position = 0;
                return memoryStream;
            });

            // Oculta o template padrão e mostra a imagem
            DefaultProfileTemplate.IsVisible = false;
            ProfileImage.IsVisible = true;

            System.Diagnostics.Debug.WriteLine("🔵 Foto carregada com sucesso!");

            // Salva a foto (opcional)
            await SaveProfilePhoto(photo);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"🔴 Erro ao carregar foto: {ex.Message}");
            await DisplayAlert("Erro", $"Erro ao carregar foto: {ex.Message}", "OK");
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

    // Método para carregar foto salva do usuário
    private async Task LoadProfilePhoto()
    {
        try
        {
            // Carregar foto do banco de dados ou preferências
            // string photoPath = Preferences.Get("profile_photo", string.Empty);

            // if (!string.IsNullOrEmpty(photoPath) && File.Exists(photoPath))
            // {
            //     ProfileImage.Source = ImageSource.FromFile(photoPath);
            //     DefaultProfileTemplate.IsVisible = false;
            //     ProfileImage.IsVisible = true;
            // }
        }
        catch (Exception ex)
        {
            // Log do erro
        }
    }

    // Método para salvar foto no sistema
    private async Task SaveProfilePhoto(FileResult photo)
    {
        try
        {
            if (photo == null) return;

            System.Diagnostics.Debug.WriteLine("🔵 Salvando foto...");

            // Copiar foto para o diretório da aplicação
            var appDataPath = FileSystem.AppDataDirectory;
            var fileName = "profile_photo.jpg";
            var destPath = Path.Combine(appDataPath, fileName);

            // Abrir stream da foto selecionada
            using var sourceStream = await photo.OpenReadAsync();

            // Salvar no diretório local
            using var fileStream = File.Create(destPath);
            await sourceStream.CopyToAsync(fileStream);

            // Salvar caminho nas preferências
            Preferences.Set("profile_photo_path", destPath);

            System.Diagnostics.Debug.WriteLine($"🔵 Foto salva em: {destPath}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"🔴 Erro ao salvar foto: {ex.Message}");
            // Não mostra alerta para o usuário, apenas loga
        }
    }

    private void LoadSavedProfilePhoto()
    {
        try
        {
            // Carregar foto do banco de dados ou preferências
            string photoPath = Preferences.Get("profile_photo_path", string.Empty);

            System.Diagnostics.Debug.WriteLine($"🔵 Tentando carregar foto de: {photoPath}");

            if (!string.IsNullOrEmpty(photoPath) && File.Exists(photoPath))
            {
                ProfileImage.Source = ImageSource.FromFile(photoPath);
                DefaultProfileTemplate.IsVisible = false;
                ProfileImage.IsVisible = true;

                System.Diagnostics.Debug.WriteLine("🔵 Foto de perfil carregada com sucesso!");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("🔵 Nenhuma foto salva encontrada");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"🔴 Erro ao carregar foto salva: {ex.Message}");
        }
    }
}