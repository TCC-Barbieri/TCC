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
        // Ação quando o mouse entra no botão
        ((Button)sender).BackgroundColor = Colors.Gainsboro; // Muda a cor do botão
    }

    private void OnPointerExited(object sender, PointerEventArgs e)
    {
        // Ação quando o mouse sai do botão
        ((Button)sender).BackgroundColor = Colors.White; // Volta à cor original
    }
}