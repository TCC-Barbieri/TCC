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
        // Você pode implementar a lógica para buscar dados reais de viagens ativas
        Title = "Viagem em Andamento";
    }

    private async void OnFinalizarViagemClicked(object sender, EventArgs e)
    {
        try
        {
            bool confirm = await DisplayAlert("Confirmar",
                "Deseja realmente finalizar esta viagem?", "Sim", "Não");

            if (confirm)
            {
                // Aqui você pode implementar a lógica para finalizar a viagem
                // Por exemplo: salvar dados da viagem no banco, calcular tempo, etc.

                TimeSpan duracaoViagem = DateTime.Now - _viagemIniciada;

                await DisplayAlert("Viagem Finalizada",
                    $"Viagem finalizada com sucesso!\nDuração: {duracaoViagem.Hours}h {duracaoViagem.Minutes}min",
                    "OK");

                // Voltar para a página anterior (Index)
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
            bool confirm = await DisplayAlert("Atenção",
                "A viagem ainda está em andamento. Deseja realmente voltar?", "Sim", "Não");

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
}