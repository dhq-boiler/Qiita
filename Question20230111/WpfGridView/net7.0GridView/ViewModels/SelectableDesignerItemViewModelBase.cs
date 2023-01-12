using Prism.Commands;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows.Media;

namespace net7GridView.ViewModels
{
    public interface ISelectItems
    {
        DelegateCommand<object> SelectItemCommand { get; }
    }


    public abstract class SelectableDesignerItemViewModelBase : BindableBase
    {
        protected CompositeDisposable _CompositeDisposable = new CompositeDisposable();

        public static int SelectedOrderCount { get; set; } = 0;

        public SelectableDesignerItemViewModelBase(int id)
        {
            this.Id = id;
            Init();
        }

        public SelectableDesignerItemViewModelBase()
        {
            Init();
        }

        public DelegateCommand<object> SelectItemCommand { get; private set; }
        public int Id { get; set; }

        // ↓ Flags ↓
        
        public ReactivePropertySlim<bool> IsSelected { get; } = new ReactivePropertySlim<bool>();
        public ReactivePropertySlim<bool> EnableForSelection { get; } = new ReactivePropertySlim<bool>();
        public ReactivePropertySlim<bool> EnablePathGeometryUpdate { get; set; } = new ReactivePropertySlim<bool>();
        public ReactivePropertySlim<bool> IsVisible { get; } = new ReactivePropertySlim<bool>();
        public ReactivePropertySlim<bool> IsHitTestVisible { get; set; } = new ReactivePropertySlim<bool>();
        public ReactivePropertySlim<bool> CanDrag { get; set; } = new ReactivePropertySlim<bool>(true);
        
        // ↑ Flags ↑

        public ReactivePropertySlim<int> SelectedOrder { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<Matrix> Matrix { get; } = new ReactivePropertySlim<Matrix>(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe | ReactivePropertyMode.DistinctUntilChanged);
        public ReactivePropertySlim<double> RotationAngle { get; } = new ReactivePropertySlim<double>(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe | ReactivePropertyMode.DistinctUntilChanged);
        public ReactivePropertySlim<int> ZIndex { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<Brush> EdgeBrush { get; } = new ReactivePropertySlim<Brush>(Brushes.Transparent);
        public ReactivePropertySlim<Brush> FillBrush { get; } = new ReactivePropertySlim<Brush>(Brushes.Transparent);
        public ReactivePropertySlim<double> EdgeThickness { get; } = new ReactivePropertySlim<double>(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe);
        public ReadOnlyReactivePropertySlim<double> HalfEdgeThickness
        {
            get { return EdgeThickness.Select(x => x / 2).ToReadOnlyReactivePropertySlim(); }
        }
        public ReadOnlyReactivePropertySlim<PathGeometry> PathGeometry { get; set; }
        public ReactivePropertySlim<PathGeometry> PathGeometryNoRotate { get; } = new ReactivePropertySlim<PathGeometry>();
        public ReactivePropertySlim<PathGeometry> PathGeometryRotate { get; } = new ReactivePropertySlim<PathGeometry>();
        public ReactivePropertySlim<PenLineJoin> PenLineJoin { get; } = new ReactivePropertySlim<PenLineJoin>();
        public ReactiveCollection<PenLineJoin> PenLineJoins { get; private set; }
        public ReactivePropertySlim<DoubleCollection> StrokeDashArray { get; } = new ReactivePropertySlim<DoubleCollection>();

        public string Name { get; set; }

        public Guid ID { get; set; } = Guid.NewGuid();

        public Guid ParentID { get; set; }

        public IDisposable GroupDisposable { get; set; }



        private void Init()
        {
            EnableForSelection.Value = true;

            RotationAngle.Subscribe(angle =>
            {
                if (angle > 360)
                {
                    RotationAngle.Value = angle % 360;
                }
            })
            .AddTo(_CompositeDisposable);
            PenLineJoins = new ReactiveCollection<PenLineJoin>
            {
                System.Windows.Media.PenLineJoin.Miter,
                System.Windows.Media.PenLineJoin.Bevel,
                System.Windows.Media.PenLineJoin.Round
            };
            StrokeDashArray.Value = new DoubleCollection();
        }

        public abstract Type GetViewType();

        public abstract bool SupportsPropertyDialog { get; }

        public void Swap(SelectableDesignerItemViewModelBase other)
        {
            if (GetType() != other.GetType())
                throw new InvalidOperationException("GetType() != other.GetType()");
            SwapInternal_SwapProperties(this, other);
            SwapInternal_SwapFields(this, other);
        }

        private static void SwapInternal_SwapFields<T>(T left, T right)
        {
            var fieldInfos = typeof(T).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);

            foreach (var fieldInfo in fieldInfos)
            {
                var temp = fieldInfo.GetValue(left);
                fieldInfo.SetValue(left, fieldInfo.GetValue(right));
                fieldInfo.SetValue(right, temp);
            }
        }

        private static void SwapInternal_SwapProperties<T>(T left, T right)
        {
            var propertyInfos = typeof(T).GetProperties(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);

            foreach (var propertyInfo in propertyInfos)
            {
                var temp = propertyInfo.GetValue(left);
                propertyInfo.SetValue(left, propertyInfo.GetValue(right));
                propertyInfo.SetValue(right, temp);
            }
        }

        public string ShowPropertiesAndFields()
        {
            string ret = $"<{GetType().Name}>{{";

            PropertyInfo[] properties = GetType().GetProperties(
                BindingFlags.Public
                | BindingFlags.Instance);

            foreach (var property in properties.Except(new PropertyInfo[] 
                                                           {
                                                               GetType().GetProperty("Parent"),
                                                               GetType().GetProperty("SelectedItems")
                                                           }))
            {
                ret += $"{property.Name}={property.GetValue(this)},";
            }

            FieldInfo[] fields = GetType().GetFields(
                BindingFlags.Public
                | BindingFlags.Instance);

            foreach (var field in fields)
            {
                ret += $"{field.Name}={field.GetValue(this)},";
            }
            ret = ret.Remove(ret.Length - 1, 1);
            ret += $"}}";
            return ret;
        }

        public override string ToString()
        {
            return ShowPropertiesAndFields();
        }
    }
}
