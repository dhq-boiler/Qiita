using Reactive.Bindings;
using System;

namespace net7GridView.ViewModels
{
    public class NRectangleViewModel : DesignerItemViewModelBase
    {
        public NRectangleViewModel()
            : base()
        {
        }

        public NRectangleViewModel(double left, double top, double width, double height)
            : base()
        {
            Left.Value = left;
            Top.Value = top;
            Width.Value = width;
            PathGeometryNoRotate.Value = null;
            Height.Value = height;
        }

        public NRectangleViewModel(double left, double top, double width, double height, double angleInDegrees)
            : this(left, top, width, height)
        {
            RotationAngle.Value = angleInDegrees;
            Matrix.Value.RotateAt(angleInDegrees, 0, 0);
        }

        public ReactiveCommand MouseDoubleClickCommand { get; } = new ReactiveCommand();

        public override bool SupportsPropertyDialog => true;

        public ReactivePropertySlim<double> RadiusX { get; } = new ReactivePropertySlim<double>();

        public ReactivePropertySlim<double> RadiusY { get; } =  new ReactivePropertySlim<double>();

        public override Type GetViewType()
        {
            return typeof(System.Windows.Shapes.Path);
        }

    }
}
