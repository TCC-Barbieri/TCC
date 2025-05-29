namespace TCC.Views;

public partial class PassengerRegisterPage : ContentPage
{
	public PassengerRegisterPage()
	{
		InitializeComponent();
	}

    // Mostrar detalhes se "Sim" for selecionado
    private void OnSimTapped(object sender, EventArgs e)
    {
        AtendimentoSimRadio.IsChecked = true;
    }

    // Ocultar detalhes se "Não" for selecionado
    private void OnNaoTapped(object sender, EventArgs e)
    {
        AtendimentoNaoRadio.IsChecked = true;
    }

    // Controla a visibilidade dos detalhes do atendimento especial
    private void OnAtendimentoEspecialChanged(object sender, CheckedChangedEventArgs e)
    {
        AtendimentoEspecialDetalhesLayout.IsVisible = AtendimentoSimRadio.IsChecked;
    }

    // Evento do botão registrar
    private async void OnRegistrarClicked(object sender, EventArgs e)
    {
       
    }

    // Evento do botão "Já tenho uma conta"
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        
    }
}