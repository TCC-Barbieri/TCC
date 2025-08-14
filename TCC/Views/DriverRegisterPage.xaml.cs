using TCC.Models;
using TCC.Services;
using System;

namespace TCC.Views;

public partial class DriverRegisterPage : ContentPage
{
    private readonly DatabaseService _databaseService = new();

    public DriverRegisterPage()
    {
        InitializeComponent();
    }

    private async void OnRegister_Clicked(object sender, EventArgs e)
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
                string.IsNullOrWhiteSpace(AddressEntry.Text) ||
                string.IsNullOrWhiteSpace(PasswordEntry.Text) ||
                string.IsNullOrWhiteSpace(ConfirmPasswordEntry.Text))
            {
                await DisplayAlert("Campos obrigatórios", "Por favor, preencha todos os campos.", "OK");
                return;
            }


            // Verificação de senhas
            if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
            {
                await DisplayAlert("Erro", "As senhas não coincidem.", "OK");
                return;
            }

            if (PasswordEntry.Text.Length < 6)
            {
                await DisplayAlert("Erro", "A senha deve ter pelo menos 6 caracteres.", "OK");
                return;
            }

            if (GenderPicker.SelectedIndex == -1)
            {
                await DisplayAlert("Campos Obrigatórios", "Por favor preencha todos os campos.", "OK");
                return;
            }

            // Validação de dados únicos (RG, CPF, Email, Telefone, CNH)
            var validationResult = await _databaseService.ValidateUniqueUserData(
                rg: RGEntry.Text.Trim(),
                cpf: CPFEntry.Text.Trim(),
                email: EmailEntry.Text.Trim(),
                phone: PhoneEntry.Text.Trim(),
                cnh: CNHEntry.Text.Trim()
            );

            if (!validationResult.IsValid)
            {
                await DisplayAlert("Dados já cadastrados", validationResult.Message, "OK");
                return;
            }

            // Cria o objeto motorista
            var driver = new Driver
            {
                Name = NameEntry.Text.Trim(),
                RG = RGEntry.Text.Trim(),
                CPF = CPFEntry.Text.Trim(),
                Email = EmailEntry.Text.Trim(),
                PhoneNumber = PhoneEntry.Text.Trim(),
                EmergencyPhoneNumber = ContatoEmergenciaEntry.Text.Trim(),
                CNH = CNHEntry.Text.Trim(),
                Address = AddressEntry.Text.Trim(),
                Password = PasswordEntry.Text,
                BirthDate = BirthDatePicker.Date,
                Genre = GenderPicker.SelectedItem?.ToString() ?? "Não especificado"
            };

            // Registra o motorista
            await _databaseService.CreateDriver(driver);

            await DisplayAlert("Sucesso", "Motorista registrado com sucesso!", "OK");

            // Limpa os campos após registro bem-sucedido
            ClearFields();

            // Opcional: Navegar para página de login
            // await Navigation.PushAsync(new Views.LoginPage());
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao registrar motorista: {ex.Message}", "OK");
        }
    }

    private void ClearFields()
    {
        NameEntry.Text = string.Empty;
        RGEntry.Text = string.Empty;
        CPFEntry.Text = string.Empty;
        EmailEntry.Text = string.Empty;
        PhoneEntry.Text = string.Empty;
        ContatoEmergenciaEntry.Text = string.Empty;
        CNHEntry.Text = string.Empty;
        AddressEntry.Text = string.Empty;
        PasswordEntry.Text = string.Empty;
        ConfirmPasswordEntry.Text = string.Empty;
        GenderPicker.SelectedIndex = -1;
        BirthDatePicker.Date = DateTime.Today;
    }

    private void OnAlreadyHaveAccount_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new Views.LoginPage());
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