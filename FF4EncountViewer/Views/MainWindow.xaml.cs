using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FF4EncountViewer.Views;
using Prism.Ioc;
using Prism.Regions;
using Unity;

namespace FF4EncountViewer
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        [Dependency]
        public IContainerExtension ContainerExtension { get; set; }

        [Dependency]
        public IRegionManager RegionManager { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        public void OnLoaded(object sender, RoutedEventArgs e)
        {
            RegionManager.AddToRegion("MainRegion", ContainerExtension.Resolve<MonsterEncountView>());

            RegionManager.RequestNavigate("MainRegion", nameof(MonsterEncountView));
        }
    }
}
