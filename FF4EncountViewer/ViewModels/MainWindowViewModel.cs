using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reactive.Bindings;
using Prism.Regions;
using Unity;

using FF4EncountViewer.Views;
using System.Windows;

namespace FF4EncountViewer.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        public ReactiveCommand ExitCommand { get; private set; }

        public ReactiveCommand AboutCommand { get; private set; }

        public MainWindowViewModel()
        {
            ExitCommand = new ReactiveCommand()
                .WithSubscribe(() =>
                {
                    System.Windows.Application.Current.Shutdown();
                });

            AboutCommand = new ReactiveCommand()
            .WithSubscribe(() =>
            {
                MessageBox.Show("FINAL FANTASY IV Encount Viewer ver 0.20\n\nFINAL FANTASY IV© 1991,2020 SQUARE ENIX CO., LTD. All Rights Reserved.LOGO ILLUSTRATION: © 2007 YOSHITAKA AMANO", "FINAL FANTASY IV Encount Viewer");
            });
        }
    }
}
