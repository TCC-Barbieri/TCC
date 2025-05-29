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

    // Ocultar detalhes se "N�o" for selecionado
    private void OnNaoTapped(object sender, EventArgs e)
    {
        AtendimentoNaoRadio.IsChecked = true;
    }

    // Controla a visibilidade dos detalhes do atendimento especial
    private void OnAtendimentoEspecialChanged(object sender, CheckedChangedEventArgs e)
    {
        AtendimentoEspecialDetalhesLayout.IsVisible = AtendimentoSimRadio.IsChecked;
    }

    // Evento do bot�o registrar
    private async void OnRegistrarClicked(object sender, EventArgs e)
    {
        // Aqui voc� deve pegar os valores dos campos, validar e salvar/enviar

        // Exemplo para pegar dados (voc� deve ligar os Entry a x:Name para acessar)
        string nome = ((Entry)((VerticalStackLayout)((VerticalStackLayout)Content).Children[2]).Children[1]).Text;
        string senha = ((Entry)((VerticalStackLayout)((VerticalStackLayout)Content).Children[3]).Children[1]).Text;
        string confirmarSenha = ((Entry)((VerticalStackLayout)((VerticalStackLayout)Content).Children[4]).Children[1]).Text;
        // Recomendo colocar x:Name nos Entry para facilitar acesso

        // Exemplo simples de valida��o
        if (string.IsNullOrWhiteSpace(nome))
        {
            await DisplayAlert("Erro", "Por favor, insira seu nome.", "OK");
            return;
        }
        if (string.IsNullOrWhiteSpace(senha) || senha != confirmarSenha)
        {
            await DisplayAlert("Erro", "Senhas n�o conferem ou est�o vazias.", "OK");
            return;
        }

        // Outras valida��es aqui...

        // Ap�s valida��o, enviar dados para servidor ou salvar localmente
        await DisplayAlert("Sucesso", "Cadastro realizado com sucesso!", "OK");

        // Opcional: navegar para outra p�gina ap�s o registro
        // await Navigation.PushAsync(new OutraPagina());
    }

    // Evento do bot�o "J� tenho uma conta"
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        // Navegar para a p�gina de login
        // await Navigation.PushAsync(new LoginPage());
        await DisplayAlert("Navegar", "Aqui deve ir para a tela de login.", "OK");
    }
}