using boilersGraphics.Controls;
using boilersGraphics.Dao;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Messenger;
using boilersGraphics.Models;
using boilersGraphics.Properties;
using boilersGraphics.UserControls;
using boilersGraphics.Views;
using Microsoft.Win32;
using NLog;
using OpenCvSharp.WpfExtensions;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Linq;
using TsOperationHistory.Extensions;

namespace boilersGraphics.ViewModels
{
    public class DiagramViewModel : BindableBase, IDiagramViewModel, IDisposable
    {
        public MainWindowViewModel MainWindowVM { get; private set; }
        private IDialogService dlgService;
        private Point _CurrentPoint;
        private ObservableCollection<Color> _FillColors = new ObservableCollection<Color>();
        private CompositeDisposable _CompositeDisposable = new CompositeDisposable();
        private int _Width;
        private int _Height;
        private double _CanvasBorderThickness;
        private bool _MiddleButtonIsPressed;
        private Point _MousePointerPosition;
        private bool disposedValue;

        public DelegateCommand<object> AddItemCommand { get; private set; }
        public DelegateCommand<object> RemoveItemCommand { get; private set; }
        public DelegateCommand<object> ClearSelectedItemsCommand { get; private set; }
        public DelegateCommand<object> CreateNewDiagramCommand { get; private set; }
        public DelegateCommand LoadCommand { get; private set; }
        public DelegateCommand<string> LoadFileCommand { get; private set; }
        public DelegateCommand SaveCommand { get; private set; }
        public DelegateCommand OverwriteCommand { get; private set; }
        public DelegateCommand ExportCommand { get; private set; }
        public DelegateCommand GroupCommand { get; private set; }
        public DelegateCommand UngroupCommand { get; private set; }
        public DelegateCommand BringForegroundCommand { get; private set; }
        public DelegateCommand BringForwardCommand { get; private set; }
        public DelegateCommand SendBackwardCommand { get; private set; }
        public DelegateCommand SendBackgroundCommand { get; private set; }
        public DelegateCommand AlignTopCommand { get; private set; }
        public DelegateCommand AlignVerticalCenterCommand { get; private set; }
        public DelegateCommand AlignBottomCommand { get; private set; }
        public DelegateCommand AlignLeftCommand { get; private set; }
        public DelegateCommand AlignHorizontalCenterCommand { get; private set; }
        public DelegateCommand AlignRightCommand { get; private set; }
        public DelegateCommand DistributeHorizontalCommand { get; private set; }
        public DelegateCommand DistributeVerticalCommand { get; private set; }
        public DelegateCommand SelectAllCommand { get; private set; }
        public DelegateCommand SettingCommand { get; private set; }
        public DelegateCommand UniformWidthCommand { get; private set; }
        public DelegateCommand UniformHeightCommand { get; private set; }
        public DelegateCommand DuplicateCommand { get; private set; }
        public DelegateCommand CutCommand { get; private set; }
        public DelegateCommand CopyCommand { get; private set; }
        public DelegateCommand PasteCommand { get; private set; }
        public DelegateCommand EditMenuOpenedCommand { get; private set; }
        public DelegateCommand UnionCommand { get; private set; }
        public DelegateCommand IntersectCommand { get; private set; }
        public DelegateCommand XorCommand { get; private set; }
        public DelegateCommand ExcludeCommand { get; private set; }
        public DelegateCommand ClipCommand { get; private set; }
        public DelegateCommand UndoCommand { get; private set; }
        public DelegateCommand RedoCommand { get; private set; }
        public DelegateCommand<MouseWheelEventArgs> MouseWheelCommand { get; private set; }
        public DelegateCommand<MouseEventArgs> PreviewMouseDownCommand { get; private set; }
        public DelegateCommand<MouseEventArgs> PreviewMouseUpCommand { get; private set; }
        public DelegateCommand<MouseEventArgs> MouseMoveCommand { get; private set; }
        public DelegateCommand<MouseEventArgs> MouseLeaveCommand { get; private set; }
        public DelegateCommand<MouseEventArgs> MouseEnterCommand { get; private set; }
        public DelegateCommand<KeyEventArgs> PreviewKeyDownCommand { get; private set; }
        public DelegateCommand PropertyCommand { get; private set; }
        public DelegateCommand<System.Windows.Shapes.Line> MouseDownStraightLineCommand { get; private set; }
        public DelegateCommand<System.Windows.Shapes.Path> MouseDownBezierCurveCommand { get; private set; }
        public DelegateCommand<System.Windows.Shapes.Path> MouseDownPolyBezierCommand { get; private set; }
        public DelegateCommand LoadedCommand { get; private set; }
        public DelegateCommand FitCanvasCommand { get; private set; }
        public DelegateCommand ClearCanvasCommand { get; private set; }
        public DelegateCommand VectorImagingCommand { get; private set; }

        #region Property

        public ReactivePropertySlim<LayerTreeViewItemBase> RootLayer { get; set; } = new ReactivePropertySlim<LayerTreeViewItemBase>(new LayerTreeViewItemBase());

        public ReactiveCollection<LayerTreeViewItemBase> Layers { get; set; }

        public ReadOnlyReactivePropertySlim<LayerTreeViewItemBase[]> SelectedLayers { get; set; }

        public ReadOnlyReactivePropertySlim<SelectableDesignerItemViewModelBase[]> AllItems { get; set; }

        public ReadOnlyReactivePropertySlim<SelectableDesignerItemViewModelBase[]> SelectedItems { get; set; }

        public ReactivePropertySlim<BackgroundViewModel> BackgroundItem { get; } = new ReactivePropertySlim<BackgroundViewModel>();

        public ReactivePropertySlim<double?> EdgeThickness { get; } = new ReactivePropertySlim<double?>();

        public ReactivePropertySlim<bool> EnableMiniMap { get; } = new ReactivePropertySlim<bool>();

        public ReactivePropertySlim<bool> EnableCombine { get; } = new ReactivePropertySlim<bool>();

        public ReactivePropertySlim<bool> EnableLayers { get; } = new ReactivePropertySlim<bool>();

        public ReactivePropertySlim<bool> EnableBrushThickness { get; } = new ReactivePropertySlim<bool>();

        public ReactivePropertySlim<string> FileName { get; } = new ReactivePropertySlim<string>();

        public ReactivePropertySlim<Brush> CanvasBackground { get; } = new ReactivePropertySlim<Brush>();

        public ReactivePropertySlim<bool> EnablePointSnap { get; } = new ReactivePropertySlim<bool>();

        public ReactivePropertySlim<bool> EnableAutoSave { get; } = new ReactivePropertySlim<bool>();

        public ReactivePropertySlim<DateTime> AutoSavedDateTime { get; } = new ReactivePropertySlim<DateTime>();

        public ReactivePropertySlim<AutoSaveType> AutoSaveType { get; } = new ReactivePropertySlim<AutoSaveType>();

        public ReactivePropertySlim<TimeSpan> AutoSaveInterval { get; } = new ReactivePropertySlim<TimeSpan>(TimeSpan.FromMinutes(1));

        public ReactiveCollection<string> AutoSaveFiles { get; set; } = new ReactiveCollection<string>();

        public ReactivePropertySlim<AngleType> AngleType { get; set; } = new ReactivePropertySlim<AngleType>();

        public ReactivePropertySlim<bool> EnableImageEmbedding { get; set; } = new ReactivePropertySlim<bool>();

        public ReactivePropertySlim<Visibility> ContextMenuVisibility { get; } = new ReactivePropertySlim<Visibility>(Visibility.Visible);

        public ReactivePropertySlim<ColorSpots> ColorSpots { get; } = new ReactivePropertySlim<ColorSpots>();

        public ReactivePropertySlim<Brush> EdgeBrush { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> FillBrush { get; } = new ReactivePropertySlim<Brush>();

        public ReactiveCollection<MenuItem> ContextMenuItems { get; } = new ReactiveCollection<MenuItem>();

        public int Width
        {
            get { return _Width; }
            set { SetProperty(ref _Width, value); }
        }

        public int Height
        {
            get { return _Height; }
            set { SetProperty(ref _Height, value); }
        }

        /// <summary>
        /// 現在ポインティングしている座標
        /// ステータスバー上の座標インジケーターに使用される
        /// </summary>
        public Point CurrentPoint
        {
            get { return _CurrentPoint; }
            set { SetProperty(ref _CurrentPoint, value); }
        }
        public double CanvasBorderThickness
        {
            get { return _CanvasBorderThickness; }
            set { SetProperty(ref _CanvasBorderThickness, value); }
        }

        public double ScaleX { get; set; } = 1.0;
        public double ScaleY { get; set; } = 1.0;
        public System.Version BGSXFileVersion { get; } = new System.Version(2, 4);

        public int LayerCount { get; set; } = 1;

        public int LayerItemCount { get; set; } = 1;

        public IEnumerable<Tuple<SnapPoint, Point>> SnapPoints
        {
            get
            {
                var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
                var resizeThumbs = designerCanvas.EnumerateChildOfType<SnapPoint>();
                var sets = resizeThumbs
                                .Select(x => new Tuple<SnapPoint, Point>(x, GetCenter(x)))
                                .Distinct();
                return sets;
            }
        }

        public IEnumerable<Tuple<SnapPoint, Point>> GetSnapPoints(IEnumerable<SnapPoint> exceptSnapPoints)
        {
            var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            var resizeThumbs = designerCanvas.EnumerateChildOfType<SnapPoint>();
            var sets = resizeThumbs
                            .Where(x => !exceptSnapPoints.Contains(x))
                            .Select(x => new Tuple<SnapPoint, Point>(x, GetCenter(x)))
                            .Distinct();
            return sets;
        }

        public IEnumerable<Tuple<SnapPoint, Point>> GetSnapPoints(Point exceptPoint)
        {
            var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            var resizeThumbs = designerCanvas.EnumerateChildOfType<SnapPoint>();
            var sets = resizeThumbs
                            .Where(x => x.InputHitTest(exceptPoint) == null)
                            .Select(x => new Tuple<SnapPoint, Point>(x, GetCenter(x)))
                            .Distinct();
            return sets;
        }

        #endregion //Property

        public DiagramViewModel(MainWindowViewModel mainWindowViewModel, int width, int height, bool isPreview = false)
        {
            MainWindowVM = mainWindowViewModel;

            if (!isPreview)
            {
                AddItemCommand = new DelegateCommand<object>(p => ExecuteAddItemCommand(p));
                RemoveItemCommand = new DelegateCommand<object>(p => ExecuteRemoveItemCommand(p));
                ClearSelectedItemsCommand = new DelegateCommand<object>(p => ExecuteClearSelectedItemsCommand(p));
                CreateNewDiagramCommand = new DelegateCommand<object>(p => ExecuteCreateNewDiagramCommand(p));
                SelectAllCommand = new DelegateCommand(() => ExecuteSelectAllCommand());
                ClipCommand = new DelegateCommand(() => ExecuteClipCommand(), () => CanExecuteClip());
                UndoCommand = new DelegateCommand(() => ExecuteUndoCommand(), () => CanExecuteUndo());
                RedoCommand = new DelegateCommand(() => ExecuteRedoCommand(), () => CanExecuteRedo());
                MouseWheelCommand = new DelegateCommand<MouseWheelEventArgs>(args =>
                {
                    LogManager.GetCurrentClassLogger().Trace($"MouseWheelCommand");
                    var diagramControl = App.Current.MainWindow.GetChildOfType<DiagramControl>();
                    var zoomBox = diagramControl.GetChildOfType<ZoomBox>();
                    if (args.Delta > 0)
                        zoomBox.ZoomSliderPlus();
                    else if (args.Delta < 0)
                        zoomBox.ZoomSliderMinus();
                    args.Handled = true;
                });
                PreviewMouseDownCommand = new DelegateCommand<MouseEventArgs>(args =>
                {
                    LogManager.GetCurrentClassLogger().Trace($"PreviewMouseDownCommand");
                    if (args.MiddleButton == MouseButtonState.Pressed)
                    {
                        _MiddleButtonIsPressed = true;
                        var diagramControl = App.Current.MainWindow.GetChildOfType<DiagramControl>();
                        _MousePointerPosition = args.GetPosition(diagramControl);
                        diagramControl.Cursor = Cursors.SizeAll;
                    }
                });
                PreviewMouseUpCommand = new DelegateCommand<MouseEventArgs>(args =>
                {
                    LogManager.GetCurrentClassLogger().Trace($"PreviewMouseUpCommand");
                    ReleaseMiddleButton(args);
                });
                MouseMoveCommand = new DelegateCommand<MouseEventArgs>(args =>
                {
                    LogManager.GetCurrentClassLogger().Trace($"MouseMoveCommand");
                    if (_MiddleButtonIsPressed)
                    {
                        var diagramControl = App.Current.MainWindow.GetChildOfType<DiagramControl>();
                        var scrollViewer = diagramControl.GetChildOfType<ScrollViewer>();
                        var newMousePointerPosition = args.GetPosition(diagramControl);
                        var diff = newMousePointerPosition - _MousePointerPosition;
                        scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - diff.Y);
                        scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - diff.X);
                        _MousePointerPosition = newMousePointerPosition;
                    }
                });
                MouseLeaveCommand = new DelegateCommand<MouseEventArgs>(args =>
                {
                    LogManager.GetCurrentClassLogger().Trace($"MouseLeaveCommand");
                    if (_MiddleButtonIsPressed)
                    {
                        ReleaseMiddleButton(args);
                    }
                });
                MouseEnterCommand = new DelegateCommand<MouseEventArgs>(args =>
                {
                    LogManager.GetCurrentClassLogger().Trace($"MouseEnterCommand");
                    if (_MiddleButtonIsPressed)
                    {
                        ReleaseMiddleButton(args);
                    }
                });
                PreviewKeyDownCommand = new DelegateCommand<KeyEventArgs>(args =>
                {
                    LogManager.GetCurrentClassLogger().Trace($"PreviewKeyDownCommand");
                    switch (args.Key)
                    {
                        case Key.Left:
                            MoveSelectedItems(-1, 0);
                            args.Handled = true;
                            break;
                        case Key.Up:
                            MoveSelectedItems(0, -1);
                            args.Handled = true;
                            break;
                        case Key.Right:
                            MoveSelectedItems(1, 0);
                            args.Handled = true;
                            break;
                        case Key.Down:
                            MoveSelectedItems(0, 1);
                            args.Handled = true;
                            break;
                    }
                });
                EditMenuOpenedCommand = new DelegateCommand(() =>
                {
                    LogManager.GetCurrentClassLogger().Trace($"EditMenuOpenedCommand");
                    CutCommand.RaiseCanExecuteChanged();
                    CopyCommand.RaiseCanExecuteChanged();
                    PasteCommand.RaiseCanExecuteChanged();
                });
                LoadedCommand = new DelegateCommand(() =>
                {
                    LogManager.GetCurrentClassLogger().Trace($"LoadedCommand");
                    //var filename = @"Z:\Git\boilersGraphics\boilersGraphics.Test\bin\Debug\XmlFiles\checker_pattern.xml";
                    //ExecuteLoadCommand(filename, false);
                    //BackgroundItem.Value.FillColor.Value = Colors.Red;
                });
                FitCanvasCommand = new DelegateCommand(() =>
                {
                    LogManager.GetCurrentClassLogger().Trace($"FitCanvasCommand");
                    double horizontalGap = AllItems.Value.OfType<DesignerItemViewModelBase>().Except(new DesignerItemViewModelBase[] { BackgroundItem.Value }).Count() > 0
                                         ? AllItems.Value.OfType<DesignerItemViewModelBase>().Except(new DesignerItemViewModelBase[] { BackgroundItem.Value }).Min(x => x.Left.Value)
                                         : 0;
                    double verticalGap = AllItems.Value.OfType<DesignerItemViewModelBase>().Except(new DesignerItemViewModelBase[] { BackgroundItem.Value }).Count() > 0
                                       ? AllItems.Value.OfType<DesignerItemViewModelBase>().Except(new DesignerItemViewModelBase[] { BackgroundItem.Value }).Min(x => x.Top.Value)
                                       : 0;

                    foreach (var item in AllItems.Value.OfType<DesignerItemViewModelBase>().Except(new DesignerItemViewModelBase[] { BackgroundItem.Value }))
                    {
                        item.Left.Value += -horizontalGap;
                        item.Top.Value += -verticalGap;
                    }

                    double horizontalMax = AllItems.Value.OfType<DesignerItemViewModelBase>().Except(new DesignerItemViewModelBase[] { BackgroundItem.Value }).Count() > 0
                                         ? AllItems.Value.OfType<DesignerItemViewModelBase>().Except(new DesignerItemViewModelBase[] { BackgroundItem.Value }).Max(x => x.Right.Value)
                                         : 0;
                    double verticalMax = AllItems.Value.OfType<DesignerItemViewModelBase>().Except(new DesignerItemViewModelBase[] { BackgroundItem.Value }).Count() > 0
                                         ? AllItems.Value.OfType<DesignerItemViewModelBase>().Except(new DesignerItemViewModelBase[] { BackgroundItem.Value }).Max(x => x.Bottom.Value)
                                         : 0;

                    Width = (int)Math.Round(horizontalMax);
                    Height = (int)Math.Round(verticalMax);
                    BackgroundItem.Value.Width.Value = Width;
                    BackgroundItem.Value.Height.Value = Height;
                }, () => AllItems.Value.OfType<DesignerItemViewModelBase>().Except(new DesignerItemViewModelBase[] { BackgroundItem.Value }).Count() > 0);
                ClearCanvasCommand = new DelegateCommand(() =>
                {
                    InitialSetting(mainWindowViewModel, true, false, false);
                });
                VectorImagingCommand = new DelegateCommand(() =>
                {
                    var selectedItem = SelectedItems.Value.First() as PictureDesignerItemViewModel;
                    Remove(selectedItem);
                    using (OpenCvSharp.Mat target = EnableImageEmbedding.Value && selectedItem.EmbeddedImage.Value != null ? selectedItem.EmbeddedImage.Value.ToMat() : new OpenCvSharp.Mat(selectedItem.FileName))
                    using (OpenCvSharp.Mat output = new OpenCvSharp.Mat())
                    {
                        if (target.Type() == OpenCvSharp.MatType.CV_8UC3)
                        {
                            OpenCvSharp.Cv2.CvtColor(target, target, OpenCvSharp.ColorConversionCodes.BGR2BGRA);
                        }
                        const int MAX_CLUSTERS = 8;
                        Kmeans(target, output, MAX_CLUSTERS, out var sets);
                        SetAlpha255(output);
                        var bag = new ConcurrentBag<SelectableDesignerItemViewModelBase>();
                        ParallelOptions options = new ParallelOptions();
                        options.MaxDegreeOfParallelism = Environment.ProcessorCount;
                        Parallel.For(0, sets.Count(), options, i =>
                        //for (int i = 0; i < sets.Count(); ++i)
                        {
                            Stopwatch sw = Stopwatch.StartNew();
                            Debug.WriteLine($"{i} / {sets.Count()} BEGIN PROCESS");
                            var color = sets.ElementAt(i);
                            //extract
                            var extracted = ExtractColor(output, color);
                            //grayscale
                            using (var grayscaled = new OpenCvSharp.Mat())
                            {
                                OpenCvSharp.Cv2.CvtColor(extracted, grayscaled, OpenCvSharp.ColorConversionCodes.BGRA2GRAY);
                                //threshold
                                using (var thresholded = new OpenCvSharp.Mat())
                                {
                                    OpenCvSharp.Cv2.Threshold(grayscaled, thresholded, 128, 255, OpenCvSharp.ThresholdTypes.Otsu);
                                    //OpenCvSharp.Cv2.ImShow("TEST", thresholded);
                                    //findcontours
                                    OpenCvSharp.Cv2.FindContours(thresholded, out var contours, out var hierarchy, OpenCvSharp.RetrievalModes.List, OpenCvSharp.ContourApproximationModes.ApproxNone);
                                    //Parallel.For(0, contours.Count(), j =>
                                    for (int j = 0; j < contours.Count(); ++j)
                                    {
                                        Stopwatch sw1 = Stopwatch.StartNew();
                                        Debug.WriteLine($"{i} {j}/{contours.Count()} BEGIN");
                                        var array = contours[j];
                                        var polyBezier = new PolyBezierViewModel();
                                        polyBezier.Owner = this;
                                        for (int k = 0; k < array.Count(); k++)
                                        {
                                            var contour = array[k];
                                            polyBezier.Points.Add(new Point(contour.X, contour.Y));
                                        }
                                        //polyBezier.Points.AddRange(array.Select(p => new Point(p.X, p.Y)));
                                        polyBezier.EdgeBrush.Value = Brushes.Transparent;
                                        polyBezier.EdgeThickness.Value = 0;
                                        polyBezier.LeftTop.Value = new Point(polyBezier.Points.Select(x => x.X).Min() - polyBezier.Owner.EdgeThickness.Value.Value / 2, polyBezier.Points.Select(x => x.Y).Min() - polyBezier.Owner.EdgeThickness.Value.Value / 2);
                                        polyBezier.ZIndex.Value = Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children.Value).Count();
                                        polyBezier.IsSelected.Value = true;
                                        polyBezier.IsVisible.Value = true;
                                        var union = Combine(GeometryCombineMode.Union, polyBezier);
                                        var fillbrush = new SolidColorBrush(Color.FromRgb(color.Item2, color.Item1, color.Item0));
                                        fillbrush.Freeze();
                                        union.FillBrush.Value = fillbrush;
                                        union.IsSelected.Value = false;
                                        union.PathGeometry.Value.Freeze();
                                        bag.Add(union);
                                        sw1.Stop();
                                        Debug.WriteLine($"{i} {j}/{contours.Count()} END {sw1.ElapsedMilliseconds}ms");
                                    }
                                }
                            }
                            sw.Stop();
                            Debug.WriteLine($"{i} / {sets.Count()} end process {sw.ElapsedMilliseconds}ms");
                        });
                        List<LayerTreeViewItemBase> l = new List<LayerTreeViewItemBase>();
                        while (bag.TryTake(out var item))
                        {
                            Stopwatch sw = Stopwatch.StartNew();
                            Debug.WriteLine($"adding item. remain:{bag.Count()}");
                            var i = new LayerItem(item.Clone() as SelectableDesignerItemViewModelBase, SelectedLayers.Value.First(), Name.GetNewLayerItemName(this));
                            i.Color.Value = Randomizer.RandomColor(new Random());
                            i.IsVisible.Value = true;
                            i.IsSelected.Value = false;
                            l.Add(i);
                            sw.Stop();
                            Debug.WriteLine($"added item. {sw.ElapsedMilliseconds}ms");
                        }
                        DisposeProperties();
                        InitializeProperties_Layers(isPreview);
                        AddNewLayer(mainWindowViewModel, false);
                        var firstSelectedLayer = SelectedLayers.Value.First();
                        firstSelectedLayer.Children.Value = new ObservableCollection<LayerTreeViewItemBase>(l);
                        InitializeProperties_Items(isPreview);
                        SetSubscribes(false);
                    }
                });
            }

            DisposeProperties();
            InitializeProperties_Layers(isPreview);
            InitializeProperties_Items(isPreview);
            SetSubscribes(isPreview);

            Width = width;
            Height = height;

            if (!isPreview)
            {
                EnableAutoSave.Subscribe(x =>
                {
                    if (!x && _AutoSaveTimerDisposableObj != null)
                        _AutoSaveTimerDisposableObj.Dispose();
                })
                .AddTo(_CompositeDisposable);
                EnableAutoSave.Value = true;
                AutoSaveType.Value = Models.AutoSaveType.SetInterval;
                AutoSaveInterval.Value = TimeSpan.FromSeconds(30);

                var id = Guid.Parse("00000000-0000-0000-0000-000000000000");
                var dao = new LogSettingDao();
                var logSettings = dao.FindBy(new Dictionary<string, object>() { { "ID", id } });
                if (logSettings.Count() == 0)
                {
                    var newLogSetting = new Models.LogSetting();
                    newLogSetting.ID = id;
                    newLogSetting.LogLevel = NLog.LogLevel.Info.ToString();
                    dao.Insert(newLogSetting);
                }
                logSettings = dao.FindBy(new Dictionary<string, object>() { { "ID", id } });
                var logSetting = logSettings.First();
                MainWindowVM.LogLevel.Value = NLog.LogLevel.FromString(logSetting.LogLevel);
                PackAutoSaveFiles();
            }

            AngleType.Value = Helpers.AngleType.Minus180To180;
            EnableImageEmbedding.Value = true;
            ColorSpots.Value = new ColorSpots();
            EnableCombine.Value = true;
            EnableLayers.Value = true;

            SettingIfDebug();
        }

        public void InitializeProperties_Layers(bool isPreview)
        {
            RootLayer.Value = new LayerTreeViewItemBase();
            SetLayers();

            if (!isPreview)
            {
                SetSelectedLayers();
            }
        }

        public void InitializeProperties_Items(bool isPreview)
        {
            SetAllItems();

            if (!isPreview)
            {
                SetSelectedItems();
            }
        }

        public void SetSubscribes(bool isPreview)
        {
            if (!isPreview)
            {
                SetAllItemsSubscribe();
                SetSelectedItemsSubscribe();
                SetSelectedLayersSubscribe();
                SetLayersObserveAddChanged();
            }
        }

        public void DisposeProperties()
        {
            if (Layers != null)
                Layers.Dispose();
            if (AllItems != null)
                AllItems.Dispose();
            if (SelectedItems != null)
                SelectedItems.Dispose();
            if (SelectedLayers != null)
                SelectedLayers.Dispose();
        }

        private void SetLayersObserveAddChanged()
        {
            Layers.ObserveAddChanged()
                  .Subscribe(x =>
                  {
                      RootLayer.Value.Children.Value = new ObservableCollection<LayerTreeViewItemBase>(Layers.Cast<LayerTreeViewItemBase>());
                      x.SetParentToChildren(RootLayer.Value);
                  })
                  .AddTo(_CompositeDisposable);
        }

        private void SetSelectedLayersSubscribe()
        {
            SelectedLayers.Subscribe(x =>
            {
                LogManager.GetCurrentClassLogger().Trace($"SelectedLayers changed {string.Join(", ", x.Select(x => x.ToString()))}");
            })
            .AddTo(_CompositeDisposable);
        }

        private void SetSelectedLayers()
        {
            SelectedLayers = Layers.ObserveElementObservableProperty(x => x.IsSelected)
                                   .Select(_ => Layers.Where(x => x.IsSelected.Value == true).ToArray())
                                   .ToReadOnlyReactivePropertySlim(Array.Empty<LayerTreeViewItemBase>());
        }

        private void SetSelectedItemsSubscribe()
        {
            SelectedItems.Subscribe(selectedItems =>
            {
                LogManager.GetCurrentClassLogger().Debug($"SelectedItems changed {string.Join(", ", selectedItems.Select(x => x?.ToString() ?? "null"))}");

                ReallocateContextMenuItems();
            })
                            .AddTo(_CompositeDisposable);
        }

        private void SetSelectedItems()
        {
            SelectedItems = Layers.CollectionChangedAsObservable()
                                  .Select(_ =>
                                      Layers
                                          .Select(x => x.SelectedLayerItemsChangedAsObservable())
                                          .Merge()
                                  )
                                  .Switch()
                                  .Do(x => LogManager.GetCurrentClassLogger().Debug("SelectedItems updated"))
                                  .Select(_ => Layers
                                      .SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children.Value)
                                      .OfType<LayerItem>()
                                      .Select(y => y.Item.Value)
                                      .Where(z => z.IsSelected.Value == true)
                                      .Except(Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children.Value)
                                          .OfType<LayerItem>()
                                          .Select(y => y.Item.Value)
                                          .OfType<ConnectorBaseViewModel>())
                                      .Where(z => z.IsSelected.Value == true)
                                      .OrderBy(z => z.SelectedOrder.Value)
                                      .ToArray()
                                  ).ToReadOnlyReactivePropertySlim(Array.Empty<SelectableDesignerItemViewModelBase>());
        }

        private void SetAllItemsSubscribe()
        {
            AllItems.Subscribe(x =>
            {
                FitCanvasCommand.RaiseCanExecuteChanged();
                LogManager.GetCurrentClassLogger().Trace($"{x.Length} items in AllItems.");
                LogManager.GetCurrentClassLogger().Trace(string.Join(", ", x.Select(y => y?.ToString() ?? "null")));
            })
            .AddTo(_CompositeDisposable);
        }

        private void SetAllItems()
        {
            AllItems = Layers.CollectionChangedAsObservable()
                             .Select(_ => Layers.Select(x => x.LayerItemsChangedAsObservable()).Merge()
                                                .Merge(this.ObserveProperty(y => y.BackgroundItem.Value).ToUnit()))
                             .Switch()
                             .Do(_ => Debug.WriteLine(String.Concat("debug ", string.Join(", ", Layers.Select(x => x?.ToString() ?? "null")))))
                             .Select(_ => Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children.Value)
                                                .Where(x => x.GetType() == typeof(LayerItem))
                                                .Select(y => (y as LayerItem).Item.Value)
                                                .Union(new SelectableDesignerItemViewModelBase[] { BackgroundItem.Value })
                                                .ToArray())
                             .ToReadOnlyReactivePropertySlim(Array.Empty<SelectableDesignerItemViewModelBase>());
        }

        private void SetLayers()
        {
            Layers = RootLayer.Value.Children.Value.CollectionChangedAsObservable()
                                             .Select(_ => RootLayer.Value.LayerChangedAsObservable())
                                             .Switch()
                                             .SelectMany(_ => RootLayer.Value.Children.Value)
                                             .ToReactiveCollection();
        }

        private static unsafe OpenCvSharp.Mat ExtractColor(OpenCvSharp.Mat output, OpenCvSharp.Vec3b color)
        {
            var ret = output.Clone();
            Debug.Assert(output.Type() == OpenCvSharp.MatType.CV_8UC4);
            Parallel.For(0, ret.Height, y =>
            {
                byte* p = (byte*)ret.Ptr(y);
                for (int x = 0; x < ret.Width; ++x)
                {
                    var b = *(p + x * 4 + 0);
                    var g = *(p + x * 4 + 1);
                    var r = *(p + x * 4 + 2);
                    var a = *(p + x * 4 + 3);
                    if (b == color.Item0 && g == color.Item1 && r == color.Item2)
                    {
                        *(p + x * 4 + 3) = 255;
                    }
                    else
                    {
                        *(p + x * 4 + 3) = 0;
                    }
                }
            });
            return ret;
        }

        private static unsafe void SetAlpha255(OpenCvSharp.Mat output)
        {
            Debug.Assert(output.Type() == OpenCvSharp.MatType.CV_8UC4);
            for (int y = 0; y < output.Height; y++)
            {
                byte* p = (byte*)output.Ptr(y);
                for (int x = 0; x < output.Width; x++)
                {
                    *(p + x * 4 + 3) = 255;
                }
            }
        }

        public static BitmapImage ConvertWriteableBitmapToBitmapImage(WriteableBitmap wbm)
        {
            BitmapImage bmImage = new BitmapImage();
            using (MemoryStream stream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(wbm));
                encoder.Save(stream);
                bmImage.BeginInit();
                bmImage.CacheOption = BitmapCacheOption.OnLoad;
                bmImage.StreamSource = stream;
                bmImage.EndInit();
                bmImage.Freeze();
            }
            return bmImage;
        }

        /// <summary>
        /// Color Quantization using K-Means Clustering in OpenCVSharp.
        /// The process of Color Quantization is used for reducing the number of colors in an image.
        /// </summary>
        /// <param name="input">Input image.</param>
        /// <param name="output">Output image applying the number of colors defined for required clusters.</param>
        /// <param name="k">Number of clusters required.</param>
        public static void Kmeans(OpenCvSharp.Mat input, OpenCvSharp.Mat output, int k, out HashSet<OpenCvSharp.Vec3b> sets)
        {
            using (OpenCvSharp.Mat points = new OpenCvSharp.Mat())
            {
                using (OpenCvSharp.Mat labels = new OpenCvSharp.Mat())
                {
                    using (OpenCvSharp.Mat centers = new OpenCvSharp.Mat())
                    {
                        int width = input.Cols;
                        int height = input.Rows;

                        points.Create(width * height, 1, OpenCvSharp.MatType.CV_32FC3);
                        centers.Create(k, 1, points.Type());
                        output.Create(height, width, input.Type());

                        // Input Image Data
                        Parallel.For(0, height, y =>
                        {
                            for (int x = 0; x < width; x++)
                            {
                                var i = y * width + x;
                                OpenCvSharp.Vec3f vec3f = new OpenCvSharp.Vec3f
                                {
                                    Item0 = input.At<OpenCvSharp.Vec3b>(y, x).Item0,
                                    Item1 = input.At<OpenCvSharp.Vec3b>(y, x).Item1,
                                    Item2 = input.At<OpenCvSharp.Vec3b>(y, x).Item2
                                };
                                points.Set<OpenCvSharp.Vec3f>(i, vec3f);
                            }
                        });

                        // Criteria:
                        // – Stop the algorithm iteration if specified accuracy, epsilon, is reached.
                        // – Stop the algorithm after the specified number of iterations, MaxIter.
                        var criteria = new OpenCvSharp.TermCriteria(type: OpenCvSharp.CriteriaTypes.Eps | OpenCvSharp.CriteriaTypes.MaxIter, maxCount: 10, epsilon: 1.0);

                        // Finds centers of clusters and groups input samples around the clusters.
                        OpenCvSharp.Cv2.Kmeans(data: points, k: k, bestLabels: labels, criteria: criteria, attempts: 3, flags: OpenCvSharp.KMeansFlags.PpCenters, centers: centers);

                        var ret = new ConcurrentBag<OpenCvSharp.Vec3b>();

                        // Output Image Data
                        Parallel.For(0, height, y =>
                        {
                            for (int x = 0; x < width; x++)
                            {
                                var i = y * width + x;
                                int index = labels.Get<int>(i);

                                OpenCvSharp.Vec3b vec3b = new OpenCvSharp.Vec3b();

                                int firstComponent = Convert.ToInt32(Math.Round(centers.At<OpenCvSharp.Vec3f>(index).Item0));
                                firstComponent = firstComponent > 255 ? 255 : firstComponent < 0 ? 0 : firstComponent;
                                vec3b.Item0 = Convert.ToByte(firstComponent);

                                int secondComponent = Convert.ToInt32(Math.Round(centers.At<OpenCvSharp.Vec3f>(index).Item1));
                                secondComponent = secondComponent > 255 ? 255 : secondComponent < 0 ? 0 : secondComponent;
                                vec3b.Item1 = Convert.ToByte(secondComponent);

                                int thirdComponent = Convert.ToInt32(Math.Round(centers.At<OpenCvSharp.Vec3f>(index).Item2));
                                thirdComponent = thirdComponent > 255 ? 255 : thirdComponent < 0 ? 0 : thirdComponent;
                                vec3b.Item2 = Convert.ToByte(thirdComponent);

                                output.Set<OpenCvSharp.Vec3b>(y, x, vec3b);
                                ret.Add(vec3b);
                            }
                        });
                        sets = new HashSet<OpenCvSharp.Vec3b>(ret);
                    }
                }
            }
        }

        private void ReallocateContextMenuItems()
        {
            if (App.IsTest)
                return;
            var diagramControl = App.Current.MainWindow.GetCorrespondingViews<DiagramControl>(this).FirstOrDefault();
            ContextMenuItems.Clear();
            ContextMenuItems.Add(new MenuItem()
            {
                Command = PropertyCommand,
                Header = Resources.MenuItem_Property
            });
            if (SelectedItems.Value.Count() > 0 && SelectedItems.Value.First() is PictureDesignerItemViewModel)
            {
                ContextMenuItems.Add(new MenuItem()
                {
                    Command = VectorImagingCommand,
                    Header = "ベクターオブジェクトに変換"
                });
            }
            var grouping = new MenuItem()
            {
                Header = Resources.Grouping
            };
            grouping.Items.Add(new MenuItem()
            {
                Header = Resources.Command_Group,
                Command = GroupCommand,
                Icon = new Image() { Source = diagramControl != null ? (ImageSource)diagramControl.FindResource("Icon_Group") : null }
            });
            grouping.Items.Add(new MenuItem()
            {
                Header = Resources.Command_Ungroup,
                Command = UngroupCommand,
                Icon = new Image() { Source = diagramControl != null ? (ImageSource)diagramControl.FindResource("Icon_Ungroup") : null }
            });
            ContextMenuItems.Add(grouping);
            var ordering = new MenuItem()
            {
                Header = Resources.Ordering
            };
            ordering.Items.Add(new MenuItem()
            {
                Header = Resources.Command_BringForeground,
                Command = BringForegroundCommand,
                Icon = new Image() { Source = diagramControl != null ? (ImageSource)diagramControl.FindResource("Icon_BringToFront") : null }
            });
            ordering.Items.Add(new MenuItem()
            {
                Header = Resources.Command_BringForward,
                Command = BringForwardCommand,
                Icon = new Image() { Source = diagramControl != null ? (ImageSource)diagramControl.FindResource("Icon_BringForward") : null }
            });
            ordering.Items.Add(new MenuItem()
            {
                Header = Resources.Command_SendBackward,
                Command = SendBackwardCommand,
                Icon = new Image() { Source = diagramControl != null ? (ImageSource)diagramControl.FindResource("Icon_SendBackward") : null }
            });
            ordering.Items.Add(new MenuItem()
            {
                Header = Resources.Command_SendBackground,
                Command = SendBackgroundCommand,
                Icon = new Image() { Source = diagramControl != null ? (ImageSource)diagramControl.FindResource("Icon_SendToBack") : null }
            });
            ContextMenuItems.Add(ordering);
            var alignment = new MenuItem()
            {
                Header = Resources.Alignment
            };
            alignment.Items.Add(new MenuItem()
            {
                Header = Resources.Command_AlignTop,
                Command = AlignTopCommand,
                Icon = new Image() { Source = diagramControl != null ? (ImageSource)diagramControl.FindResource("Icon_AlignTop") : null }
            });
            alignment.Items.Add(new MenuItem()
            {
                Header = Resources.Command_AlignVerticalCenter,
                Command = AlignVerticalCenterCommand,
                Icon = new Image() { Source = diagramControl != null ? (ImageSource)diagramControl.FindResource("Icon_AlignCenteredVertical") : null }
            });
            alignment.Items.Add(new MenuItem()
            {
                Header = Resources.Command_AlignBottom,
                Command = AlignBottomCommand,
                Icon = new Image() { Source = diagramControl != null ? (ImageSource)diagramControl.FindResource("Icon_AlignBottom") : null }
            });
            alignment.Items.Add(new MenuItem()
            {
                Header = Resources.Command_AlignLeft,
                Command = AlignLeftCommand,
                Icon = new Image() { Source = diagramControl != null ? (ImageSource)diagramControl.FindResource("Icon_AlignLeft") : null }
            });
            alignment.Items.Add(new MenuItem()
            {
                Header = Resources.Command_AlignHorizontalCenter,
                Command = AlignHorizontalCenterCommand,
                Icon = new Image() { Source = diagramControl != null ? (ImageSource)diagramControl.FindResource("Icon_AlignCenteredHorizontal") : null }
            });
            alignment.Items.Add(new MenuItem()
            {
                Header = Resources.Command_AlignRight,
                Command = AlignRightCommand,
                Icon = new Image() { Source = diagramControl != null ? (ImageSource)diagramControl.FindResource("Icon_AlignRight") : null }
            });
            alignment.Items.Add(new MenuItem()
            {
                Header = Resources.Command_DistributeHorizontal,
                Command = DistributeHorizontalCommand,
                Icon = new Image() { Source = diagramControl != null ? (ImageSource)diagramControl.FindResource("Icon_DistributeHorizontal") : null }
            });
            alignment.Items.Add(new MenuItem()
            {
                Header = Resources.Command_DistributeVertical,
                Command = DistributeVerticalCommand,
                Icon = new Image() { Source = diagramControl != null ? (ImageSource)diagramControl.FindResource("Icon_DistributeVertical") : null }
            });
            ContextMenuItems.Add(alignment);
        }

        [Conditional("DEBUG")]
        private void SettingIfDebug()
        {
            EnableAutoSave.Value = false;
        }

        private void PackAutoSaveFiles()
        {
            if (AutoSaveFiles != null)
                AutoSaveFiles.ClearOnScheduler();
            try
            {
                var files = Directory.EnumerateFiles(System.IO.Path.Combine(boilersGraphics.Helpers.Path.GetRoamingDirectory(), "dhq_boiler\\boilersGraphics\\AutoSave"), "AutoSave-*-*-*-*-*-*.xml");
                foreach (var file in files.OrderByDescending(x => new FileInfo(x).LastWriteTime))
                {
                    AutoSaveFiles.AddOnScheduler(file);
                }
            }
            catch (DirectoryNotFoundException)
            {
                //Ignore it as it only happens on Azure DevOps
            }
        }

        private void MoveSelectedItems(int horizontalDiff, int verticalDiff)
        {
            MainWindowVM.Recorder.BeginRecode();
            SelectedItems.Value.OfType<DesignerItemViewModelBase>().ToList().ForEach(x =>
            {
                MainWindowVM.Recorder.Current.ExecuteSetProperty(x, "Left.Value", x.Left.Value + horizontalDiff);
                MainWindowVM.Recorder.Current.ExecuteSetProperty(x, "Top.Value", x.Top.Value + verticalDiff);
            });
            MainWindowVM.Recorder.EndRecode();
        }

        private IDisposable _AutoSaveTimerDisposableObj;

        public void Initialize(bool isPreview = false)
        {
            MainWindowVM.Recorder.BeginRecode();

            InitialSetting(MainWindowVM, true, true, isPreview);

            MainWindowVM.Recorder.EndRecode();

            MainWindowVM.Controller.Flush();
        }

        private void UpdateStatisticsCountAutoSave()
        {
            var statistics = MainWindowVM.Statistics.Value;
            statistics.NumberOfTimesAutomaticallySaved++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        public LayerTreeViewItemBase GetLayerTreeViewItemBase(SelectableDesignerItemViewModelBase item)
        {
            return Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children.Value)
                         .Where(x => x is LayerItem)
                         .First(x => (x as LayerItem).Item.Value == item);
        }

        private Point GetCenter(SnapPoint snapPoint)
        {
            var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            var leftTop = snapPoint.TransformToAncestor(designerCanvas).Transform(new Point(0, 0));
            switch (snapPoint.Tag)
            {
                case "左上":
                    return new Point(leftTop.X + snapPoint.Width - 1, leftTop.Y + snapPoint.Height - 1);
                case "右上":
                    return new Point(leftTop.X + 1, leftTop.Y + snapPoint.Height - 1);
                case "左下":
                    return new Point(leftTop.X + snapPoint.Width - 1, leftTop.Y + 1);
                case "右下":
                    return new Point(leftTop.X + 1, leftTop.Y + 1);
                case "左":
                case "上":
                case "右":
                case "下":
                    return new Point(leftTop.X, leftTop.Y);
                case "中央":
                    return new Point(leftTop.X + snapPoint.Width / 2, leftTop.Y + snapPoint.Height / 2);
                case "始点":
                case "終点":
                case "制御点":
                case "独立点":
                    return new Point(leftTop.X + snapPoint.Width / 2, leftTop.Y + snapPoint.Height / 2);
                case "頂点":
                    return new Point(leftTop.X + snapPoint.Width / 2, leftTop.Y + snapPoint.Height / 2);
                default:
                    throw new Exception("ResizeThumb.Tag doesn't set");
            }
        }

        [Conditional("DEBUG")]
        private void DebugPrint(int width, int height, IEnumerable<Tuple<SnapPoint, Point>> sets)
        {
            var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            var rtb = new RenderTargetBitmap((int)designerCanvas.ActualWidth, (int)designerCanvas.ActualHeight, 96, 96, PixelFormats.Pbgra32);

            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
            {
                VisualBrush brush = new VisualBrush(designerCanvas);
                context.DrawRectangle(brush, null, new Rect(new Point(), new Size(designerCanvas.Width, designerCanvas.Height)));

                Random rand = new Random();
                foreach (var set in sets)
                {
                    context.DrawText(new FormattedText((string)set.Item1.Tag, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("メイリオ"), 12, Randomizer.RandomColorBrush(rand), VisualTreeHelper.GetDpi(designerCanvas).PixelsPerDip), set.Item2);
                    context.DrawEllipse(Brushes.Red, new Pen(Brushes.Red, 1), set.Item2, 2, 2);
                }
            }

            rtb.Render(visual);

            OpenCvSharpHelper.ImShow("DebugPrint", rtb);
        }

        private void InitialSetting(MainWindowViewModel mainwindowViewModel, bool addingLayer = false, bool initCanvasBackground = false, bool isPreview = false)
        {
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "EdgeBrush.Value", Brushes.Black as Brush);
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "FillBrush.Value", Brushes.White as Brush);
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "EdgeThickness.Value", 1.0);
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "CanvasBorderThickness", 0.0);
            if (initCanvasBackground)
            {
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "CanvasBackground.Value", Brushes.White as Brush);
            }
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value", new BackgroundViewModel());
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.ZIndex.Value", -1);
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.FillBrush.Value", CanvasBackground.Value);
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.Left.Value", 0d);
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.Top.Value", 0d);
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.Width.Value", (double)Width);
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.Height.Value", (double)Height);
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.Owner", this);
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.EdgeBrush.Value", Brushes.Black as Brush);
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.EdgeThickness.Value", 1d);
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.EnableForSelection.Value", false);
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.IsVisible.Value", true);
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "EnablePointSnap.Value", true);
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "LayerCount", 1);
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "LayerItemCount", 1);
            RootLayer.Dispose();
            RootLayer = new ReactivePropertySlim<LayerTreeViewItemBase>(new LayerTreeViewItemBase());
            Layers.ToClearOperation().ExecuteTo(mainwindowViewModel.Recorder.Current);
            if (addingLayer)
            {
                AddNewLayer(mainwindowViewModel, isPreview);
            }
        }

        public void AddNewLayer(MainWindowViewModel mainwindowViewModel, bool isPreview)
        {
            var layer = new Layer(isPreview);
            layer.IsVisible.Value = true;
            layer.IsSelected.Value = true;
            layer.Name.Value = Name.GetNewLayerName(this);
            Random rand = new Random();
            layer.Color.Value = Randomizer.RandomColor(rand);
            mainwindowViewModel.Recorder.Current.ExecuteAdd(Layers, layer);
        }

        private void ExecuteRedoCommand()
        {
            MainWindowVM.Controller.Redo();
            RedoCommand.RaiseCanExecuteChanged();
            UpdateStatisticsCountRedo();
        }

        private void UpdateStatisticsCountRedo()
        {
            var statistics = MainWindowVM.Statistics.Value;
            statistics.NumberOfRedoes++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        public bool CanExecuteRedo()
        {
            return MainWindowVM.Controller.CanRedo;
        }

        private void ExecuteUndoCommand()
        {
            MainWindowVM.Controller.Undo();
            UndoCommand.RaiseCanExecuteChanged();
            RedoCommand.RaiseCanExecuteChanged();
            UpdateStatisticsCountUndo();
        }

        private void UpdateStatisticsCountUndo()
        {
            var statistics = MainWindowVM.Statistics.Value;
            statistics.NumberOfUndos++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        public bool CanExecuteUndo()
        {
            return MainWindowVM.Controller.CanUndo;
        }


        private void ExecuteClipCommand()
        {
            var picture = SelectedItems.Value.OfType<PictureDesignerItemViewModel>().First();
            var other = SelectedItems.Value.OfType<DesignerItemViewModelBase>().Last();
            var pathGeometry = GeometryCreator.CreateRectangle(other as NRectangleViewModel, picture.Left.Value, picture.Top.Value);
            (picture.TransformNortification.Value.Sender as PictureDesignerItemViewModel).Clip.Value = pathGeometry;
            (picture.TransformNortification.Value.Sender as PictureDesignerItemViewModel).ClipObject.Value = other;
            picture.TransformNortification.Zip(picture.TransformNortification.Skip(1), (Old, New) => new { OldItem = Old, NewItem = New })
            .Where(x => x.NewItem.PropertyName == "Width" || x.NewItem.PropertyName == "Height")
            .Subscribe(x =>
            {
                var _other = picture.ClipObject.Value;
                var _pathGeometry = GeometryCreator.CreateRectangle(_other as NRectangleViewModel, picture.Left.Value, picture.Top.Value, x.NewItem.PropertyName, (double)x.NewItem.OldValue, (double)x.NewItem.NewValue);
                picture.Clip.Value = _pathGeometry;
            })
            .AddTo(_CompositeDisposable);
            Remove(other);
        }

        public bool CanExecuteClip()
        {
            return SelectedItems.Value.Count() == 2 &&
                   SelectedItems.Value.First().GetType() == typeof(PictureDesignerItemViewModel);
        }


        private SelectableDesignerItemViewModelBase Combine(GeometryCombineMode mode, SelectableDesignerItemViewModelBase item1, SelectableDesignerItemViewModelBase item2 = null)
        {
            int count = 0;
            if (item1 != null)
                count++;
            if (item2 != null)
                count++;
            if (count == 1 && item1 is PolyBezierViewModel pb)
            {
                //Remove(pb);
                var combine = new CombineGeometryViewModel();
                combine.EdgeBrush.Value = pb.EdgeBrush.Value;
                combine.EdgeThickness.Value = pb.EdgeThickness.Value;
                combine.IsSelected.Value = true;
                combine.Owner = this;
                combine.ZIndex.Value = Layers.SelectMany(x => x.Children.Value).Count();
                combine.IsHitTestVisible.Value = MainWindowVM.ToolBarViewModel.CurrentHitTestVisibleState.Value;
                combine.PathGeometry.Value = GeometryCreator.CreateCombineGeometry(pb);
                combine.Left.Value = combine.PathGeometry.Value.Bounds.Left;
                combine.Top.Value = combine.PathGeometry.Value.Bounds.Top;
                combine.Width.Value = combine.PathGeometry.Value.Bounds.Width;
                combine.Height.Value = combine.PathGeometry.Value.Bounds.Height;
                //Add(combine);
                return combine;
            }
            else
            {
                var combine = new CombineGeometryViewModel();
                //Remove(item1);
                //Remove(item2);
                combine.EdgeBrush.Value = item1.EdgeBrush.Value;
                combine.EdgeThickness.Value = item1.EdgeThickness.Value;
                combine.IsSelected.Value = true;
                combine.Owner = this;
                combine.ZIndex.Value = Layers.SelectMany(x => x.Children.Value).Count();
                combine.IsHitTestVisible.Value = MainWindowVM.ToolBarViewModel.CurrentHitTestVisibleState.Value;
                combine.PathGeometry.Value = GeometryCreator.CreateCombineGeometry(item1, item2);
                if (combine.PathGeometry.Value == null || combine.PathGeometry.Value.Figures.Count() == 0)
                {
                    var item1PathGeometry = item1.PathGeometry.Value;
                    var item2PathGeometry = item2.PathGeometry.Value;

                    if (item1 is DesignerItemViewModelBase designerItem1 && item1.RotationAngle.Value != 0)
                        item1PathGeometry = designerItem1.RotatePathGeometry.Value;
                    if (item2 is DesignerItemViewModelBase designerItem2 && item2.RotationAngle.Value != 0)
                        item2PathGeometry = designerItem2.RotatePathGeometry.Value;

                    combine.PathGeometry.Value = Geometry.Combine(item1PathGeometry, item2PathGeometry, mode, null);
                }
                combine.Left.Value = combine.PathGeometry.Value.Bounds.Left;
                combine.Top.Value = combine.PathGeometry.Value.Bounds.Top;
                combine.Width.Value = combine.PathGeometry.Value.Bounds.Width;
                combine.Height.Value = combine.PathGeometry.Value.Bounds.Height;
                //Add(combine);
                return combine;
            }
        }

        private SelectableDesignerItemViewModelBase GetSelectedItemFirst()
        {
            return GetSelectedItemsForCombine().First();
        }

        private SelectableDesignerItemViewModelBase GetSelectedItemLast()
        {
            return GetSelectedItemsForCombine().Skip(1).Take(1).First();
        }


        public bool CanExecuteUnion()
        {
            var countIsCorrent = GetCountIsCorrent();
            if (countIsCorrent)
            {
                var firstElementTypeIsCorrect = SelectedItems.ElementAt(0).GetType() != typeof(PictureDesignerItemViewModel);
                var secondElementTypeIsCorrect = SelectedItems.ElementAt(1).GetType() != typeof(PictureDesignerItemViewModel);
                return countIsCorrent && firstElementTypeIsCorrect && secondElementTypeIsCorrect;
            }
            var polyBezier = GetSelectedItemsForCombine().FirstOrDefault() as PolyBezierViewModel;
            if (polyBezier != null)
            {
                return true;
            }
            return false;
        }

        private bool GetCountIsCorrent()
        {
            List<SelectableDesignerItemViewModelBase> newlist = GetSelectedItemsForCombine();
            return newlist.Count() == 2;
        }

        private List<SelectableDesignerItemViewModelBase> GetSelectedItemsForCombine()
        {
            var list = SelectedItems.Value.ToList();
            var newlist = new List<SelectableDesignerItemViewModelBase>();
            foreach (var item in list)
            {
                if (item is DesignerItemViewModelBase)
                    newlist.Add(item);
            }
            newlist = newlist.Distinct().ToList();
            return newlist;
        }

        private void ReleaseMiddleButton(MouseEventArgs args)
        {
            if (args.MiddleButton == MouseButtonState.Released)
            {
                _MiddleButtonIsPressed = false;
                var diagramControl = App.Current.MainWindow.GetChildOfType<DiagramControl>();
                diagramControl.Cursor = Cursors.Arrow;
            }
        }

        public DiagramViewModel(MainWindowViewModel MainWindowVM, IDialogService dlgService, int width, int height)
            : this(MainWindowVM, width, height)
        {
            this.dlgService = dlgService;

            Mediator.Instance.Register(this);
        }

        public void DeselectAll()
        {
            foreach (var layerItem in Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children.Value)
                                            .OfType<LayerItem>())
            {
                layerItem.Item.Value.IsSelected.Value = false;
                layerItem.IsSelected.Value = false;
            }
        }

        private void ExecuteAddItemCommand(object parameter)
        {
            if (parameter is SelectableDesignerItemViewModelBase item)
            {
                var targetLayer = SelectedLayers.Value.First();
                var newZIndex = targetLayer.GetNewZIndex(Layers.TakeWhile(x => x != targetLayer));
                Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children.Value)
                      .Where(x => x != targetLayer)
                      .ToList()
                      .ForEach(x => x.PushZIndex(MainWindowVM.Recorder, newZIndex));
                item.ZIndex.Value = newZIndex;
                item.Owner = this;
                Add(item);
            }
        }

        private void ExecuteRemoveItemCommand(object parameter)
        {
            if (parameter is SelectableDesignerItemViewModelBase)
            {
                SelectableDesignerItemViewModelBase item = (SelectableDesignerItemViewModelBase)parameter;
                Remove(item);
                item.Dispose();
                UpdateZIndex();
            }
        }

        private void UpdateZIndex()
        {
            var items = (from item in Layers.SelectMany(x => x.Children.Value)
                         orderby (item as LayerItem).Item.Value.ZIndex.Value ascending
                         select item).ToList();

            for (int i = 0; i < items.Count; ++i)
            {
                (items.ElementAt(i) as LayerItem).Item.Value.ZIndex.Value = i;
            }
        }


        private void ExecuteClearSelectedItemsCommand(object parameter)
        {
            foreach (var layerItem in Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children.Value)
                                            .OfType<LayerItem>())
            {
                layerItem.Item.Value.IsSelected.Value = false;
            }
        }

        private void ExecuteCreateNewDiagramCommand(object parameter)
        {
            Layers.Clear();
            Layers.Add(new Layer());
        }


        private void Add(SelectableDesignerItemViewModelBase item, bool isRecording = true, string layerItemName = null)
        {
            SelectedLayers.Value.First().AddItem(MainWindowVM, this, item, isRecording, layerItemName: layerItemName);
        }

        private void Add(LayerItem item)
        {
            SelectedLayers.Value.First().AddItem(MainWindowVM, this, item);
            LogManager.GetCurrentClassLogger().Info($"Add item {item.ShowPropertiesAndFields()}");
            UpdateStatisticsCountAdd();
        }

        private void UpdateStatisticsCountAdd()
        {
            var statistics = MainWindowVM.Statistics.Value;
            statistics.NumberOfTimesTheItemWasDrawn++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        private void Remove(SelectableDesignerItemViewModelBase item)
        {
            Layers.ToList().ForEach(x => x.RemoveItem(MainWindowVM, item));
            LogManager.GetCurrentClassLogger().Info($"Remove item {item.ShowPropertiesAndFields()}");
            UpdateStatisticsCountRemove();
        }

        private void UpdateStatisticsCountRemove()
        {
            var statistics = MainWindowVM.Statistics.Value;
            statistics.NumberOfTimesTheItemWasDeleted++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        private void ExecuteSelectAllCommand()
        {
            Layers.SelectMany(x => x.Children.Value).ToList().ForEach(x => (x as LayerItem).Item.Value.IsSelected.Value = true);
        }

        
        private IEnumerable<SelectableDesignerItemViewModelBase> GetGroupMembers(SelectableDesignerItemViewModelBase item)
        {
            var list = new List<SelectableDesignerItemViewModelBase>();
            list.Add(item);
            var children = Layers.SelectMany(x => x.Children.Value)
                                 .Where(x => (x as LayerItem).Item.Value.ParentID == item.ID)
                                 .Select(x => (x as LayerItem).Item.Value);
            list.AddRange(children);
            return list;
        }

        public static Rect GetBoundingRectangle(IEnumerable<SelectableDesignerItemViewModelBase> items)
        {
            double x1 = Double.MaxValue;
            double y1 = Double.MaxValue;
            double x2 = Double.MinValue;
            double y2 = Double.MinValue;

            foreach (var item in items)
            {
                if (item is DesignerItemViewModelBase designerItem)
                {
                    var centerPoint = designerItem.CenterPoint.Value;
                    var angleInDegrees = designerItem.RotationAngle.Value;

                    var p0 = new Point(designerItem.Left.Value + designerItem.Width.Value, designerItem.Top.Value + designerItem.Height.Value / 2);
                    var p1 = new Point(designerItem.Left.Value, designerItem.Top.Value);
                    var p2 = new Point(designerItem.Left.Value + designerItem.Width.Value, designerItem.Top.Value);
                    var p3 = new Point(designerItem.Left.Value + designerItem.Width.Value, designerItem.Top.Value + designerItem.Height.Value);
                    var p4 = new Point(designerItem.Left.Value, designerItem.Top.Value + designerItem.Height.Value);

                    var vector_p0_center = p0 - centerPoint;
                    var vector_p1_center = p1 - centerPoint;
                    var vector_p2_center = p2 - centerPoint;
                    var vector_p3_center = p3 - centerPoint;
                    var vector_p4_center = p4 - centerPoint;

                    UpdateBoundary(ref x1, ref y1, ref x2, ref y2, centerPoint, angleInDegrees + Vector.AngleBetween(vector_p0_center, vector_p1_center), p1);
                    UpdateBoundary(ref x1, ref y1, ref x2, ref y2, centerPoint, angleInDegrees + Vector.AngleBetween(vector_p0_center, vector_p2_center), p2);
                    UpdateBoundary(ref x1, ref y1, ref x2, ref y2, centerPoint, angleInDegrees + Vector.AngleBetween(vector_p0_center, vector_p3_center), p3);
                    UpdateBoundary(ref x1, ref y1, ref x2, ref y2, centerPoint, angleInDegrees + Vector.AngleBetween(vector_p0_center, vector_p4_center), p4);
                }
                else if (item is ConnectorBaseViewModel connector)
                {
                    x1 = Math.Min(Math.Min(connector.Points[0].X, connector.Points[1].X), x1);
                    y1 = Math.Min(Math.Min(connector.Points[0].Y, connector.Points[1].Y), y1);

                    x2 = Math.Max(Math.Max(connector.Points[0].X, connector.Points[1].X), x2);
                    y2 = Math.Max(Math.Max(connector.Points[0].Y, connector.Points[1].Y), y2);
                }
            }

            return new Rect(new Point(x1, y1), new Point(x2, y2));
        }

        private static void UpdateBoundary(ref double x1, ref double y1, ref double x2, ref double y2, Point centerPoint, double angleInDegrees, Point point)
        {
            var rad = angleInDegrees * Math.PI / 180;

            var t = RotatePoint(centerPoint, point, rad);

            x1 = Math.Min(t.Item1, x1);
            y1 = Math.Min(t.Item2, y1);
            x2 = Math.Max(t.Item1, x2);
            y2 = Math.Max(t.Item2, y2);
        }

        private static Tuple<double, double> RotatePoint(Point center, Point point, double rad)
        {
            var z1 = point.X - center.X;
            var z2 = point.Y - center.Y;
            var x = center.X + Math.Sqrt(Math.Pow(z1, 2) + Math.Pow(z2, 2)) * Math.Cos(rad);
            var y = center.Y + Math.Sqrt(Math.Pow(z1, 2) + Math.Pow(z2, 2)) * Math.Sin(rad);

            return new Tuple<double, double>(x, y);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Layers.Dispose();
                    AllItems.Dispose();
                    SelectedItems.Dispose();
                    EdgeThickness.Dispose();
                    EnableMiniMap.Dispose();
                    EnableCombine.Dispose();
                    EnableLayers.Dispose();
                    FileName.Dispose();
                    CanvasBackground.Dispose();
                    EnablePointSnap.Dispose();
                    if (_AutoSaveTimerDisposableObj != null)
                        _AutoSaveTimerDisposableObj.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
