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
                await DisplayAlert("Erro", $"Erro ao navegar para a p�gina de viagem: {ex.Message}", "OK");
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
                bool confirm = await DisplayAlert("Desconectar", "Tem certeza que deseja sair da sua conta?", "Sim", "N�o");

                if (confirm)
                {

                    // Navegar de volta para a p�gina inicial
                    await Navigation.PushAsync(new Home());
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao desconectar: {ex.Message}", "OK");
            }
        }

        // Efeitos visuais para os bot�es
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
                button.Scale = 1.05;
                button.BackgroundColor = Color.FromArgb("#CC0000");
            }
        }

        private void OnPointer2Exited(object sender, PointerEventArgs e)
        {
            if (sender is Button button)
            {
                button.Scale = 1.0;
                button.BackgroundColor = Colors.Transparent;
            }
        }
    }
}