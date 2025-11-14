using TCC.Models;
using TCC.Services;

namespace TCC.Views
{
    public partial class ChoosePagePassenger : ContentPage
    {
        private bool _EVH = true;

        private readonly DatabaseService _databaseService = new();
        private readonly int _passengerId;
        private Passenger _currentPassenger;


        public ChoosePagePassenger(int passengerId)
        {
            InitializeComponent();
            _passengerId = passengerId;
        }



        private async void PerfilButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new Index());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao navegar para o perfil: {ex.Message}", "OK");
            }
        }

        private async void GruposButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new GroupViewPage());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao navegar para grupos: {ex.Message}", "OK");
            }
        }

        private async void DesconectarButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                bool confirm = await DisplayAlert("Desconectar", "Tem certeza que deseja sair da sua conta?", "Sim", "Não");

                if (confirm)
                {

                    // Navegar de volta para a página inicial
                    await Navigation.PushAsync(new Home());
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao desconectar: {ex.Message}", "OK");
            }
        }


        private async void EVH_Clicked(object sender, EventArgs e)
        {
            var passengers = await _databaseService.GetPassengers();
            _currentPassenger = passengers.FirstOrDefault(p => p.Id == _passengerId);

            _EVH = !_EVH;

            if(_EVH)
            {
                EVH.Text = "Vou hoje";
                EVH.BackgroundColor = Color.FromArgb("#28a745"); // Verde
                EVH.TextColor = Colors.White;

                _currentPassenger.EVH = true;

                await _databaseService.UpdatePassenger(_currentPassenger);
            }
            else
            {
                EVH.Text = "Não vou hoje";
                EVH.BackgroundColor = Color.FromArgb("#dc3545"); // Vermelho
                EVH.TextColor = Colors.White;

                _currentPassenger.EVH = false;

                await _databaseService.UpdatePassenger(_currentPassenger);
            }
        }
    }
}