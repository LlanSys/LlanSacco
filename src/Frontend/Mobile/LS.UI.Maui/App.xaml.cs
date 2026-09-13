namespace LS.UI.Maui;

internal sealed partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    //protected override Window CreateWindow(IActivationState? activationState)
    //    => new Window(new MainPage()) { Title = "LS.UI.Maui" };

    protected override Window CreateWindow(IActivationState? _)
        => new(new MainPage());
}
