﻿using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using Question20210925;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace boilersGraphics.Adorners
{
    public class RubberbandAdorner : Adorner
    {
        private Point? _startPoint;
        private Point? _endPoint;
        private Pen _rubberbandPen;

        private DesignerCanvas _designerCanvas;

        public RubberbandAdorner(DesignerCanvas designerCanvas, Point? dragStartPoint)
            : base(designerCanvas)
        {
            _designerCanvas = designerCanvas;
            _startPoint = dragStartPoint;
            _rubberbandPen = new Pen(Brushes.LightSlateGray, 1);
            _rubberbandPen.DashStyle = new DashStyle(new double[] { 2 }, 1);
        }

        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!this.IsMouseCaptured)
                    this.CaptureMouse();

                _endPoint = e.GetPosition(this);

                (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = $"({_startPoint.Value.X}, {_startPoint.Value.Y}) - ({_endPoint.Value.X}, {_endPoint.Value.Y})";

                UpdateSelection();
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

            // remove this adorner from adorner layer
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(_designerCanvas);
            if (adornerLayer != null)
                adornerLayer.Remove(this);

            (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "";
            (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = "";

            e.Handled = true;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            // without a background the OnMouseMove event would not be fired !
            // Alternative: implement a Canvas as a child of this adorner, like
            // the ConnectionAdorner does.
            dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

            if (_startPoint.HasValue && _endPoint.HasValue)
                dc.DrawRectangle(Brushes.Transparent, _rubberbandPen, new Rect(_startPoint.Value, _endPoint.Value));
        }


        private T GetParent<T>(Type parentType, DependencyObject dependencyObject) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(dependencyObject);
            if (parent.GetType() == parentType)
                return (T)parent;

            return GetParent<T>(parentType, parent);
        }



        private void UpdateSelection()
        {
            IDiagramViewModel vm = (_designerCanvas.DataContext as IDiagramViewModel);
            Rect rubberBand = new Rect(_startPoint.Value, _endPoint.Value);
            ItemsControl itemsControl = GetParent<ItemsControl>(typeof(ItemsControl), _designerCanvas);

            foreach (SelectableDesignerItemViewModelBase item in vm.Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                                                                          .Where(x => x is LayerItem)
                                                                          .Select(x => (x as LayerItem).Item.Value))
            {
                if (item is SelectableDesignerItemViewModelBase)
                {
                    if (item is ConnectorBaseViewModel connector)
                    {
                        var snapPointVM = connector.SnapPoint0VM.Value;
                        UpdateSelectionSnapPoint(rubberBand, snapPointVM);
                        snapPointVM = connector.SnapPoint1VM.Value;
                        UpdateSelectionSnapPoint(rubberBand, snapPointVM);
                    }
                    else
                    {
                        DependencyObject container = itemsControl.ItemContainerGenerator.ContainerFromItem(item);

                        Rect itemRect = VisualTreeHelper.GetDescendantBounds((Visual)container);
                        Rect itemBounds = ((Visual)container).TransformToAncestor(_designerCanvas).TransformBounds(itemRect);

                        if (rubberBand.Contains(itemBounds))
                        {
                            item.IsSelected.Value = true;
                        }
                        else
                        {
                            if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                            {
                                item.IsSelected.Value = false;
                            }
                        }
                    }
                }
            }
        }

        private void UpdateSelectionStraightConnector(Rect rubberBand, ItemsControl itemsControl, SelectableDesignerItemViewModelBase item)
        {
            if (item is ConnectorBaseViewModel connector)
            {
                var vm = connector.SnapPoint0VM.Value;
                UpdateSelectionSnapPoint(rubberBand, vm);
                vm = connector.SnapPoint1VM.Value;
                UpdateSelectionSnapPoint(rubberBand, vm);
            }
        }

        private void UpdateSelectionSnapPoint(Rect rubberBand, SnapPointViewModel vm)
        {
            LineResizeHandle container = App.Current.MainWindow.GetChildOfType<DesignerCanvas>().GetCorrespondingViews<LineResizeHandle>(vm).First();

            Rect itemRect = VisualTreeHelper.GetDescendantBounds((Visual)container);
            Rect itemBounds = ((Visual)container).TransformToAncestor(_designerCanvas).TransformBounds(itemRect);

            if (rubberBand.Contains(itemBounds))
            {
                vm.IsSelected.Value = true;
            }
            else
            {
                if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                {
                    vm.IsSelected.Value = false;
                }
            }
        }
    }
}
