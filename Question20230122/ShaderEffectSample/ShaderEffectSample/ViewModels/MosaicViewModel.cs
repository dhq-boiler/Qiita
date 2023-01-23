using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SharpDX.D3DCompiler;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ShaderEffectSample.ViewModels
{
    public class MosaicViewModel : DependencyObject, INotifyPropertyChanged
    {
        private CompositeDisposable _CompositeDisposable = new CompositeDisposable();
        private ReactivePropertySlim<IList<byte>> _bytecode = new ReactivePropertySlim<IList<byte>>();
        private ReactivePropertySlim<string> _errorMessage = new ReactivePropertySlim<string>();

        public static readonly DependencyProperty BitmapProperty = DependencyProperty.Register("Bitmap", typeof(BitmapSource), typeof(MosaicViewModel));

        public event PropertyChangedEventHandler? PropertyChanged;

        public BitmapSource Bitmap
        {
            get { return (BitmapSource)GetValue(BitmapProperty); }
            set { SetValue(BitmapProperty, value); }
        }

        public ReactivePropertySlim<double> ColumnPixels { get; } = new ReactivePropertySlim<double>(30d);
        public ReactivePropertySlim<double> RowPixels { get; } = new ReactivePropertySlim<double>(30d);

        public MosaicViewModel()
        {
            Source = new ReactivePropertySlim<string>(@"
sampler2D input : register(s0);
float width : register(c0);
float height : register(c1);
float cp : register(c2);
float rp : register(c3);

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float x = cp;
    float y = rp;
    float l = 0.0;
    if (width > height)
    {
        x = x * width / height;
        l = 0.5 / y;
    }
    else
    {
        y = y * height / width;
        l = 0.5 / x;
    }
    float2 uv2 = float2(0.5 * (floor(uv.x * x) + ceil(uv.x * x)) / x, 0.5 * (floor(uv.y * y) + ceil(uv.y * y)) / y);
    return tex2D(input, uv2);
}
");
            Bytecode = _bytecode.ToReadOnlyReactivePropertySlim();
            ErrorMessage = _errorMessage.ToReadOnlyReactivePropertySlim();

            Source
                .Delay(TimeSpan.FromMilliseconds(500))
                .Select(value =>
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        try
                        {
                            return ShaderBytecode.Compile(value, "main", "ps_3_0");
                        }
                        catch (Exception e)
                        {
                            return new CompilationResult(null, SharpDX.Result.Fail, e.StackTrace);
                        }
                    }
                    return new CompilationResult(null, SharpDX.Result.Ok, string.Empty);
                })
                .ObserveOnDispatcher()
                .Subscribe(value =>
                {
                    if (value != null)
                    {
                        _bytecode.Value = value.Bytecode?.Data;
                        _errorMessage.Value = value.Message;
                    }
                    else
                    {
                        _bytecode.Value = null;
                        _errorMessage.Value = string.Empty;
                    }
                });

            ColumnPixels.Subscribe(_ =>
            {
                CompilationResult result = null;
                try
                {
                    result = ShaderBytecode.Compile(Source.Value, "main", "ps_3_0");
                }
                catch (Exception e)
                {
                    result = new CompilationResult(null, SharpDX.Result.Fail, e.StackTrace);
                }

                if (result != null)
                {
                    _bytecode.Value = result.Bytecode?.Data;
                    _errorMessage.Value = result.Message;
                }
                else
                {
                    _bytecode.Value = null;
                    _errorMessage.Value = string.Empty;
                }
            }).AddTo(_CompositeDisposable);
            RowPixels.Subscribe(_ =>
            {
                CompilationResult result = null;
                try
                {
                    result = ShaderBytecode.Compile(Source.Value, "main", "ps_3_0");
                }
                catch (Exception e)
                {
                    result = new CompilationResult(null, SharpDX.Result.Fail, e.StackTrace);
                }

                if (result != null)
                {
                    _bytecode.Value = result.Bytecode?.Data;
                    _errorMessage.Value = result.Message;
                }
                else
                {
                    _bytecode.Value = null;
                    _errorMessage.Value = string.Empty;
                }
            }).AddTo(_CompositeDisposable);
        }

        public ReactivePropertySlim<string> Source
        {
            get;
        }

        public ReadOnlyReactivePropertySlim<IList<byte>> Bytecode
        {
            get;
        }

        public ReadOnlyReactivePropertySlim<string> ErrorMessage
        {
            get;
        }

    }
}
