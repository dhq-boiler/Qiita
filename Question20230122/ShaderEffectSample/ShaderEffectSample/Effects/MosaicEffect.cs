﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace ShaderEffectSample.Effects
{
    public class MosaicEffect : ShaderEffect
    {
        public MosaicEffect()
        {
            PixelShader ps = new PixelShader();
            this.PixelShader = ps;
            UpdateShaderValue(InputProperty);
        }

        public void UpdateShaderValue(DependencyProperty dependencyProperty)
        {
            base.UpdateShaderValue(dependencyProperty);
        }

        public Brush Input
        {
            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        public static readonly DependencyProperty InputProperty =
            ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(MosaicEffect), 0);


        public double Width
        {
            get { return (double)GetValue(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Width.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WidthProperty =
            DependencyProperty.Register("Width",
                typeof(double),
                typeof(MosaicEffect),
                new PropertyMetadata(0.0, PixelShaderConstantCallback(0)));

        public double Height
        {
            get { return (double)GetValue(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Height.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeightProperty =
            DependencyProperty.Register(
                "Height",
                typeof(double),
                typeof(MosaicEffect),
                new PropertyMetadata(0.0, PixelShaderConstantCallback(1)));

        public double Cp
        {
            get { return (double)GetValue(CpProperty); }
            set { SetValue(CpProperty, value); }
        }

        public static readonly DependencyProperty CpProperty =
            DependencyProperty.Register(
                "Cp",
                typeof(double),
                typeof(MosaicEffect),
                new PropertyMetadata(0.0, PixelShaderConstantCallback(2)));
        public double Rp
        {
            get { return (double)GetValue(RpProperty); }
            set { SetValue(RpProperty, value); }
        }

        public static readonly DependencyProperty RpProperty =
            DependencyProperty.Register(
                "Rp",
                typeof(double),
                typeof(MosaicEffect),
                new PropertyMetadata(0.0, PixelShaderConstantCallback(3)));

        public IList<byte> Bytecode
        {
            get { return (IList<byte>)GetValue(BytecodeProperty); }
            set { SetValue(BytecodeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Bytecode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BytecodeProperty =
            DependencyProperty.Register(
                "Bytecode",
                typeof(IList<byte>),
                typeof(MosaicEffect),
                new PropertyMetadata(
                    null,
                    (d, e) =>
                    {
                        var t = d as MosaicEffect;
                        if (t != null)
                        {
                            t.OnBytecodePropertyChanged(e);
                        }
                    }));


        private void OnBytecodePropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            var v = e.NewValue as byte[];
            if (v != null)
            {
                using (var ms = new MemoryStream(v))
                {
                    PixelShader.SetStreamSource(ms);
                }
            }
        }
    }
}
