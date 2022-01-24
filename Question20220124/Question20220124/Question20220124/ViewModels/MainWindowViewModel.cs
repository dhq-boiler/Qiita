using Prism.Mvvm;
using Prism.Services.Dialogs;
using Question20220124.Helpers;
using Question20220124.Models;
using Question20220124.Views;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Question20220124.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private IDialogService _dialogService;
        public ReactiveCommand OpenColorPickerCommand { get; } = new ReactiveCommand();

        public ReactivePropertySlim<Color> Color { get; } = new ReactivePropertySlim<Color>();

        public ReactivePropertySlim<ColorSpots> ColorSpots { get; } = new ReactivePropertySlim<ColorSpots>();


        public MainWindowViewModel(IDialogService dialogService)
        {
            Color.Value = Colors.White;
            ColorSpots.Value = new ColorSpots();
            _dialogService = dialogService;
            OpenColorPickerCommand.Subscribe(_ =>
            {
                IDialogResult result = null;
                _dialogService.ShowDialog(nameof(ColorPicker),
                                           new DialogParameters()
                                           {
                                               {
                                                   "ColorExchange",
                                                   new ColorExchange()
                                                   {
                                                       Old = Color.Value
                                                   }
                                               },
                                               {
                                                   "ColorSpots",
                                                   ColorSpots.Value
                                               }
                                           },
                                           ret => result = ret);
                if (result != null)
                {
                    var exchange = result.Parameters.GetValue<ColorExchange>("ColorExchange");
                    if (exchange != null)
                    {
                        Color.Value = exchange.New.Value;
                    }
                    var colorSpots = result.Parameters.GetValue<ColorSpots>("ColorSpots");
                    if (colorSpots != null)
                    {
                        ColorSpots.Value = colorSpots;
                    }
                }
            });
        }
    }
}
