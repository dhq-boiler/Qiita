﻿using boilersGraphics.Exceptions;
using boilersGraphics.Extensions;
using boilersGraphics.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace boilersGraphics.Controls
{
    public class RotateThumb : Thumb
    {
        private Matrix _initialMatrix;
        private MatrixTransform _rotateTransform;
        private Point _centerPoint;
        private FrameworkElement _designerItem;
        private Canvas _canvas;

        public RotateThumb()
        {
            DragDelta += new DragDeltaEventHandler(this.RotateThumb_DragDelta);
            DragStarted += new DragStartedEventHandler(this.RotateThumb_DragStarted);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = boilersGraphics.Properties.Resources.String_Rotate;
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "";
            (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = "";

            _canvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
        }

        private void RotateThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            _designerItem = this.GetParentOfType("selectedGrid") as FrameworkElement;

            if (_designerItem != null)
            {
                _canvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(_canvas);

                if (_canvas != null)
                {
                    _centerPoint = _designerItem.TranslatePoint(
                        new Point(_designerItem.ActualWidth * _designerItem.RenderTransformOrigin.X,
                                  _designerItem.ActualHeight * _designerItem.RenderTransformOrigin.Y),
                                  _canvas);

                    _rotateTransform = _designerItem.RenderTransform as MatrixTransform;
                    if (_rotateTransform == null)
                    {
                        throw new UnexpectedException();
                    }
                    else
                    {
                        _initialMatrix = _rotateTransform.Matrix;
                    }
                }
            }
        }

        private void RotateThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var viewModel = DataContext as DesignerItemViewModelBase;

            if (_designerItem != null && _canvas != null)
            {
                _canvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(_canvas);

                (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = $"{boilersGraphics.Properties.Resources.String_Angle}={viewModel.RotationAngle.Value}°";

                _designerItem.InvalidateMeasure();
            }
        }
    }
}
