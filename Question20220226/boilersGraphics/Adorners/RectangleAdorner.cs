using boilersGraphics.Controls;
using boilersGraphics.Dao;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace boilersGraphics.Adorners
{
    internal class RectangleAdorner : Adorner
    {
        private DesignerCanvas _designerCanvas;
        private Point? _startPoint;
        private NRectangleViewModel item;
        private Point? _endPoint;
        private Pen _rectanglePen;

        public RectangleAdorner(DesignerCanvas designerCanvas, Point? dragStartPoint, NRectangleViewModel item)
            : base(designerCanvas)
        {
            _designerCanvas = designerCanvas;
            _startPoint = dragStartPoint;
            this.item = item;
            var parent = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;
            var brush = parent.EdgeBrush.Value.Clone();
            brush.Opacity = 0.5;
            _rectanglePen = new Pen(brush, parent.EdgeThickness.Value.Value);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!this.IsMouseCaptured)
                    this.CaptureMouse();

                //ドラッグ終了座標を更新
                _endPoint = e.GetPosition(this);
                var currentPosition = _endPoint.Value;
                _endPoint = currentPosition;

                (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.CurrentPoint = currentPosition;
                (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = $"({_startPoint.Value.X}, {_startPoint.Value.Y}) - ({_endPoint.Value.X}, {_endPoint.Value.Y}) (w, h) = ({_endPoint.Value.X - _startPoint.Value.X}, {_endPoint.Value.Y - _startPoint.Value.Y})";

                this.InvalidateVisual();
            }
            else
            {
                if (this.IsMouseCaptured) this.ReleaseMouseCapture();
            }

            e.Handled = true;
        }

        protected override void OnMouseUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            // release mouse capture
            if (this.IsMouseCaptured) this.ReleaseMouseCapture();

            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(_designerCanvas);
            if (adornerLayer != null)
            {
                adornerLayer.Remove(this);
            }

            if (_startPoint.HasValue && _endPoint.HasValue)
            {
                var rand = new Random();
                item.Owner = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;
                item.Left.Value = Math.Min(_startPoint.Value.X, _endPoint.Value.X);
                item.Top.Value = Math.Min(_startPoint.Value.Y, _endPoint.Value.Y);
                item.Width.Value = Math.Max(_startPoint.Value.X - _endPoint.Value.X, _endPoint.Value.X - _startPoint.Value.X);
                item.Height.Value = Math.Max(_startPoint.Value.Y - _endPoint.Value.Y, _endPoint.Value.Y - _startPoint.Value.Y);
                item.EdgeBrush.Value = item.Owner.EdgeBrush.Value.Clone();
                item.FillBrush.Value = item.Owner.FillBrush.Value.Clone();
                item.EdgeThickness.Value = item.Owner.EdgeThickness.Value.Value;
                item.ZIndex.Value = item.Owner.Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children.Value).Count();
                item.PathGeometry.Value = GeometryCreator.CreateRectangle(item);
                item.IsSelected.Value = true;
                item.IsVisible.Value = true;
                item.Owner.DeselectAll();
                ((AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel).AddItemCommand.Execute(item);

                UpdateStatisticsCount();

                _startPoint = null;
                _endPoint = null;
            }

            (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "";
            (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = "";

            e.Handled = true;
        }

        private static void UpdateStatisticsCount()
        {
            var statistics = (App.Current.MainWindow.DataContext as MainWindowViewModel).Statistics.Value;
            statistics.NumberOfDrawsOfTheRectangleTool++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

            if (_startPoint.HasValue && _endPoint.HasValue)
                dc.DrawRectangle(((AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel).FillBrush.Value.Clone(), _rectanglePen, ShiftEdgeThickness());
        }

        private Rect ShiftEdgeThickness()
        {
            var parent = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;
            var point1 = _startPoint.Value;
            point1.X += parent.EdgeThickness.Value.Value / 2;
            point1.Y += parent.EdgeThickness.Value.Value / 2;
            var point2 = _endPoint.Value;
            point2.X -= parent.EdgeThickness.Value.Value / 2;
            point2.Y -= parent.EdgeThickness.Value.Value / 2;
            return new Rect(point1, point2);
        }
    }
}
