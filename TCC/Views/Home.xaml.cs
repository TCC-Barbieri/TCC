namespace TCC.Views;

public partial class Home : ContentPage
{
	public Home()
	{
		InitializeComponent();
	}

    private void registerButton_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new ChoicePage());
    }

    private void loginButton_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new LoginPage());
    }
    private void OnPointerEntered(object sender, PointerEventArgs e)
    {
        // Ação quando o mouse entra no botão
        ((Button)sender).BackgroundColor = Colors.Gainsboro; // Muda a cor do botão
    }

    private void OnPointerExited(object sender, PointerEventArgs e)
    {
        // Ação quando o mouse sai do botão
        ((Button)sender).BackgroundColor = Colors.White; // Volta à cor original
    }

    private void OnPointer2Entered(object sender, PointerEventArgs e)
    {
        // Ação quando o mouse entra no botão
        ((Button)sender).TextColor = Colors.Gainsboro; // Muda a cor do botão
    }

    private void OnPointer2Exited(object sender, PointerEventArgs e)
    {
        // Ação quando o mouse sai do botão
        ((Button)sender).TextColor = Colors.White; // Volta à cor original
    }
}