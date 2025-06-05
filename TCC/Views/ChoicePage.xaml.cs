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
}