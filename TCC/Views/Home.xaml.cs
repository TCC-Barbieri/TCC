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
}