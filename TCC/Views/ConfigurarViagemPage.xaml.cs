using Microsoft.Maui.Controls;
using System;

namespace TCC.Views
{
    public partial class ConfigurarViagemPage : ContentPage
    {
        public ConfigurarViagemPage()
        {
            InitializeComponent();

            // Configurar eventos se necessário
            SetupEventHandlers();
        }

        private void SetupEventHandlers()
        {
            // Eventos para funcionalidades futuras se necessário
            LocalDestinoPicker.SelectedIndexChanged += OnConfigurationChanged;
        }

        private void OnConfigurationChanged(object sender, EventArgs e)
        {
            // Método para futuras implementações se necessário
        }

        private async void OnIniciarViagemClicked(object sender, EventArgs e)
        {
            try
            {
                if (!ValidarConfiguracao())
                {
                    return;
                }

                IniciarViagemButton.IsEnabled = false;
                IniciarViagemButton.Text = "Iniciando...";

                await Task.Delay(1000);

                await Navigation.PushAsync(new ViagemPage());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Não foi possível iniciar a viagem: {ex.Message}", "OK");
            }
            finally
            {
                IniciarViagemButton.IsEnabled = true;
                IniciarViagemButton.Text = "🚀 INICIAR VIAGEM";
            }
        }



        private async void OnVoltarClicked(object sender, EventArgs e)
        {
            try
            {
                // Verificar se há alterações não salvas
                bool hasUnsavedChanges = HasUnsavedChanges();

                if (hasUnsavedChanges)
                {
                    bool shouldDiscard = await DisplayAlert(
                        "Alterações não salvas",
                        "Você tem configurações não salvas. Deseja sair mesmo assim?",
                        "Sair",
                        "Cancelar");

                    if (!shouldDiscard)
                        return;
                }

                // Voltar para a página anterior (ChoosePageDriver)
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Não foi possível voltar: {ex.Message}", "OK");
            }
        }

        private bool ValidarConfiguracao()
        {
            // Validar local de destino
            if (LocalDestinoPicker.SelectedItem == null)
            {
                DisplayAlert("Atenção", "Por favor, selecione o local de destino.", "OK");
                return false;
            }

            return true;
        }

        private bool HasUnsavedChanges()
        {
            // Verificar se há configurações diferentes dos valores padrão
            return LocalDestinoPicker.SelectedItem != null;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }
    }
}