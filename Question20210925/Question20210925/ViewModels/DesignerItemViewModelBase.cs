﻿using boilersGraphics.Helpers;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.ViewModels
{
    public abstract class DesignerItemViewModelBase : SelectableDesignerItemViewModelBase, IObservable<TransformNotification>, ICloneable
    {
        private bool _showConnectors = false;

        private double _MinWidth;
        private double _MinHeight;
        public static readonly double DefaultWidth = 65d;
        public static readonly double DefaultHeight = 65d;

        public DesignerItemViewModelBase(int id, IDiagramViewModel parent, double left, double top) : base(id, parent)
        {
            Left.Value = left;
            Top.Value = top;
            Init();
        }

        public DesignerItemViewModelBase() : base()
        {
            Init();
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

        public ReactivePropertySlim<double> Left { get; } = new ReactivePropertySlim<double>(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe | ReactivePropertyMode.DistinctUntilChanged);

        public ReactivePropertySlim<double> Top { get; } = new ReactivePropertySlim<double>(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe | ReactivePropertyMode.DistinctUntilChanged);

        public ReadOnlyReactivePropertySlim<double> Right { get; private set; }

        public ReadOnlyReactivePropertySlim<double> Bottom { get; private set; }

        public ReactivePropertySlim<Point> CenterPoint { get; } = new ReactivePropertySlim<Point>();

        public ReactivePropertySlim<Point> RotatedCenterPoint { get; } = new ReactivePropertySlim<Point>();

        public ReactivePropertySlim<TransformNotification> TransformNortification { get; } = new ReactivePropertySlim<TransformNotification>(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe | ReactivePropertyMode.DistinctUntilChanged);

        private void UpdateCenterPoint()
        {
            var leftTop = new Point(Left.Value, Top.Value);
            var center = new Point(leftTop.X + Width.Value * 0.5, leftTop.Y + Height.Value * 0.5);
            CenterPoint.Value = center;
        }

        private void Init()
        {
            MinWidth = 0;
            MinHeight = 0;

            Left
                .Zip(Left.Skip(1), (Old, New) => new { OldItem = Old, NewItem = New})
                .Subscribe(x => UpdateTransform(nameof(Left), x.OldItem, x.NewItem))
                .AddTo(_CompositeDisposable);
            Top
                .Zip(Top.Skip(1), (Old, New) => new { OldItem = Old, NewItem = New })
                .Subscribe(x => UpdateTransform(nameof(Top), x.OldItem, x.NewItem))
                .AddTo(_CompositeDisposable);
            Width
                .Zip(Width.Skip(1), (Old, New) => new { OldItem = Old, NewItem = New })
                .Subscribe(x => UpdateTransform(nameof(Width), x.OldItem, x.NewItem))
                .AddTo(_CompositeDisposable);
            Height
                .Zip(Height.Skip(1), (Old, New) => new { OldItem = Old, NewItem = New })
                .Subscribe(x => UpdateTransform(nameof(Height), x.OldItem, x.NewItem))
                .AddTo(_CompositeDisposable);
            RotationAngle
                .Zip(RotationAngle.Skip(1), (Old, New) => new { OldItem = Old, NewItem = New })
                .Subscribe(x => UpdateTransform(nameof(RotationAngle), x.OldItem, x.NewItem))
                .AddTo(_CompositeDisposable);
            Matrix
                .Zip(Matrix.Skip(1), (Old, New) => new { OldItem = Old, NewItem = New })
                .Subscribe(x => UpdateTransform(nameof(Matrix), x.OldItem, x.NewItem))
                .AddTo(_CompositeDisposable);
            Right = Left.CombineLatest(Width, (a, b) => a + b)
                        .ToReadOnlyReactivePropertySlim();
            Bottom = Top.CombineLatest(Height, (a, b) => a + b)
                        .ToReadOnlyReactivePropertySlim();

            Matrix.Value = new Matrix();
        }

        public void UpdateTransform(string propertyName, object oldValue, object newValue)
        {
            UpdateCenterPoint();
            TransformObserversOnNext(propertyName, oldValue, newValue);
            UpdatePathGeometryInCase(propertyName);
        }

        private void UpdatePathGeometryInCase(string propertyName)
        {
            switch (propertyName)
            {
                case "Left":
                case "Top":
                case "Width":
                case "Height":
                case "RotationAngle":
                case "Matrix":
                    UpdatePathGeometryIfEnable();
                    break;
                default:
                    break;
            }
        }

        public void UpdatePathGeometryIfEnable()
        {
            if (EnablePathGeometryUpdate.Value)
            {
                if (RotationAngle.Value == 0)
                {
                    PathGeometry.Value = CreateGeometry();
                }
                else
                {
                    RotatePathGeometry.Value = CreateGeometry(RotationAngle.Value);
                }
            }
        }

        public abstract PathGeometry CreateGeometry();

        public abstract PathGeometry CreateGeometry(double angle);

        public void TransformObserversOnNext(string propertyName, object oldValue, object newValue)
        {
            var tn = new TransformNotification()
            {
                Sender = this,
                PropertyName = propertyName,
                OldValue = oldValue,
                NewValue = newValue
            };
            this.TransformNortification.Value = tn;
            _observers.ForEach(x => x.OnNext(tn));
        }

        #region IObservable<TransformNotification>

        private List<IObserver<TransformNotification>> _observers = new List<IObserver<TransformNotification>>();

        public IDisposable Subscribe(IObserver<TransformNotification> observer)
        {
            _observers.Add(observer);
            observer.OnNext(new TransformNotification()
            {
                Sender = this
            });
            return new DesignerItemViewModelBaseDisposable(this, observer);
        }

        public class DesignerItemViewModelBaseDisposable : IDisposable
        {
            private DesignerItemViewModelBase _obj;
            private IObserver<TransformNotification> _observer;
            public DesignerItemViewModelBaseDisposable(DesignerItemViewModelBase obj, IObserver<TransformNotification> observer)
            {
                _obj = obj;
                _observer = observer;
            }

            public void Dispose()
            {
                _obj._observers.Remove(_observer);
            }
        }

        #endregion //IObservable<TransformNotification>
    }
}
