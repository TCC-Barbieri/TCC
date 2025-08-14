using Microsoft.Maui.Controls;
using TCC.Models;

namespace TCC.Views;

public partial class ViagemPage : ContentPage
{
    private DateTime _viagemIniciada;

    public ViagemPage()
    {
        InitializeComponent();
        _viagemIniciada = DateTime.Now;
        LoadViagemData();
    }

    private void LoadViagemData()
    {
        // Dados de exemplo para a viagem
        // Voc� pode implementar a l�gica para buscar dados reais de viagens ativas
        Title = "Viagem em Andamento";
    }

    private async void OnFinalizarViagemClicked(object sender, EventArgs e)
    {
        try
        {
            bool confirm = await DisplayAlert("Confirmar",
                "Deseja realmente finalizar esta viagem?", "Sim", "N�o");

            if (confirm)
            {
                // Aqui voc� pode implementar a l�gica para finalizar a viagem
                // Por exemplo: salvar dados da viagem no banco, calcular tempo, etc.

                TimeSpan duracaoViagem = DateTime.Now - _viagemIniciada;

                await DisplayAlert("Viagem Finalizada",
                    $"Viagem finalizada com sucesso!\nDura��o: {duracaoViagem.Hours}h {duracaoViagem.Minutes}min",
                    "OK");

                // Voltar para a p�gina anterior (Index)
                await Navigation.PopAsync();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao finalizar viagem: {ex.Message}", "OK");
        }
    }

    private async void OnVoltarClicked(object sender, EventArgs e)
    {
        try
        {
            bool confirm = await DisplayAlert("Aten��o",
                "A viagem ainda est� em andamento. Deseja realmente voltar?", "Sim", "N�o");

            if (confirm)
            {
                await Navigation.PopAsync();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao voltar: {ex.Message}", "OK");
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