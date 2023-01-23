using ShaderEffectSample.Effects;
using ShaderEffectSample.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace ShaderEffectSample.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            {
                //Display Blurred image by Code behind
                var imageBlurEffect = new Image();
                imageBlurEffect.Source = new BitmapImage(new Uri(@"/Assets/Fm2MhTmakAAlBhz.jpg", UriKind.Relative));
                this.manualBlurEffect.Content = imageBlurEffect;
                var blurEffect = new BlurEffect();
                blurEffect.Radius = 30;
                imageBlurEffect.Effect = blurEffect;
            }
            {
                //Display Mosaicked image by Code behind
                var imageMosaickedEffect = new Image();
                imageMosaickedEffect.Source = new BitmapImage(new Uri(@"./Assets/Fm2MhTmakAAlBhz.jpg", UriKind.Relative));
                this.manualMosaicEffect.Content = imageMosaickedEffect;
                var mosaickedEffect = new MosaicEffect();
                imageMosaickedEffect.Effect = mosaickedEffect;
                mosaickedEffect.Width = imageMosaickedEffect.Source.Width;
                mosaickedEffect.Height = imageMosaickedEffect.Source.Height;
                mosaickedEffect.Cp = 10;
                mosaickedEffect.Rp = 10;
                mosaickedEffect.Bytecode = new MosaicViewModel().Bytecode.Value;
            }
        }
    }
}
