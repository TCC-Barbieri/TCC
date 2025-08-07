using TCC.Models;
using TCC.Services;
using Microsoft.Maui.Storage;

namespace TCC.Views;

public partial class DriverEditPage : ContentPage
{
    private readonly DatabaseService _databaseService = new();
    private readonly int _driverId;
    private Driver _currentDriver;

    public DriverEditPage(int driverId)
    {
        InitializeComponent();
        _driverId = driverId;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDriverData();
    }

    private async Task LoadDriverData()
    {
        try
        {
            var drivers = await _databaseService.GetDrivers();
            _currentDriver = drivers.FirstOrDefault(d => d.Id == _driverId);

            if (_currentDriver != null)
            {
                // Preenche os campos com os dados atuais
                NameEntry.Text = _currentDriver.Name;
                RGEntry.Text = _currentDriver.RG;
                CPFEntry.Text = _currentDriver.CPF;
                EmailEntry.Text = _currentDriver.Email;
                BirthDatePicker.Date = _currentDriver.BirthDate;
                PhoneEntry.Text = _currentDriver.PhoneNumber;
                ContatoEmergenciaEntry.Text = _currentDriver.EmergencyPhoneNumber;
                CNHEntry.Text = _currentDriver.CNH;
                AddressEntry.Text = _currentDriver.Address;

                // Seleciona o gênero no Picker
                if (!string.IsNullOrEmpty(_currentDriver.Genre))
                {
                    var genderIndex = GenderPicker.Items.IndexOf(_currentDriver.Genre);
                    if (genderIndex >= 0)
                        GenderPicker.SelectedIndex = genderIndex;
                }
            }
            else
            {
                await DisplayAlert("Erro", "Motorista não encontrado.", "OK");
                await Navigation.PopAsync();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao carregar dados: {ex.Message}", "OK");
        }
    }

    private async void OnSave_Clicked(object sender, EventArgs e)
    {
        try
        {
            // Verificação de campos obrigatórios
            if (string.IsNullOrWhiteSpace(NameEntry.Text) ||
                string.IsNullOrWhiteSpace(RGEntry.Text) ||
                string.IsNullOrWhiteSpace(CPFEntry.Text) ||
                string.IsNullOrWhiteSpace(EmailEntry.Text) ||
                string.IsNullOrWhiteSpace(PhoneEntry.Text) ||
                string.IsNullOrWhiteSpace(ContatoEmergenciaEntry.Text) ||
                string.IsNullOrWhiteSpace(CNHEntry.Text) ||
                string.IsNullOrWhiteSpace(AddressEntry.Text))
            {
                await DisplayAlert("Campos obrigatórios", "Por favor, preencha todos os campos.", "OK");
                return;
            }

            // Validação de senha (se foi alterada)
            if (!string.IsNullOrWhiteSpace(NewPasswordEntry.Text))
            {
                if (NewPasswordEntry.Text != ConfirmNewPasswordEntry.Text)
                {
                    await DisplayAlert("Erro", "As novas senhas não coincidem.", "OK");
                    return;
                }

                if (NewPasswordEntry.Text.Length < 6)
                {
                    await DisplayAlert("Erro", "A nova senha deve ter pelo menos 6 caracteres.", "OK");
                    return;
                }
            }

            // Verifica se algum dado foi alterado e se já está em uso por outro usuário
            var validationResult = await _databaseService.ValidateUniqueUserData(
                rg: RGEntry.Text.Trim(),
                cpf: CPFEntry.Text.Trim(),
                email: EmailEntry.Text.Trim(),
                phone: PhoneEntry.Text.Trim(),
                cnh: CNHEntry.Text.Trim(),
                excludeUserId: _driverId,
                userType: "driver"
            );

            if (!validationResult.IsValid)
            {
                await DisplayAlert("Dados já cadastrados", validationResult.Message, "OK");
                return;
            }

            // Atualiza os dados do motorista
            _currentDriver.Name = NameEntry.Text;
            _currentDriver.RG = RGEntry.Text;
            _currentDriver.CPF = CPFEntry.Text;
            _currentDriver.Email = EmailEntry.Text;
            _currentDriver.PhoneNumber = PhoneEntry.Text;
            _currentDriver.EmergencyPhoneNumber = ContatoEmergenciaEntry.Text;
            _currentDriver.CNH = CNHEntry.Text;
            _currentDriver.Address = AddressEntry.Text;
            _currentDriver.BirthDate = BirthDatePicker.Date;
            _currentDriver.Genre = GenderPicker.SelectedItem?.ToString() ?? "Não especificado";

            // Atualiza a senha se foi alterada
            if (!string.IsNullOrWhiteSpace(NewPasswordEntry.Text))
            {
                _currentDriver.Password = NewPasswordEntry.Text;
            }

            // Salva no banco de dados
            await _databaseService.UpdateDriver(_currentDriver);

            await DisplayAlert("Sucesso", "Dados atualizados com sucesso!", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao salvar alterações: {ex.Message}", "OK");
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

    private void OnPointerEntered(object sender, PointerEventArgs e)
    {
        // Ação quando o mouse entra no botão
        ((Button)sender).BackgroundColor = Colors.DarkRed; // Muda a cor do botão
    }

    private void OnPointerExited(object sender, PointerEventArgs e)
    {
        // Ação quando o mouse sai do botão
        ((Button)sender).BackgroundColor = Colors.Red; // Volta à cor original
    }

    private void OnPointer2Entered(object sender, PointerEventArgs e)
    {
        // Ação quando o mouse entra no botão
        ((Button)sender).TextColor = Colors.DarkRed; // Muda a cor do botão
    }

    private void OnPointer2Exited(object sender, PointerEventArgs e)
    {
        // Ação quando o mouse sai do botão
        ((Button)sender).TextColor = Colors.Red; // Volta à cor original
    }
}