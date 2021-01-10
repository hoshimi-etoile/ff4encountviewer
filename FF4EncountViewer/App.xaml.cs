using System.Windows;
using FF4EncountViewer.Models;
using FF4EncountViewer.ViewModels;
using FF4EncountViewer.Views;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Unity;

namespace FF4EncountViewer
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IModel,Model>();
        }

        protected override void ConfigureViewModelLocator()
        {
            base.ConfigureViewModelLocator();
            ViewModelLocationProvider.Register<MainWindow, MainWindowViewModel>();
            ViewModelLocationProvider.Register<MonsterEncountView, MonsterEncountViewModel>();
        }
    }
}
