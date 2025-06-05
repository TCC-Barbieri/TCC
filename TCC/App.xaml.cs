using TCC.Views;

namespace TCC
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            Application.Current.MainPage = new NavigationPage(new Home());
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            Window window = base.CreateWindow(activationState); // Cria a janela principal

            window.Width = 800; // Define largura
            window.Height = 600; // Define altura

            return window; // Retorna a janela configurada
        }
    }
}