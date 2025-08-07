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

                // Seleciona o g�nero no Picker
                if (!string.IsNullOrEmpty(_currentDriver.Genre))
                {
                    var genderIndex = GenderPicker.Items.IndexOf(_currentDriver.Genre);
                    if (genderIndex >= 0)
                        GenderPicker.SelectedIndex = genderIndex;
                }
            }
            else
            {
                await DisplayAlert("Erro", "Motorista n�o encontrado.", "OK");
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
            // Verifica��o de campos obrigat�rios
            if (string.IsNullOrWhiteSpace(NameEntry.Text) ||
                string.IsNullOrWhiteSpace(RGEntry.Text) ||
                string.IsNullOrWhiteSpace(CPFEntry.Text) ||
                string.IsNullOrWhiteSpace(EmailEntry.Text) ||
                string.IsNullOrWhiteSpace(PhoneEntry.Text) ||
                string.IsNullOrWhiteSpace(ContatoEmergenciaEntry.Text) ||
                string.IsNullOrWhiteSpace(CNHEntry.Text) ||
                string.IsNullOrWhiteSpace(AddressEntry.Text))
            {
                await DisplayAlert("Campos obrigat�rios", "Por favor, preencha todos os campos.", "OK");
                return;
            }

            // Valida��o de senha (se foi alterada)
            if (!string.IsNullOrWhiteSpace(NewPasswordEntry.Text))
            {
                if (NewPasswordEntry.Text != ConfirmNewPasswordEntry.Text)
                {
                    await DisplayAlert("Erro", "As novas senhas n�o coincidem.", "OK");
                    return;
                }

                if (NewPasswordEntry.Text.Length < 6)
                {
                    await DisplayAlert("Erro", "A nova senha deve ter pelo menos 6 caracteres.", "OK");
                    return;
                }
            }

            // Verifica se algum dado foi alterado e se j� est� em uso por outro usu�rio
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
                await DisplayAlert("Dados j� cadastrados", validationResult.Message, "OK");
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
            _currentDriver.Genre = GenderPicker.SelectedItem?.ToString() ?? "N�o especificado";

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
            await DisplayAlert("Erro", $"Erro ao salvar altera��es: {ex.Message}", "OK");
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