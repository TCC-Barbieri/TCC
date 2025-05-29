using TCC.Views;

namespace TCC
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new DriverRegisterPage(); // Mudar página inicial 
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