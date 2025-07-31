namespace TCC.Views;

public partial class ChoicePage : ContentPage
{
	public ChoicePage()
	{
		InitializeComponent();
	}

    private void driverButton_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new Views.DriverRegisterPage());
    }

    private void passengerButton_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new Views.PassengerRegisterPage());
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