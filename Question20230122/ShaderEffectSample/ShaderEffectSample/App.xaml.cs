using ShaderEffectSample.Views;
using Prism.Ioc;
using Prism.Unity;
using System.Windows;

namespace ShaderEffectSample
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            var w = new MainWindow();
            return w;
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
        }
    }
}
