using Prism.Mvvm;
using Reactive.Bindings;
using System.Windows.Media;

namespace net7GridView.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        public NRectangleViewModel Instance { get; set; }
        public ReactiveCollection<PropertyOptionsValueCombination> Properties { get; } = new ReactiveCollection<PropertyOptionsValueCombination>();

        public MainWindowViewModel()
        {
            Instance = new NRectangleViewModel();
            Instance.PenLineJoin.Value = System.Windows.Media.PenLineJoin.Round;
            Instance.StrokeDashArray.Value = DoubleCollection.Parse("1 2 1 2 1");
            Properties.Add(new PropertyOptionsValueCombinationStruct<NRectangleViewModel, System.Windows.Media.PenLineJoin>(Instance, "PenLineJoin", new System.Windows.Media.PenLineJoin[] {
                System.Windows.Media.PenLineJoin.Miter,
                System.Windows.Media.PenLineJoin.Bevel,
                System.Windows.Media.PenLineJoin.Round
            }));
            Properties.Add(new PropertyOptionsValueCombinationClass<NRectangleViewModel, DoubleCollection>(Instance, "StrokeDashArray"));
        }
    }
}
