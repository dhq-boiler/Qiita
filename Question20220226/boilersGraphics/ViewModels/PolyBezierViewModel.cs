using boilersGraphics.Helpers;
using boilersGraphics.Views;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using Reactive.Bindings;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Shapes;

namespace boilersGraphics.ViewModels
{
    public class PolyBezierViewModel : ConnectorBaseViewModel
    {

        public PolyBezierViewModel()
            : base()
        { }

        public PolyBezierViewModel(int id, IDiagramViewModel parent)
            : base(id, parent)
        {
            Init(parent);
        }

        public PolyBezierViewModel(IDiagramViewModel parent, Point beginPoint)
            : base(0, parent)
        {
            Init(parent);
            Points.Add(beginPoint);
        }

        private void Init(IDiagramViewModel diagramViewModel)
        {
            Points.CollectionChanged += Points_CollectionChanged;
        }

        private void Points_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
        }

        public override object Clone()
        {
            var clone = new PolyBezierViewModel(0, Owner);
            clone.EdgeBrush.Value = EdgeBrush.Value;
            clone.FillBrush.Value = FillBrush.Value;
            clone.EdgeThickness.Value = EdgeThickness.Value;
            clone.Points = Points;
            clone.PathGeometry.Value = GeometryCreator.CreatePolyBezier(clone);
            clone.StrokeStartLineCap.Value = StrokeStartLineCap.Value;
            clone.StrokeEndLineCap.Value = StrokeEndLineCap.Value;
            clone.PenLineJoin.Value = PenLineJoin.Value;
            clone.StrokeDashArray.Value = StrokeDashArray.Value;
            return clone;
        }

        public override Type GetViewType()
        {
            return typeof(System.Windows.Shapes.Path);
        }
    }
}
