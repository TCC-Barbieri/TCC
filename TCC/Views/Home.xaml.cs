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
        // A��o quando o mouse entra no bot�o
        ((Button)sender).BackgroundColor = Colors.Gainsboro; // Muda a cor do bot�o
    }

    private void OnPointerExited(object sender, PointerEventArgs e)
    {
        // A��o quando o mouse sai do bot�o
        ((Button)sender).BackgroundColor = Colors.White; // Volta � cor original
    }

}