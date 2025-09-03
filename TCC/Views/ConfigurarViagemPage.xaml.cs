using Microsoft.Maui.Controls;
using System;

namespace TCC.Views
{
    public partial class ConfigurarViagemPage : ContentPage
    {
        public ConfigurarViagemPage()
        {
            InitializeComponent();

            // Inicializar valores padrão
            InitializeDefaultValues();

            // Configurar eventos para atualizar o resumo
            SetupEventHandlers();
        }

        private void InitializeDefaultValues()
        {
            // Definir data padrão para hoje
            DataViagemPicker.Date = DateTime.Today;

            // Definir horário padrão para agora + 1 hora
            HorarioPartidaPicker.Time = DateTime.Now.AddHours(1).TimeOfDay;

            // Capacidade padrão
            CapacidadeEntry.Text = "40";

            // Atualizar resumo inicial
            UpdateResumo();
        }

        private void SetupEventHandlers()
        {
            // Eventos para atualizar o resumo automaticamente
            LocalPartidaPicker.SelectedIndexChanged += OnConfigurationChanged;
            LocalDestinoPicker.SelectedIndexChanged += OnConfigurationChanged;
            DataViagemPicker.DateSelected += OnConfigurationChanged;
            HorarioPartidaPicker.PropertyChanged += OnConfigurationChanged;
            CapacidadeEntry.TextChanged += OnConfigurationChanged;
        }

        private void OnConfigurationChanged(object sender, EventArgs e)
        {
            UpdateResumo();
        }

        private void UpdateResumo()
        {
            try
            {
                // Atualizar rota
                string partida = LocalPartidaPicker.SelectedItem?.ToString() ?? "Não selecionado";
                string destino = LocalDestinoPicker.SelectedItem?.ToString() ?? "Não selecionado";
                ResumoRotaLabel.Text = $"{partida} → {destino}";

                // Atualizar data e hora
                string data = DataViagemPicker.Date.ToString("dd/MM/yyyy");
                string hora = HorarioPartidaPicker.Time.ToString(@"hh\:mm");
                ResumoDataHoraLabel.Text = $"{data} {hora}";

                // Atualizar capacidade
                string capacidade = string.IsNullOrEmpty(CapacidadeEntry.Text) ? "0" : CapacidadeEntry.Text;
                ResumoCapacidadeLabel.Text = $"{capacidade} passageiros";
            }
            catch (Exception ex)
            {
                // Log do erro (implementar logging se necessário)
                System.Diagnostics.Debug.WriteLine($"Erro ao atualizar resumo: {ex.Message}");
            }
        }

        private async void OnIniciarViagemClicked(object sender, EventArgs e)
        {
            try
            {
                if (!ValidarConfiguracao())
                {
                    return;
                }

                // Desabilitar botão durante o processamento
                IniciarViagemButton.IsEnabled = false;
                IniciarViagemButton.Text = "Iniciando...";

                // Simular delay de processamento
                await Task.Delay(1000);

                // TODO: Salvar dados da viagem no banco de dados
                // TODO: Inicializar serviços de geolocalização se necessário

                // Navegar para a página da viagem
                await Navigation.PushAsync(new ViagemPage());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Não foi possível iniciar a viagem: {ex.Message}", "OK");
            }
            finally
            {
                // Restaurar estado do botão
                IniciarViagemButton.IsEnabled = true;
                IniciarViagemButton.Text = "🚀 INICIAR VIAGEM";
            }
        }

        private async void OnSalvarConfigClicked(object sender, EventArgs e)
        {
            try
            {
                if (!ValidarConfiguracao())
                {
                    return;
                }

                // Desabilitar botão durante o processamento
                SalvarConfigButton.IsEnabled = false;
                SalvarConfigButton.Text = "Salvando...";

                // TODO: Implementar salvamento das configurações
                // Exemplo: await SaveConfigurationToDatabase();

                // Simular delay
                await Task.Delay(800);

                await DisplayAlert("Sucesso", "Configurações salvas com sucesso!", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Não foi possível salvar as configurações: {ex.Message}", "OK");
            }
            finally
            {
                // Restaurar estado do botão
                SalvarConfigButton.IsEnabled = true;
                SalvarConfigButton.Text = "💾 Salvar Configurações";
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
            // Validar local de partida
            if (LocalPartidaPicker.SelectedItem == null)
            {
                DisplayAlert("Atenção", "Por favor, selecione o local de partida.", "OK");
                return false;
            }

            // Validar local de destino
            if (LocalDestinoPicker.SelectedItem == null)
            {
                DisplayAlert("Atenção", "Por favor, selecione o local de destino.", "OK");
                return false;
            }

            // Verificar se partida e destino são diferentes
            if (LocalPartidaPicker.SelectedItem.ToString() == LocalDestinoPicker.SelectedItem.ToString())
            {
                DisplayAlert("Atenção", "O local de partida deve ser diferente do local de destino.", "OK");
                return false;
            }

            // Validar data (não pode ser no passado)
            if (DataViagemPicker.Date < DateTime.Today)
            {
                DisplayAlert("Atenção", "A data da viagem não pode ser no passado.", "OK");
                return false;
            }

            // Validar horário se for hoje
            if (DataViagemPicker.Date == DateTime.Today &&
                HorarioPartidaPicker.Time <= DateTime.Now.TimeOfDay)
            {
                DisplayAlert("Atenção", "O horário da viagem deve ser futuro se a data for hoje.", "OK");
                return false;
            }

            // Validar capacidade
            if (string.IsNullOrEmpty(CapacidadeEntry.Text) ||
                !int.TryParse(CapacidadeEntry.Text, out int capacidade) ||
                capacidade <= 0)
            {
                DisplayAlert("Atenção", "Por favor, informe uma capacidade válida maior que zero.", "OK");
                return false;
            }

            if (capacidade > 100)
            {
                DisplayAlert("Atenção", "A capacidade não pode ser maior que 100 passageiros.", "OK");
                return false;
            }

            return true;
        }

        private bool HasUnsavedChanges()
        {
            // Verificar se há configurações diferentes dos valores padrão
            return LocalPartidaPicker.SelectedItem != null ||
                   LocalDestinoPicker.SelectedItem != null ||
                   !string.IsNullOrEmpty(ObservacoesEditor.Text) ||
                   CapacidadeEntry.Text != "40";
        }

        // Event handlers para efeitos visuais dos botões
        private void OnPointerEntered(object sender, PointerEventArgs e)
        {
            if (sender is Button button)
            {
                button.Scale = 1.05;
                button.Opacity = 0.8;
            }
        }

        private void OnPointerExited(object sender, PointerEventArgs e)
        {
            if (sender is Button button)
            {
                button.Scale = 1.0;
                button.Opacity = 1.0;
            }
        }

        private void OnPointer2Entered(object sender, PointerEventArgs e)
        {
            if (sender is Button button)
            {
                button.Scale = 1.02;
                if (button.BackgroundColor.Equals(Colors.Transparent))
                {
                    button.BackgroundColor = Color.FromRgba("#20e86464"); // Cor de fundo sutil
                }
            }
        }

        private void OnPointer2Exited(object sender, PointerEventArgs e)
        {
            if (sender is Button button)
            {
                button.Scale = 1.0;
                if (button.BorderColor != null)
                {
                    button.BackgroundColor = Colors.Transparent;
                }
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Atualizar resumo quando a página aparecer
            UpdateResumo();

            // TODO: Carregar configurações salvas se existirem
            // LoadSavedConfigurations();
        }

        // Método para ser chamado quando integrar com mapa
        private void OnMapTapped(object sender, EventArgs e)
        {
            // TODO: Implementar seleção de pontos no mapa
            // Este método será útil quando integrar o mapa
        }

        // Métodos auxiliares para integração futura com banco de dados
        /*
        private async Task SaveConfigurationToDatabase()
        {
            var config = new ViagemConfiguration
            {
                LocalPartida = LocalPartidaPicker.SelectedItem.ToString(),
                LocalDestino = LocalDestinoPicker.SelectedItem.ToString(),
                DataViagem = DataViagemPicker.Date,
                HorarioPartida = HorarioPartidaPicker.Time,
                Capacidade = int.Parse(CapacidadeEntry.Text),
                Observacoes = ObservacoesEditor.Text,
                MotoristaId = CurrentUser.Id // TODO: Implementar controle de usuário atual
            };
            
            // TODO: Salvar no banco
            // await DatabaseService.SaveViagemConfiguration(config);
        }

        private async Task LoadSavedConfigurations()
        {
            // TODO: Carregar configurações salvas do usuário atual
            // var configs = await DatabaseService.GetSavedConfigurations(CurrentUser.Id);
        }
        */
    }
}