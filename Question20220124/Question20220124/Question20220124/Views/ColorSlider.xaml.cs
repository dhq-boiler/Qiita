using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Question20220124.Views
{
    /// <summary>
    /// ColorSlider.xaml の相互作用ロジック
    /// </summary>
    public partial class ColorSlider : UserControl
    {
        public ColorSlider()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty BackgroundBitmapSourceProperty = DependencyProperty.Register("BackgroundBitmapSource", typeof(ImageSource), typeof(ColorSlider));

        public ImageSource BackgroundBitmapSource
        {
            get { return (ImageSource)GetValue(BackgroundBitmapSourceProperty); }
            set { SetValue(BackgroundBitmapSourceProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(int), typeof(ColorSlider));

        public int Maximum
        {
            get { return (int)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(int), typeof(ColorSlider));

        public int Minimum
        {
            get { return (int)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(int), typeof(ColorSlider));

        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
    }
}
