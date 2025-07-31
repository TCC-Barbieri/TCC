namespace TCC.Views;

public partial class ViagemPage : ContentPage
{
    public ViagemPage()
    {
        InitializeComponent();
    }

    private async void OnFinalizarViagemClicked(object sender, EventArgs e)
    {
        var result = await DisplayAlert("Finalizar Viagem",
            "Tem certeza que deseja finalizar a viagem?",
            "Sim", "N�o");

        if (result)
        {
            await DisplayAlert("Viagem Finalizada",
                "Viagem finalizada com sucesso!\nObrigado por usar o Transporte Barbieri.",
                "OK");

            // Voltar para a p�gina inicial
            await Navigation.PopAsync();
        }
    }

    private async void OnVoltarClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}