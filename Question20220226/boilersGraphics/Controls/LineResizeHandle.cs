using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TsOperationHistory;
using TsOperationHistory.Extensions;

namespace boilersGraphics.Controls
{
    public class LineResizeHandle : SnapPoint
    {
        public OperationRecorder Recorder { get; } = new OperationRecorder((App.Current.MainWindow.DataContext as MainWindowViewModel).Controller);

        public LineResizeHandle()
        { 
        }

        public static readonly DependencyProperty OppositeHandleProperty = DependencyProperty.Register("OppositeHandle", typeof(LineResizeHandle), typeof(LineResizeHandle));

        public static readonly DependencyProperty TargetPointIndexProperty = DependencyProperty.Register("TargetPointIndex", typeof(int), typeof(LineResizeHandle));

        public LineResizeHandle OppositeHandle
        {
            get { return (LineResizeHandle)GetValue(OppositeHandleProperty); }
            set { SetValue(OppositeHandleProperty, value); }
        }

        public int TargetPointIndex
        {
            get { return (int)GetValue(TargetPointIndexProperty); }
            set { SetValue(TargetPointIndexProperty, value); }
        }

        public Point? BeginDragPoint { get; private set; }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            BeginDragPoint = e.GetPosition(App.Current.MainWindow.GetChildOfType<DesignerCanvas>());

            (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = boilersGraphics.Properties.Resources.String_Deform;

            Recorder.BeginRecode();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (BeginDragPoint.HasValue)
            {
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            BeginDragPoint = null;

            (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "";
            (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = "";

            Recorder.EndRecode();
        }
    }
}
