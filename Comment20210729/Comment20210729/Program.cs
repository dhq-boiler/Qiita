﻿using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace Comment20210729
{
    class Program
    {
        static void Main(string[] args)
        {
            var diagramViewModel = new DiagramViewModel();
            
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー１";
            Console.WriteLine("Add layer1");
            diagramViewModel.Layers.Add(layer1);
            
            var layer2 = new Layer();
            layer2.Name.Value = "レイヤー２";
            Console.WriteLine("Add layer2");
            diagramViewModel.Layers.Add(layer2);
            
            Console.WriteLine("set IsSelect={true, false}");
            diagramViewModel.Layers[0].IsSelected.Value = true;
            diagramViewModel.Layers[1].IsSelected.Value = false;
            Console.WriteLine($"{diagramViewModel.SelectedLayers.Value.Count()} items");
            Console.WriteLine(string.Join(", ", diagramViewModel.SelectedLayers.Value.Select(x => $"{x.Name.Value.ToString()}[{x.IsSelected.Value}]")));

            Console.WriteLine("set IsSelect={false, true}");
            diagramViewModel.Layers[0].IsSelected.Value = false;
            diagramViewModel.Layers[1].IsSelected.Value = true;
            Console.WriteLine($"{diagramViewModel.SelectedLayers.Value.Count()} items");
            Console.WriteLine(string.Join(", ", diagramViewModel.SelectedLayers.Value.Select(x => $"{x.Name.Value.ToString()}[{x.IsSelected.Value}]")));

            Console.WriteLine("set IsSelect={true, false}");
            diagramViewModel.Layers[0].IsSelected.Value = true;
            diagramViewModel.Layers[1].IsSelected.Value = false;
            Console.WriteLine($"{diagramViewModel.SelectedLayers.Value.Count()} items");
            Console.WriteLine(string.Join(", ", diagramViewModel.SelectedLayers.Value.Select(x => $"{x.Name.Value.ToString()}[{x.IsSelected.Value}]")));

            Console.WriteLine("add LayerItem");
            var item = new SelectableDesignerItemViewModelBase();
            var layerItem = new LayerItem(item, diagramViewModel.Layers[0]);
            diagramViewModel.Layers[0].Items.Add(layerItem);
            Console.WriteLine("switch IsSelected to true");
            item.IsSelected.Value = true;
            Console.WriteLine("switch IsSelected to false");
            item.IsSelected.Value = false;
        }
    }

    class DiagramViewModel : BindableBase
    {

        public ReactiveCollection<Layer> Layers { get; } = new ReactiveCollection<Layer>();

        public ReadOnlyReactivePropertySlim<Layer[]> SelectedLayers { get; }

        public DiagramViewModel()
        {
            SelectedLayers = Layers.ObserveElementObservableProperty(x => x.IsSelected)
                                   .Select(_ => Layers.Where(x => x.IsSelected.Value == true).ToArray())
                                   .ToReadOnlyReactivePropertySlim(Array.Empty<Layer>()); //work fine!!!
        }
    }

    public class Layer : BindableBase, IObservable<LayerObservable>
    {

        public ReactivePropertySlim<bool> IsSelected { get; } = new ReactivePropertySlim<bool>();

        public ReactivePropertySlim<string> Name { get; } = new ReactivePropertySlim<string>();

        public ReactiveCollection<LayerItem> Items { get; } = new ReactiveCollection<LayerItem>();

        public Layer()
        {

        }

        private List<IObserver<LayerObservable>> _observers = new List<IObserver<LayerObservable>>();

        public IDisposable Subscribe(IObserver<LayerObservable> observer)
        {
            _observers.Add(observer);
            observer.OnNext(new LayerObservable());
            return new LayerDisposable(this, observer);
        }

        public class LayerDisposable : IDisposable
        {
            private Layer layer;
            private IObserver<LayerObservable> observer;

            public LayerDisposable(Layer layer, IObserver<LayerObservable> observer)
            {
                this.layer = layer;
                this.observer = observer;
            }

            public void Dispose()
            {
                layer._observers.Remove(observer);
            }
        }
    }

    public class LayerObservable : BindableBase
    {
    }

    public class LayerItem : BindableBase
    {
        public ReactiveProperty<bool> IsSelected { get; set; }
        public ReactivePropertySlim<string> Name { get; } = new ReactivePropertySlim<string>();
        public ReactivePropertySlim<Layer> Owner { get; } = new ReactivePropertySlim<Layer>();
        public ReactivePropertySlim<SelectableDesignerItemViewModelBase> Item { get; }

        public LayerItem(SelectableDesignerItemViewModelBase item, Layer owner)
        {
            Item = new ReactivePropertySlim<SelectableDesignerItemViewModelBase>(item);
            Owner.Value = owner;
            Init();
        }

        private void Init()
        {
            IsSelected = Item.Where(x => x != null)
                             .Select(x => x.IsSelected.Value)
                             .ToReactiveProperty();
            IsSelected.Subscribe(x =>
            {
                if (Item.Value != null)
                {
                    Item.Value.IsSelected.Value = x;
                }
            });
            Item.ObserveProperty(x => x.Value.IsSelected)
                .Subscribe(x =>
                {
                    Console.WriteLine("detected LayerItem.Item changes. run LayerItem.UpdateAppearance().");
                });
        }
    }

    public class SelectableDesignerItemViewModelBase : BindableBase, IObservable<SelectableDesignerItemViewModelBaseObservable>
    {
        public ReactivePropertySlim<bool> IsSelected { get; } = new ReactivePropertySlim<bool>();

        private List<IObserver<SelectableDesignerItemViewModelBaseObservable>> _observers = new List<IObserver<SelectableDesignerItemViewModelBaseObservable>>();

        public SelectableDesignerItemViewModelBase()
        {
            IsSelected.Subscribe();
        }

        public IDisposable Subscribe(IObserver<SelectableDesignerItemViewModelBaseObservable> observer)
        {
            _observers.Add(observer);
            observer.OnNext(new SelectableDesignerItemViewModelBaseObservable(this));
            return new SelectableDesignerItemViewModelBaseDisposable(this, observer);
        }

        public class SelectableDesignerItemViewModelBaseDisposable : IDisposable
        {
            private SelectableDesignerItemViewModelBase item;
            private IObserver<SelectableDesignerItemViewModelBaseObservable> observer;

            public SelectableDesignerItemViewModelBaseDisposable(SelectableDesignerItemViewModelBase item, IObserver<SelectableDesignerItemViewModelBaseObservable> observer)
            {
                this.item = item;
                this.observer = observer;
            }

            public void Dispose()
            {
                item._observers.Remove(observer);
            }
        }
    }

    public class SelectableDesignerItemViewModelBaseObservable : BindableBase
    {
        public SelectableDesignerItemViewModelBase Owner { get; }
        public SelectableDesignerItemViewModelBaseObservable(SelectableDesignerItemViewModelBase owner)
        {
            Owner = owner;
        }
    }
}
