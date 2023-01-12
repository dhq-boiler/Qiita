using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows;

namespace net7GridView.ViewModels
{
    public abstract class DesignerItemViewModelBase : SelectableDesignerItemViewModelBase
    {
        private bool _showConnectors = false;

        private double _MinWidth;
        private double _MinHeight;
        public static readonly double DefaultWidth = 65d;
        public static readonly double DefaultHeight = 65d;

        public DesignerItemViewModelBase(int id, double left, double top) : base(id)
        {
            Left.Value = left;
            Top.Value = top;
        }

        public DesignerItemViewModelBase() : base()
        {
        }

        public double MinWidth
        {
            get { return _MinWidth; }
            set { SetProperty(ref _MinWidth, value); }
        }

        public double MinHeight
        {
            get { return _MinHeight; }
            set { SetProperty(ref _MinHeight, value); }
        }

        public ReactivePropertySlim<double> Width { get; } = new ReactivePropertySlim<double>(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe | ReactivePropertyMode.DistinctUntilChanged);

        public ReactivePropertySlim<double> Height { get; } = new ReactivePropertySlim<double>(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe | ReactivePropertyMode.DistinctUntilChanged);

        public ReadOnlyReactivePropertySlim<Size> Size => Width.CombineLatest(Height, (w, h) => new Size(w, h)).ToReadOnlyReactivePropertySlim();
        
        public ReadOnlyReactivePropertySlim<Size> SizeIncludeFrame => Width.CombineLatest(Height, (w, h) => new Size(w + 1, h + 1)).ToReadOnlyReactivePropertySlim();

        public bool ShowConnectors
        {
            get
            {
                return _showConnectors;
            }
            set
            {
                if (_showConnectors != value)
                {
                    _showConnectors = value;
                    RaisePropertyChanged("ShowConnectors");
                }
            }
        }

        public ReactivePropertySlim<string> Pool { get; } = new ReactivePropertySlim<string>();

        public ReactivePropertySlim<double> Left { get; } = new ReactivePropertySlim<double>(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe | ReactivePropertyMode.DistinctUntilChanged);

        public ReactivePropertySlim<double> Top { get; } = new ReactivePropertySlim<double>(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe | ReactivePropertyMode.DistinctUntilChanged);

        public ReadOnlyReactivePropertySlim<double> Right { get; private set; }

        public ReadOnlyReactivePropertySlim<double> Bottom { get; private set; }

        public ReactivePropertySlim<double> CenterX { get; } = new ReactivePropertySlim<double>();
        public ReactivePropertySlim<double> CenterY { get; } = new ReactivePropertySlim<double>();

        public ReactiveProperty<Point> CenterPoint { get; private set; }

        public List<IDisposable> SnapObjs { get; set; } = new List<IDisposable>();

        private void UpdateCenterPoint()
        {
            var leftTop = new Point(Left.Value, Top.Value);
            var center = new Point(leftTop.X + Width.Value * 0.5, leftTop.Y + Height.Value * 0.5);
            CenterX.Value = center.X;
            CenterY.Value = center.Y;
        }

        private void UpdateMatrix(double oldAngle, double newAngle)
        {
            var targetMatrix = Matrix.Value;
            targetMatrix.RotateAt(newAngle - oldAngle, 0, 0);
            Matrix.Value = targetMatrix;
        }

        private void UpdateLeft(double value)
        {
            Left.Value = value - Width.Value / 2;
        }

        private void UpdateTop(double value)
        {
            Top.Value = value - Height.Value / 2;
        }

        public virtual void UpdateMargin(string propertyName, object oldValue, object newValue)
        { }
    }
}
