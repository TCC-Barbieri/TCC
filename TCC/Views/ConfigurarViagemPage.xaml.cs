using TCC.Services;

namespace TCC.Views
{
    public partial class ConfigurarViagemPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        private int _driverId;

        // CONSTRUTOR COM ID DO MOTORISTA
        public ConfigurarViagemPage(int driverId)
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            _driverId = driverId;

            SetupEventHandlers();
        }

        private void SetupEventHandlers()
        {
            TurmaPicker.SelectedIndexChanged += OnTurmaPickerChanged;
        }

        // ATUALIZA CONTADOR DE PASSAGEIROS QUANDO SELECIONA TURMA
        private async void OnTurmaPickerChanged(object sender, EventArgs e)
        {
            try
            {
                if (TurmaPicker.SelectedItem == null)
                {
                    PassengersCountLabel.Text = "Selecione uma turma para ver os passageiros";
                    PassengersCountLabel.TextColor = Colors.Gray;
                    return;
                }

                string escolaSelecionada = TurmaPicker.SelectedItem.ToString();

                // Buscar passageiros da escola com EVH = true
                var todosPassageiros = await _databaseService.GetPassengers();
                var passageirosAtivos = todosPassageiros
                    .Where(p => p.School == escolaSelecionada && p.EVH == true)
                    .ToList();

                int count = passageirosAtivos.Count;

                if (count == 0)
                {
                    PassengersCountLabel.Text = "Nenhum passageiro ativo nesta turma";
                    PassengersCountLabel.TextColor = Colors.Red;
                }
                else if (count == 1)
                {
                    PassengersCountLabel.Text = "1 passageiro ativo";
                    PassengersCountLabel.TextColor = Colors.Green;
                }
                else
                {
                    PassengersCountLabel.Text = $"{count} passageiros ativos";
                    PassengersCountLabel.TextColor = Colors.Green;
                }
            }
            catch (Exception ex)
            {
                PassengersCountLabel.Text = "Erro ao contar passageiros";
                PassengersCountLabel.TextColor = Colors.Red;
                System.Diagnostics.Debug.WriteLine($"Erro: {ex.Message}");
            }
        }

        // INICIAR VIAGEM
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

                // Obter a escola selecionada
                string escolaSelecionada = TurmaPicker.SelectedItem?.ToString();

                if (string.IsNullOrEmpty(escolaSelecionada))
                {
                    await DisplayAlert("Atenção", "Por favor, selecione uma turma.", "OK");
                    return;
                }

                // Buscar passageiros da escola selecionada que vão hoje (EVH = true)
                var todosPassageiros = await _databaseService.GetPassengers();
                var passageirosAtivos = todosPassageiros
                    .Where(p => p.School == escolaSelecionada && p.EVH == true)
                    .ToList();

                if (passageirosAtivos.Count == 0)
                {
                    await DisplayAlert("Atenção",
                        "Não há passageiros ativos para esta turma no momento.",
                        "OK");
                    return;
                }

                // Obter dados do motorista
                var drivers = await _databaseService.GetDrivers();
                var driver = drivers.FirstOrDefault(d => d.Id == _driverId);

                if (driver == null)
                {
                    await DisplayAlert("Erro", "Motorista não encontrado.", "OK");
                    return;
                }

                string localDestino = LocalDestinoPicker.SelectedItem?.ToString();

                await System.Threading.Tasks.Task.Delay(500);

                // Navegar para a página de viagem
                await Navigation.PushAsync(new ViagemPage(
                    driver,
                    passageirosAtivos,
                    localDestino
                ));
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

                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Não foi possível voltar: {ex.Message}", "OK");
            }
        }

        private bool ValidarConfiguracao()
        {
            if (TurmaPicker.SelectedItem == null)
            {
                DisplayAlert("Atenção", "Por favor, selecione a turma.", "OK");
                return false;
            }

            if (LocalDestinoPicker.SelectedItem == null)
            {
                DisplayAlert("Atenção", "Por favor, selecione o local de destino.", "OK");
                return false;
            }

            return true;
        }

        private bool HasUnsavedChanges()
        {
            return LocalDestinoPicker.SelectedItem != null || TurmaPicker.SelectedItem != null;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }
    }
}