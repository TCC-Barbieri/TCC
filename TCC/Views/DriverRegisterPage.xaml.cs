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
            // Verifica��o de campos obrigat�rios
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
                await DisplayAlert("Campos obrigat�rios", "Por favor, preencha todos os campos.", "OK");
                return;
            }

            if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
            {
                await DisplayAlert("Erro", "As senhas n�o coincidem.", "OK");
                return;
            }

            var driver = new Driver
            {
                Name = NameEntry.Text,
                RG = RGEntry.Text,
                CPF = CPFEntry.Text,
                Email = EmailEntry.Text,
                PhoneNumber = PhoneEntry.Text,
                EmergencyPhoneNumber = ContatoEmergenciaEntry.Text,
                CNH = CNHEntry.Text,
                Address = AddressEntry.Text,
                Password = PasswordEntry.Text,
                BirthDate = BirthDatePicker.Date,
                Genre = GenderPicker.SelectedItem?.ToString() ?? "N�o especificado"
            };

            if (await _databaseService.IsEmailTaken(driver.Email))
            {
                await DisplayAlert("Erro", "Este e-mail j� est� em uso.", "OK");
                return;
            }

            await _databaseService.CreateDriver(driver);
            await DisplayAlert("Sucesso", "Motorista registrado com sucesso!", "OK");
            // Navega��o ou limpeza dos campos pode ser feita aqui
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao registrar motorista: {ex.Message}", "OK");
        }
    }
}
