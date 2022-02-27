using boilersGraphics.Extensions;
using boilersGraphics.Models;
using boilersGraphics.Properties;
using boilersGraphics.ViewModels;
using Moq;
using NUnit.Framework;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class ReactivePropertyTest
    {
        [Test]
        public void Children代入()
        {
            boilersGraphics.App.IsTest = true;
            var dlgService = new Mock<IDialogService>();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
            var diagramVM = new DiagramViewModel(mainWindowViewModel, 1000, 1000);
            diagramVM.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = $"{Resources.Name_Layer}1";
            diagramVM.Layers.Add(layer1);
            layer1.IsSelected.Value = true; //レイヤー1を選択状態にする

            //diagramVM.DisposeProperties();
            //diagramVM.InitializeProperties_Layers(false);
            diagramVM.AddNewLayer(mainWindowViewModel, false);
            var firstSelectedLayer = layer1;
            var item = new NRectangleViewModel();
            var layerItem = new LayerItem(item, firstSelectedLayer, "TEST");
            firstSelectedLayer.Children.Value.Add(layerItem);
            Assert.That(firstSelectedLayer.Name.Value, Is.EqualTo("レイヤー1"));
            Assert.That(diagramVM.AllItems.Value, Has.Member(item));
            firstSelectedLayer.Children.Value = new ObservableCollection<LayerTreeViewItemBase>(new LayerItem[] { layerItem });
            //diagramVM.InitializeProperties_Items(false);
            //diagramVM.SetSubscribes(false);

            Assert.That(diagramVM.AllItems.Value, Has.Member(item));
        }

        [Test]
        public void これも通る()
        {
            boilersGraphics.App.IsTest = true;
            var dlgService = new Mock<IDialogService>();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
            var diagramVM = new DiagramViewModel(mainWindowViewModel, 1000, 1000);
            diagramVM.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = $"{Resources.Name_Layer}1";
            diagramVM.Layers.Add(layer1);
            layer1.IsSelected.Value = true; //レイヤー1を選択状態にする

            var item = new NRectangleViewModel();
            var layerItem = new LayerItem(item, layer1, "TEST");
            layer1.Children.Value.Add(layerItem);
            Assert.That(diagramVM.AllItems.Value, Has.Member(item));
            var item2 = new NRectangleViewModel();
            var layerItem2 = new LayerItem(item2, layer1, "TEST2");
            layer1.Children.Value.Add(layerItem2);
            diagramVM.RootLayer.Value.Children.Value = new ObservableCollection<LayerTreeViewItemBase>(new LayerItem[] { layerItem, layerItem2 });

            Assert.That(diagramVM.AllItems.Value, Has.Member(item));
            Assert.That(diagramVM.AllItems.Value, Has.Member(item2));
        }

        class TestClassB : BindableBase
        {

        }

        class TestClass : BindableBase
        {
            public ReactivePropertySlim<ObservableCollection<TestClassB>> Children { get; set; } = new ReactivePropertySlim<ObservableCollection<TestClassB>>(new ObservableCollection<TestClassB>());

            public ReadOnlyReactivePropertySlim<TestClassB[]> X { get; }

            public TestClass()
            {
                X = this.ObserveProperty(x => x.Children.Value)
                        .Select(_ => Children.Value.ToArray())
                        .ToReadOnlyReactivePropertySlim(Array.Empty<TestClassB>());
            }
        }

        [Test]
        public void これは通る()
        {
            var testclassB = new TestClassB();
            var testclass = new TestClass();
            
            Assert.That(testclass.X.Value, Has.No.Member(testclassB));

            testclass.Children.Value = new ObservableCollection<TestClassB>() { testclassB };

            Assert.That(testclass.X.Value, Has.Member(testclassB));
        }

        class TestClassC : BindableBase
        {
            public ReactiveCollection<LayerTreeViewItemBase> Layers { get; set; } = new ReactiveCollection<LayerTreeViewItemBase>();

            public ReadOnlyReactivePropertySlim<SelectableDesignerItemViewModelBase[]> AllItems { get; set; }

            public TestClassC()
            {
                AllItems = Layers.CollectionChangedAsObservable()
                             .Select(_ => Layers.Select(x => x.LayerItemsChangedAsObservable()).Merge())
                             .Switch()
                             .Select(_ => Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children.Value)
                                                .Where(x => x.GetType() == typeof(LayerItem))
                                                .Select(y => (y as LayerItem).Item.Value)
                                                .ToArray())
                             .ToReadOnlyReactivePropertySlim(Array.Empty<SelectableDesignerItemViewModelBase>());
            }
        }

        [Test]
        public void これも通る2()
        {
            boilersGraphics.App.IsTest = true;
            var vm = new NRectangleViewModel();
            var layerItem = new LayerItem(vm, null, String.Empty);
            var testclass = new TestClassC();
            testclass.Layers.Add(new Layer(false));

            Assert.That(testclass.AllItems.Value, Has.No.Member(vm));

            testclass.Layers.First().Children.Value = new ObservableCollection<LayerTreeViewItemBase>() { layerItem };

            Assert.That(testclass.AllItems.Value, Has.Member(vm));
        }

        [Test]
        public void ReactiveCollection初期化()
        {
            boilersGraphics.App.IsTest = true;
            var bag = new ConcurrentBag<SelectableDesignerItemViewModelBase>();
            const int count = 100000;
            Parallel.For(0, count, i =>
            {
                bag.Add(new NRectangleViewModel() { });
            });
            var l = new List<LayerTreeViewItemBase>();
            while (bag.TryTake(out var item))
            {
                var i = new LayerItem(item, null, null);
                l.Add(i);
            }
            var reactiveCollection = new ReactiveCollection<LayerTreeViewItemBase>(l.ToObservable());
            Assert.That(reactiveCollection, Has.Count.EqualTo(100000));
        }
    }
}
