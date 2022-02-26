using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Models;
using boilersGraphics.Properties;
using boilersGraphics.Views.Behaviors;
using Microsoft.Win32;
using Microsoft.Xaml.Behaviors;
using NLog;
using Prism.Commands;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace boilersGraphics.ViewModels
{
    public class ToolBarViewModel
    {
        private IDialogService dlgService = null;
        public ReactiveCollection<ToolItemData> ToolItems { get; } = new ReactiveCollection<ToolItemData>();
        public ReactiveCollection<ToolItemData> ToolItems2 { get; } = new ReactiveCollection<ToolItemData>();

        public BehaviorCollection Behaviors { get { return Interaction.GetBehaviors(App.GetCurrentApp().MainWindow.GetChildOfType<DesignerCanvas>()); } }

        public ReactivePropertySlim<bool> CurrentHitTestVisibleState { get; } = new ReactivePropertySlim<bool>();

        public DeselectBehavior DeselectBehavior { get; } = new DeselectBehavior();
        public NDrawRectangleBehavior NDrawRectangleBehavior { get; } = new NDrawRectangleBehavior();
        public PictureBehavior PictureBehavior { get; private set; }

        public ToolBarViewModel(IDialogService dialogService, MainWindowViewModel mainWindowViewModel)
        {
            this.dlgService = dialogService;
            InitializeToolItems(dialogService);
            InitializeToolItems2(mainWindowViewModel);
        }

        public void InitializeToolItems(IDialogService dialogService)
        {
            ToolItems.Add(new ToolItemData("pointer", "pack://application:,,,/Assets/img/pointer.png", Resources.Tool_Pointer, new DelegateCommand(() =>
            {
                var mainWindowViewModel = (App.Current.MainWindow.DataContext as MainWindowViewModel);
                mainWindowViewModel.ClearCurrentOperationAndDetails();
                Behaviors.Clear();
                if (!Behaviors.Contains(DeselectBehavior))
                {
                    Behaviors.Add(DeselectBehavior);
                }
                ChangeHitTestToEnable();
                SelectOneToolItem("pointer");
            })));
            ToolItems.Add(new ToolItemData("rectangle", "pack://application:,,,/Assets/img/rectangle.png", Resources.Tool_Rectangle, new DelegateCommand(() =>
            {
                var mainWindowViewModel = (App.Current.MainWindow.DataContext as MainWindowViewModel);
                mainWindowViewModel.ClearCurrentOperationAndDetails();
                Behaviors.Clear();
                if (!Behaviors.Contains(NDrawRectangleBehavior))
                {
                    Behaviors.Add(NDrawRectangleBehavior);
                }
                ChangeHitTestToDisable();
                SelectOneToolItem("rectangle");
            })));
            ToolItems.Add(new ToolItemData("picture", "pack://application:,,,/Assets/img/Picture.png", Resources.Tool_Picture, new DelegateCommand(() =>
            {
                var mainWindowViewModel = (App.Current.MainWindow.DataContext as MainWindowViewModel);
                mainWindowViewModel.ClearCurrentOperationAndDetails();
                var dialog = new OpenFileDialog();
                dialog.Multiselect = false;
                dialog.Filter = boilersGraphics.Properties.Resources.String_SupportImage;
                if (dialog.ShowDialog() == true)
                {
                    var bitmap = BitmapFactory.FromStream(new FileStream(dialog.FileName, FileMode.Open, FileAccess.Read));
                    PictureBehavior = new PictureBehavior(dialog.FileName, bitmap.Width, bitmap.Height);
                    Behaviors.Clear();
                    if (!Behaviors.Contains(PictureBehavior))
                    {
                        Behaviors.Add(PictureBehavior);
                    }
                    ChangeHitTestToDisable();
                    SelectOneToolItem("picture");
                }
            })));
        }

        private void InitializeToolItems2(MainWindowViewModel mainWindowViewModel)
        {
            var toolItemData = new ToolItemData("minimap", "pack://application:,,,/Assets/img/minimap.png", Resources.MenuItem_MiniMap, new DelegateCommand(() =>
            {
                mainWindowViewModel.DiagramViewModel.EnableMiniMap.Value = !mainWindowViewModel.DiagramViewModel.EnableMiniMap.Value;
            }));
            ToolItems2.Add(toolItemData);
            toolItemData.IsChecked = mainWindowViewModel.DiagramViewModel.EnableMiniMap.Value;
            toolItemData.IsChecked = mainWindowViewModel.DiagramViewModel.EnableCombine.Value;
            toolItemData = new ToolItemData("layers", "pack://application:,,,/Assets/img/icon_Layers.png", Resources.MenuItem_Layering, new DelegateCommand(() =>
            {
                mainWindowViewModel.DiagramViewModel.EnableLayers.Value = !mainWindowViewModel.DiagramViewModel.EnableLayers.Value;
            }));
            ToolItems2.Add(toolItemData);
            toolItemData.IsChecked = mainWindowViewModel.DiagramViewModel.EnableLayers.Value;
        }

        public void FinalizeToolItems()
        {
            ToolItems.Clear();
        }

        public void ReinitializeToolItems()
        {
            FinalizeToolItems();
            InitializeToolItems(dlgService);
        }

        private void ChangeHitTestToDisable()
        {
            var diagramViewModel = (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel;
            diagramViewModel.AllItems.Value.ToList().ForEach(x => x.IsHitTestVisible.Value = false);
            CurrentHitTestVisibleState.Value = false;
        }

        private void ChangeHitTestToEnable()
        {
            var diagramViewModel = (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel;
            diagramViewModel.SelectedLayers.Value.ToList().ForEach(x => 
                (x as Layer).Children.Value.ToList().ForEach(y =>
                {
                    var layerItem = y as LayerItem;
                    layerItem.Item.Value.IsHitTestVisible.Value = true;
                    LogManager.GetCurrentClassLogger().Trace($"{layerItem.Name.Value}.IsHitTestVisible={layerItem.Item.Value.IsHitTestVisible.Value}");
                })
            );
            CurrentHitTestVisibleState.Value = true;
        }

        private void SelectOneToolItem(string toolName)
        {
            var toolItem = ToolItems.Where(i => i.Name.Value == toolName).Single();
            toolItem.IsChecked = true;

            ToolItems.Where(i => i.Name.Value != toolName).ToList().ForEach(i => i.IsChecked = false);

            switch (toolName)
            {
                case "pointer":
                case "lasso":
                case "straightline":
                case "rectangle":
                case "ellipse":
                case "picture":
                case "letter":
                case "letter-vertical":
                case "polygon":
                case "bezier":
                case "snappoint":
                case "brush":
                case "eraser":
                case "slice":
                case "polybezier":
                case "pie":
                    (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.ContextMenuVisibility.Value = Visibility.Visible;
                    break;
                case "dropper":
                    (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.ContextMenuVisibility.Value = Visibility.Collapsed;
                    break;
            }
        }
    }
}
