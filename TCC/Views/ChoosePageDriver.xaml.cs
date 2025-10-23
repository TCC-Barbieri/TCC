using CommunityToolkit.Maui.Extensions;
using Microsoft.Maui.Controls;

namespace TCC.Views
{
    public partial class ChoosePageDriver : ContentPage
    {
        public ChoosePageDriver()
        {
            InitializeComponent();
        }

        private async void IniciarViagemButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new ConfigurarViagemPage());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao navegar para a página de viagem: {ex.Message}", "OK");
            }
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
    }
}