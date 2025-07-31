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

}